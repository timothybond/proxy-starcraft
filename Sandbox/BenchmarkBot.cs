using System;
using System.Collections.Generic;
using System.Linq;

using ProxyStarcraft;
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

        public IReadOnlyList<ICommand> Act(GameState gameState)
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
            var commands = new List<ICommand>();

            var controlledUnits = gameState.Units.Where(u => u.Alliance == Alliance.Self).ToList();

            Unit commandCenter = null;
            var workers = new List<Unit>();
            var mineralDeposits = new List<Unit>();
            var soldiers = new List<Unit>();
            var soldierProducers = new List<Unit>();

            foreach (var unit in gameState.Units)
            {
                if (unit.IsMineralDeposit())
                {
                    mineralDeposits.Add(unit);
                }

                if (unit.Alliance == Alliance.Self)
                {
                    if (gameState.Translator.IsUnitOfType(unit, TerranBuilding.CommandCenter))
                    {
                        commandCenter = unit;
                    }
                    else if (gameState.Translator.IsUnitOfType(unit, TerranBuilding.Barracks))
                    {
                        soldierProducers.Add(unit);
                    }
                    else if (gameState.Translator.IsUnitOfType(unit, TerranUnit.SCV))
                    {
                        workers.Add(unit);
                    }
                    else if (gameState.Translator.IsUnitOfType(unit, TerranUnit.Marine))
                    {
                        soldiers.Add(unit);
                    }
                }
            }
            
            if (commandCenter == null)
            {
                // Accept death as inevitable.
                return new List<ICommand>();
            }

            // Each possible behavior is implemented as a function that adds a command to the list
            // if the situation is appropriate. We will then execute whichever command was added first.

            // This would otherwise be called twice
            var mineralsNeedingWorkers = MineralDepositsNeedingWorkers(mineralDeposits, commandCenter);

            if (mineralsNeedingWorkers.Count > 0)
            {
                BuildWorkerForMineralDeposit(gameState, mineralDeposits, commandCenter, commands);
            }
            
            BuildMarine(gameState, soldierProducers, commands);
            BuildSupplyDepot(gameState, workers, commands);
            BuildBarracks(gameState, workers, commands);

            commands = commands.Take(1).ToList();

            // No matter what else happens, we can always attack, and we should always set idle workers to harvest minerals
            Attack(gameState, commandCenter, soldiers, commands);
            SetIdleWorkerToHarvest(gameState, mineralsNeedingWorkers, workers, commands);

            return commands;
        }

        private void BuildWorkerForMineralDeposit(GameState gameState, List<Unit> mineralDepositsNeedingWorkers, Unit commandCenter, List<ICommand> commands)
        {
            if (gameState.Translator.IsBuildingSomething(commandCenter))
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.Minerals >= 50 &&
                gameState.Observation.PlayerCommon.FoodUsed < gameState.Observation.PlayerCommon.FoodCap)
            {
                commands.Add(new TrainCommand(commandCenter, TerranUnit.SCV));
            }
        }

        private void BuildMarine(GameState gameState, List<Unit> soldierProducers, List<ICommand> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < 50 ||
                gameState.Observation.PlayerCommon.FoodUsed >= gameState.Observation.PlayerCommon.FoodCap)
            {
                return;
            }

            foreach (var producer in soldierProducers)
            {
                if (!gameState.Translator.IsBuildingSomething(producer))
                {
                    commands.Add(new TrainCommand(producer, TerranUnit.Marine));
                    return;
                }
            }
        }

        private void Attack(GameState gameState, Unit commandCenter, List<Unit> soldiers, List<ICommand> commands)
        {
            var idleSoldiers = soldiers.Where(s => s.Orders.Count == 0).ToList();

            if (idleSoldiers.Count >= AttackThreshold)
            {
                var enemyStartLocation = gameState.MapData.Raw.StartLocations.OrderByDescending(point => commandCenter.GetDistance(point)).First();

                foreach (var soldier in idleSoldiers)
                {
                    commands.Add(new AttackMoveCommand(soldier, enemyStartLocation.X, enemyStartLocation.Y));
                }
            }
        }

        private void BuildSupplyDepot(GameState gameState, List<Unit> workers, List<ICommand> commands)
        {
            // Don't queue up multiple Supply Depots
            if (workers.Any(worker => gameState.Translator.IsBuilding(worker, TerranBuilding.SupplyDepot)))
            {
                return;
            }

            if (gameState.Observation.PlayerCommon.FoodUsed >= gameState.Observation.PlayerCommon.FoodCap &&
                gameState.Observation.PlayerCommon.FoodCap < 200)
            {
                Build(gameState, workers, TerranBuilding.SupplyDepot, 100, commands);
            }
        }

        private void BuildBarracks(GameState gameState, List<Unit> workers, List<ICommand> commands)
        {
            Build(gameState, workers, TerranBuilding.Barracks, 150, commands);
        }

        private void Build(GameState gameState, List<Unit> workers, TerranBuilding building, uint minerals, List<ICommand> commands)
        {
            if (gameState.Observation.PlayerCommon.Minerals < minerals)
            {
                return;
            }

            var worker = workers.FirstOrDefault(w => !gameState.Translator.IsBuildingSomething(w));

            if (worker == null)
            {
                // We ran out of workers or they're all building something.
                return;
            }
            
            commands.Add(GetBuildCommand(worker, building, gameState));
        }

        private void SetIdleWorkerToHarvest(
            GameState gameState,
            IReadOnlyList<Unit> mineralDepositsNeedingWorkers,
            List<Unit> workers,
            List<ICommand> commands)
        {
            // To simplify our job, we only do this to one worker per frame, so we can't
            // accidentally stack up too many on one mineral deposit (in theory)
            if (gameState.Observation.PlayerCommon.IdleWorkerCount == 0)
            {
                return;
            }

            if (mineralDepositsNeedingWorkers.Count == 0)
            {
                return;
            }

            commands.Add(new HarvestCommand(workers.First(w => w.Orders.Count == 0), mineralDepositsNeedingWorkers.First()));
        }

        private IReadOnlyList<Unit> MineralDepositsNeedingWorkers(List<Unit> mineralDeposits, Unit commandCenter)
        {
            var mineralsAtBase = mineralDeposits.Where(m => commandCenter.GetDistance(m) < 10f).ToList();

            return mineralsAtBase.Where(m => m.AssignedHarvesters < MaxWorkersPerMineralDeposit).ToList();
        }
        
        private static BuildCommand GetBuildCommand(Unit unit, Building building, GameState gameState)
        {
            // We're going to make some dumb assumptions here:
            // 1. We'd like to build this building very near where this unit currently is
            // 2. As long as we don't block anything, it doesn't matter where it goes
            var size = gameState.Translator.GetBuildingSize(building);

            var locations = new HashSet<Location> { new Location { X = (int)Math.Round(unit.Pos.X), Y = (int)Math.Round(unit.Pos.Y) } };
            var pastLocations = new HashSet<Location>();
            var nextLocations = new HashSet<Location>();

            // I think this is basically a breadth-first search of map locations
            while (locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    if (gameState.MapData.CanBuild(size, location.X, location.Y))
                    {
                        return new BuildCommand(unit, building, location.X, location.Y);
                    }

                    var adjacentLocations = AdjacentLocations(location, gameState.MapData.Size);

                    foreach (var adjacentLocation in adjacentLocations)
                    {
                        if (!pastLocations.Contains(adjacentLocation) && !locations.Contains(adjacentLocation) && gameState.MapData.CanTraverse(adjacentLocation))
                        {
                            nextLocations.Add(adjacentLocation);
                        }
                    }

                    pastLocations.Add(location);
                }

                locations = nextLocations;
                nextLocations = new HashSet<Location>();
            }

            throw new InvalidOperationException("Cannot find placement location anywhere on map.");
        }
        
        private static List<Location> AdjacentLocations(Location location, Size2DI mapSize)
        {
            var results = new List<Location>();

            var xVals = new List<int> { location.X - 1, location.X, location.X + 1 };
            xVals.Remove(-1);
            xVals.Remove(mapSize.X);

            var yVals = new List<int> { location.Y - 1, location.Y, location.Y + 1 };
            yVals.Remove(-1);
            yVals.Remove(mapSize.Y);

            foreach (var x in xVals)
            {
                foreach (var y in yVals)
                {
                    if (x != location.X || y != location.Y)
                    {
                        results.Add(new Location { X = x, Y = y });
                    }
                }
            }

            return results;
        }
    }
}
