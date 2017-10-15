using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft;
using ProxyStarcraft.Basic;
using ProxyStarcraft.Map;
using ProxyStarcraft.Proto;

namespace Sandbox
{
    /// <summary>
    /// A simple bot used to benchmark against more complex strategies.
    /// Builds as many of its basic military unit as possible and attacks.
    /// </summary>
    public class BenchmarkBot : IBot
    {
        // Every time there are this many idle soldiers, attack
        private const uint AttackThreshold = 10;

        private const uint MaxWorkersPerMineralDeposit = 2;

        private Dictionary<ulong, List<ulong>> workersByMineralDeposit;

        private IPlacementStrategy placementStrategy = new BasicPlacementStrategy();

        private bool first = true;

        // I'm not entirely sure units are getting updated with their commands in a timely fashion,
        // so I'm going to avoid issuing any commands within one step of the last command set
        private int sleep = 0;

        public Race Race => Race.Terran;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            /* Detailed strategy:
             * 
             * 1. If there are mineral deposits near a base and not being fully mined, supply is not maxed, the base is not currently building anything, and minerals are available, build a worker.
             * 2. If minerals and supply are available, and there is a Barracks or equivalent not building a basic military unit, start building one.
             * 3. If supply is maxed out, build a Supply Depot or equivalent.
             * 4. If minerals are available, build a Barracks or equivalent.
             * 
             * No expansion. Once you hit the threshold of military units in existence, attack-move your opponent's base.
             * 
             * Implement for Terran only initially (no Creep/Pylon placement concerns).
             * 
             * Implement for maps with only two starting locations, so we don't scout.
             */
            var commands = new List<Command>();

            var controlledUnits = gameState.RawUnits.Where(u => u.Alliance == Alliance.Self).ToList();

            TerranBuilding commandCenter = null;
            var workers = new List<TerranUnit>();
            var soldiers = new List<TerranUnit>();
            var soldierProducers = new List<TerranBuilding>();

            if (sleep > 0)
            {
                sleep -= 1;
                return commands;
            }
            
            foreach (var unit in gameState.Units)
            {
                if (unit is TerranBuilding terranBuilding)
                {
                    if (terranBuilding.TerranBuildingType == TerranBuildingType.CommandCenter ||
                        terranBuilding.TerranBuildingType == TerranBuildingType.OrbitalCommand ||
                        terranBuilding.TerranBuildingType == TerranBuildingType.PlanetaryFortress)
                    {
                        commandCenter = terranBuilding;
                    }
                    else if (terranBuilding.TerranBuildingType == TerranBuildingType.Barracks)
                    {
                        soldierProducers.Add(terranBuilding);
                    }
                }
                else if (unit is TerranUnit terranUnit)
                {
                    if (terranUnit.TerranUnitType == TerranUnitType.SCV)
                    {
                        workers.Add(terranUnit);
                    }
                    else if (terranUnit.TerranUnitType == TerranUnitType.Marine)
                    {
                        soldiers.Add(terranUnit);
                    }
                }
            }

            if (commandCenter == null)
            {
                // Accept death as inevitable.
                return new List<Command>();

                // TODO: Surrender?
            }

            Deposit closestDeposit = gameState.MapData.Deposits.OrderBy(d => commandCenter.GetDistance(d.Center)).First();
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
            }
            
            // Each possible behavior is implemented as a function that adds a command to the list
            // if the situation is appropriate. We will then execute whichever command was added first.
            BuildWorker(gameState, commandCenter, commands);
            BuildMarine(gameState, soldierProducers, commands);
            BuildSupplyDepot(gameState, workers, soldierProducers, commands);
            BuildBarracks(gameState, workers, commands);

            commands = commands.Take(1).ToList();

            // If we tell a worker to build something, make sure we don't think he's harvesting
            if (commands.Count == 1)
            {
                sleep = 1;
                RemoveWorkerFromHarvestAssignments(commands[0].Unit);
            }

            // No matter what else happens, we can always attack, and we should always set idle workers to harvest minerals
            Attack(gameState, commandCenter, soldiers, commands);
            SetIdleWorkerToHarvest(gameState, workers, mineralDeposits, commands);

            if (first)
            {
                first = false;

                // Make sure workers don't automatically harvest minerals, since we're managing assignments ourselves
                commands.Add(commandCenter.RallyWorkers(commandCenter.Raw.Pos.X, commandCenter.Raw.Pos.Y));
            }
            
            return commands;
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

        private void BuildWorker(GameState gameState, TerranBuilding commandCenter, List<Command> commands)
        {
            if (commandCenter.IsBuildingSomething)
            {
                return;
            }

            if (workersByMineralDeposit.All(pair => pair.Value.Count >= 2))
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.Minerals >= 50 &&
                gameState.Observation.PlayerCommon.FoodUsed < gameState.Observation.PlayerCommon.FoodCap)
            {
                commands.Add(commandCenter.Train(TerranUnitType.SCV));
            }
        }

        private void BuildMarine(GameState gameState, List<TerranBuilding> soldierProducers, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 50 ||
                gameState.Observation.PlayerCommon.FoodUsed >= gameState.Observation.PlayerCommon.FoodCap)
            {
                return;
            }

            foreach (var producer in soldierProducers)
            {
                if (!producer.IsBuildingSomething && producer.Raw.BuildProgress == 1.0)
                {
                    commands.Add(producer.Train(TerranUnitType.Marine));
                    return;
                }
            }
        }

        private void Attack(GameState gameState, TerranBuilding commandCenter, List<TerranUnit> soldiers, List<Command> commands)
        {
            // Initially, await X idle soldiers and send them toward the enemy's starting location.
            // Once they're there and have no further orders, send them to attack any sighted enemy unit/structure.
            // Once we run out of those, send them to scout every resource deposit until we find more.
            var enemyStartLocation = gameState.MapData.Raw.StartLocations.OrderByDescending(point => commandCenter.GetDistance(point)).First();

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

        private void BuildSupplyDepot(GameState gameState, List<TerranUnit> workers, List<TerranBuilding> soldierProducers, List<Command> commands)
        {
            // Keep spare supply equal to what we could use, which is one worker from the Command Center and
            // one Marine for each Barracks
            if (gameState.Observation.PlayerCommon.FoodUsed + 1 + soldierProducers.Count >= gameState.Observation.PlayerCommon.FoodCap &&
                gameState.Observation.PlayerCommon.FoodCap < 200)
            {
                Build(gameState, workers, TerranBuildingType.SupplyDepot, 100, commands, false);
            }
        }

        private void BuildBarracks(GameState gameState, List<TerranUnit> workers, List<Command> commands)
        {
            // Can't build a Barracks if you don't have a Supply Depot first
            if (gameState.RawUnits.Any(
                unit =>
                    unit.Alliance == Alliance.Self &&
                    gameState.Translator.IsUnitOfType(unit, TerranBuildingType.SupplyDepot) &&
                    unit.BuildProgress == 1.0))
            {
                Build(gameState, workers, TerranBuildingType.Barracks, 150, commands, true);
            }
        }

        private void Build(GameState gameState, List<TerranUnit> workers, TerranBuildingType building, uint minerals, List<Command> commands, bool allowMultipleInProgress)
        {
            if (!allowMultipleInProgress && workers.Any(w => w.IsBuilding(building)))
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.Minerals < minerals)
            {
                return;
            }

            var worker = workers.FirstOrDefault(w => !w.IsBuildingSomething);

            if (worker == null)
            {
                // We ran out of workers or they're all building something.
                return;
            }
            
            commands.Add(GetBuildCommand(worker, building, gameState));
        }

        private void SetIdleWorkerToHarvest(
            GameState gameState,
            List<TerranUnit> workers,
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
        
        private BuildCommand GetBuildCommand(TerranUnit unit, TerranBuildingType building, GameState gameState)
        {
            var buildLocation = this.placementStrategy.GetPlacement(building, gameState);
            return unit.Build(building, buildLocation);
        }
    }
}
