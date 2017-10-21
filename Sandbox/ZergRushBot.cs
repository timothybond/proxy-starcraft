using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft;
using ProxyStarcraft.Basic;
using ProxyStarcraft.Map;
using ProxyStarcraft.Proto;

namespace Sandbox
{
    /// <summary>
    /// This object represents an <see cref="IBot"/> that builds a <see cref="ZergBuildingType.SpawningPool"/> as quickly as possible and dumps Zerglings at the opponent.
    /// Very similar to <see cref="BenchmarkBot"/> but Zerg, so it builds secondary <see cref="ZergBuildingType.Hatchery"/> entities for the Zerglings. Sometimes. Unless it breaks.
    /// </summary>
    public class ZergRushBot : IBot
    {
        // Every time there are this many idle soldiers, attack
        private const uint AttackThreshold = 24;

        private const uint MaxWorkersPerMineralDeposit = 2;

        private Dictionary<ulong, List<ulong>> workersByMineralDeposit;

        private IPlacementStrategy placementStrategy = new BasicPlacementStrategy();

        private bool first = true; // shamelessly borrowing some setup from BenchmarkBot
        private float primaryHatcheryX;
        private float primaryHatcheryY;
        private int sleep = 0;

        public Race Race => Race.Zerg;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            var commands = new List<Command>();
            if (sleep > 0)
            {
                sleep--;
                return commands;
            }
            ZergBuilding hatchery = null;
            ZergBuilding pool = null;
            var workers = new List<ZergUnit>();
            var lings = new List<ZergUnit>();
            var idleLarva = new List<ZergUnit>();
            var queens = new List<ZergUnit>();

            foreach (var unit in gameState.Units)
            {
                if (unit is ZergBuilding zergBuilding)
                {
                    if (zergBuilding.ZergBuildingType == ZergBuildingType.Hatchery ||
                        zergBuilding.ZergBuildingType == ZergBuildingType.Lair ||
                        zergBuilding.ZergBuildingType == ZergBuildingType.Hive)
                    {
                        if (zergBuilding.X == primaryHatcheryX || zergBuilding.Y == primaryHatcheryY)
                        {
                            hatchery = zergBuilding;
                        }
                        else
                        {
                            if (first)
                            {
                                hatchery = zergBuilding;
                                primaryHatcheryX = hatchery.X;
                                primaryHatcheryY = hatchery.Y;
                            }
                            // we proooobably don't care about secondary hatcheries, but if we did they'd go here.
                        }
                    }
                    if (zergBuilding.ZergBuildingType == ZergBuildingType.SpawningPool)
                    {
                        pool = zergBuilding;
                    }
                }
                else if (unit is ZergUnit zUnit)
                {
                    if (zUnit.ZergUnitType == ZergUnitType.Drone)
                    {
                        workers.Add(zUnit);
                    }
                    else if (zUnit.ZergUnitType == ZergUnitType.Zergling)
                    {
                        lings.Add(zUnit);
                    }
                    else if (zUnit.ZergUnitType == ZergUnitType.Larva)
                    {
                        idleLarva.Add(zUnit);
                    }
                    else if (zUnit.ZergUnitType == ZergUnitType.Queen)
                    {
                        queens.Add(zUnit);
                    }
                }
            }
            if (hatchery == null)
            {
                // Accept death as inevitable.
                return new List<Command>();

                // TODO: Surrender?
            }
            Deposit closestDeposit = gameState.MapData.Deposits.OrderBy(d => hatchery.GetDistance(d.Center)).First();

            var mineralDeposits = closestDeposit.Resources.Where(u => u.IsMineralDeposit).ToList();

            // First update, ignore the default worker orders, which are to mass on the center mineral deposit
            // (this ends up causing problems since we have to keep track of who is harvesting what ourselves)
            if (first)
            {
                foreach (var worker in workers)
                {
                    worker.Raw.Orders.Clear();
                }

                workersByMineralDeposit = mineralDeposits.ToDictionary(m => m.Raw.Tag, m => new List<ulong>());

                first = false;

                // Make sure workers don't automatically harvest minerals, since we're managing assignments ourselves
                commands.Add(hatchery.RallyWorkers(hatchery.Raw.Pos.X, hatchery.Raw.Pos.Y));
            }
            if (idleLarva.Any())
            {
                BuildOverlord(gameState, idleLarva, commands);
                // Could not find a larva timer in the SC2 API. 
                if (idleLarva.Count >= 3 && !gameState.Units.Any(u => u.Type == ZergUnitType.Cocoon))
                {
                    BuildWorker(gameState, idleLarva, commands);
                }
            }
            if (pool == null)
            {
                TryBuildPool(gameState, workers, commands);
            }
            if (pool?.IsBuilt == true)
            {
                if (idleLarva.Any())
                {
                    if (pool?.IsBuilt == true)
                    {
                        BuildAllLings(gameState, idleLarva, commands);
                    }
                }
                if (!gameState.Units.Any(u => u.Type == ZergUnitType.Queen || u.IsBuilding(ZergUnitType.Queen)))
                {
                    BuildQueen(gameState, hatchery, commands);
                }
            }
            else
            {
                return commands;
            }
            

            // This bot isn't exactly aiming for the stars in terms of the tech tree, so we can get away with this. Revisit if you're shamelessly bastardizing this code.
            if (gameState.Observation.PlayerCommon.Minerals >= 600) // only expand hatcheries if mineral production > zergling capacity.
            {
                BuildHatchery(gameState, workers, closestDeposit, commands);
            }
            
            
            // TODO: Vespene is important for Zerg.

            commands = commands.Take(1).ToList();

            // If we tell a worker to build something, make sure we don't think he's harvesting
            if (commands.Count == 1)
            {
                sleep = 1;
                RemoveWorkerFromHarvestAssignments(commands[0].Unit);
            }

            // No matter what else happens, we can always attack, and we should always set idle workers to harvest minerals
            Attack(gameState, hatchery, lings, commands);
            SetIdleWorkerToHarvest(gameState, workers, mineralDeposits, commands);

            if (queens.Any())
            {
                SpawnLarvaOnSingleHatchery(queens.First(), hatchery, commands);
            }
            
            return commands;
        }



        private void SpawnLarvaOnSingleHatchery(ZergUnit closestQueen, ZergBuilding hatchery, List<Command> commands)
        {
            if (closestQueen.Raw.Energy >= 25)
            {
                //commands.Add(new RallyTargetCommand(251, closestQueen, hatchery)); // The hackiest hack.
            }            
        }

        private void Attack(GameState gameState, ZergBuilding hatchery, List<ZergUnit> soldiers, List<Command> commands)
        {
            // Initially, await X idle soldiers and send them toward the enemy's starting location.
            // Once they're there and have no further orders, send them to attack any sighted enemy unit/structure.
            // Once we run out of those, send them to scout every resource deposit until we find more.
            var enemyStartLocation = gameState.MapData.Raw.StartLocations.OrderByDescending(point => hatchery.GetDistance(point)).First();

            var idleSoldiers = soldiers.Where(s => s.Raw.Orders.Count == 0).ToList();

            if (!soldiers.Any(s => s.GetDistance(enemyStartLocation) < 5f) ||
                gameState.EnemyUnits.Any(e => e.GetDistance(enemyStartLocation) < 10f))
            {
                if (idleSoldiers.Count >= AttackThreshold)
                {
                    foreach (var soldier in idleSoldiers)
                    {
                        commands.Add(soldier.AttackMove(enemyStartLocation.X, enemyStartLocation.Y));
                    }
                }

                return;
            }

            if (gameState.EnemyUnits.Count > 0)
            {
                foreach (var soldier in idleSoldiers)
                {
                    commands.Add(soldier.AttackMove(gameState.EnemyUnits[0].X, gameState.EnemyUnits[0].Y));
                }

                return;
            }

            var unscoutedLocations = gameState.MapData.Deposits.Select(d => d.Center).ToList();

            foreach (var location in unscoutedLocations)
            {
                if (soldiers.Any(s => s.GetDistance(location) < 5f ||
                                 s.Raw.Orders.Any(o => o.TargetWorldSpacePos.GetDistance(location) < 5f)))
                {
                    continue;
                }

                if (idleSoldiers.Count == 0)
                {
                    break;
                }

                commands.Add(idleSoldiers[0].AttackMove(location));
                idleSoldiers.RemoveAt(0);
            }
        }

        private void RemoveWorkerFromHarvestAssignments(ProxyStarcraft.Unit unit)
        {
            foreach (var pair in workersByMineralDeposit)
            {
                if (pair.Value.Contains(unit.Tag))
                {
                    pair.Value.Remove(unit.Tag);
                }
            }
        }

        private void BuildWorker(GameState gameState, List<ZergUnit> idleLarva, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 50 ||
               gameState.Observation.PlayerCommon.FoodUsed >= gameState.Observation.PlayerCommon.FoodCap)
            {
                return;
            }
            commands.Add(idleLarva.First().Train(ZergUnitType.Drone));
        }

        private void BuildQueen(GameState gameState, ZergBuilding hatchery, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 150 ||
               gameState.Observation.PlayerCommon.FoodUsed >= gameState.Observation.PlayerCommon.FoodCap - 2)
            {
                return;
            }
            commands.Add(hatchery.Train(ZergUnitType.Queen));
        }

        private void BuildOverlord(GameState gameState, List<ZergUnit> idleLarva, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 100 || 
                gameState.Observation.PlayerCommon.FoodCap - gameState.Observation.PlayerCommon.FoodUsed >= 2 ||
                    gameState.Units.Any(u => u.IsBuilding(ZergUnitType.Overlord)))
            {
                return;
            }
            commands.Add(idleLarva.First().Train(ZergUnitType.Overlord));
        }

        private void BuildAllLings(GameState gameState, List<ZergUnit> larvae, List<Command> commands)
        {
            var allottableUnits = gameState.Observation.PlayerCommon.FoodCap - gameState.Observation.PlayerCommon.FoodUsed;
            if (gameState.Observation.PlayerCommon.Minerals < 50 ||
                allottableUnits <= 0)
            {
                return;
            }

            for (var i = 0; i < allottableUnits && i < larvae.Count; i++)
            {
                commands.Add(larvae[i].Train(ZergUnitType.Zergling));
            }
        }

        private void SetIdleWorkerToHarvest(
            GameState gameState,
            List<ZergUnit> workers,
            List<ProxyStarcraft.Unit> minerals,
            List<Command> commands)
        {
            var idleWorkers = workers.Where(w => w.Raw.Orders.Count == 0).ToList();
            var mineralsByTag = minerals.ToDictionary(m => m.Raw.Tag);

            while (!IsFullyHarvestingMineralDeposits() && idleWorkers.Count > 0)
            {
                var mineralDeposit = MineralsNeedingWorkers().First();
                var lastIdleWorker = idleWorkers.Last();

                commands.Add(lastIdleWorker.Harvest(mineralsByTag[mineralDeposit]));
                workersByMineralDeposit[mineralDeposit].Add(lastIdleWorker.Raw.Tag);
                idleWorkers.Remove(lastIdleWorker);
            }
        }

        private bool IsFullyHarvestingMineralDeposits()
        {
            return MineralsNeedingWorkers().Count == 0;
        }

        private IReadOnlyList<ulong> MineralsNeedingWorkers()
        {
            return workersByMineralDeposit.Where(pair => pair.Value.Count < MaxWorkersPerMineralDeposit).Select(pair => pair.Key).ToList();
        }

        private void TryBuildPool(GameState gameState, List<ZergUnit> workers, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals > 200)
            {
                Build(gameState, workers, ZergBuildingType.SpawningPool, 200, 0, commands, false);
            }
        }

        private void BuildHatchery(GameState gameState, List<ZergUnit> workers, Deposit mainBaseDeposit, List<Command> commands)
        {
            Build(gameState, workers, ZergBuildingType.Hatchery, 400, 0, commands, true);
        }

        private void Build(GameState gameState, List<ZergUnit> workers, ZergBuildingType building, uint minerals, uint vespene, List<Command> commands, bool allowMultipleInProgress)
        {
            var anotherInstanceBeingBuilt = gameState.RawUnits.Any(b => b.Alliance == Alliance.Self && gameState.Translator.IsUnitOfType(b, building) && b.BuildProgress < 1.0);
            if (!allowMultipleInProgress && anotherInstanceBeingBuilt)
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.Minerals < minerals)
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.Vespene < vespene)
            {
                return;
            }

            var worker = workers.FirstOrDefault();

            if (worker == null)
            {
                // We ran out of workers.
                return;
            }

            commands.Add(GetBuildCommand(worker, building, gameState));
        }

        private BuildCommand GetBuildCommand(ZergUnit unit, ZergBuildingType building, GameState gameState)
        {
            var buildLocation = this.placementStrategy.GetPlacement(building, gameState); // TODO: Stop Hatcheries getting placed on mineral deposits.
            return unit.Build(building, buildLocation);
        }
    }
}
