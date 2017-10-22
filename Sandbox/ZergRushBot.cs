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
        private const int AttackThreshold = 16;

        private BasicEconomyBot economyBot = new BasicEconomyBot() { AutoBuildWorkers = true };
        private BasicMilitaryBot militaryBot = new BasicMilitaryBot(AttackThreshold);
        private IPlacementStrategy placementStrategy = new BasicPlacementStrategy();

        private float primaryHatcheryX = -1.0f;
        private float primaryHatcheryY = -1.0f;
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
            var secondaryHatcheries = new List<ZergBuilding>();

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
                            if (primaryHatcheryX == -1.0f && primaryHatcheryY == -1.0f)
                            {
                                hatchery = zergBuilding;
                                primaryHatcheryX = hatchery.X;
                                primaryHatcheryY = hatchery.Y;
                            }
                            secondaryHatcheries.Add(zergBuilding);
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
            var hatcheries = secondaryHatcheries.ToList();
            hatcheries.Add(hatchery);
            var queensByHatchery = this.ClosestQueenByHatchery(hatcheries, queens.ToList());
            
            if (idleLarva.Any())
            {
                BuildOverlord(gameState, idleLarva, commands);
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
                if (hatcheries.Count() > queens.Count() && !gameState.Units.Any(u => u.IsBuilding(ZergUnitType.Queen)))
                {
                    BuildQueen(gameState, hatchery, commands);
                }
            }

            // This bot isn't exactly aiming for the stars in terms of the tech tree, so we can get away with this. Revisit if you're shamelessly bastardizing this code.
            // only expand hatcheries if mineral production >> zergling production. Also only build one because we don't expand.
            if (gameState.Observation.PlayerCommon.Minerals >= 1500) 
            {
                BuildHatchery(gameState, workers, commands);
            }
                        
            // TODO: Vespene is important for Zerg.

            commands = commands.Take(1).ToList();

            // If we tell a worker to build something, make sure we don't think he's harvesting
            if (commands.Count == 1)
            {
                sleep = 1;
                economyBot.RemoveWorkerFromHarvestAssignments(commands[0].Unit);
            }
            foreach (var hatcheryQueenPair in queensByHatchery)
            {
                hatcheryQueenPair.Value.SpawnLarva(hatcheryQueenPair.Key, commands);
            }
            economyBot.AutoBuildWorkers = !(gameState.Observation.PlayerCommon.FoodCap - gameState.Observation.PlayerCommon.FoodUsed < 3);

            commands.AddRange(militaryBot.Act(gameState));
            commands.AddRange(economyBot.Act(gameState));

            return commands;
        }

        private Dictionary<ZergBuilding, ZergUnit> ClosestQueenByHatchery(List<ZergBuilding> hatcheries, List<ZergUnit> queens)
        {
            var results = new Dictionary<ZergBuilding, ZergUnit>();
            if (!queens.Any())
            {
                return results;
            }
            foreach (var item in hatcheries)
            {
                var closestQueen = (ZergUnit)item.GetClosest(queens);
                queens.Remove(closestQueen);
                results.Add(item, closestQueen);
                if (!queens.Any())
                {
                    return results;
                }
            }
            return results;
        }

        private void BuildQueen(GameState gameState, ZergBuilding hatchery, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 150 ||
               gameState.Observation.PlayerCommon.FoodUsed > gameState.Observation.PlayerCommon.FoodCap - 2)
            {
                return;
            }
            commands.Add(hatchery.Train(ZergUnitType.Queen));
        }

        private void BuildOverlord(GameState gameState, List<ZergUnit> idleLarva, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 100 || 
                gameState.Observation.PlayerCommon.FoodCap - gameState.Observation.PlayerCommon.FoodUsed >= 3 ||
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

        private void TryBuildPool(GameState gameState, List<ZergUnit> workers, List<Command> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals > 200)
            {
                Build(gameState, workers, ZergBuildingType.SpawningPool, 200, 0, commands, false);
            }
        }

        private void BuildHatchery(GameState gameState, List<ZergUnit> workers, List<Command> commands)
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
