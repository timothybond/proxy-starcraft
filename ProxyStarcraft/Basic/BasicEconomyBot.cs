using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    // TODO: Add support for multiple bases
    // TODO: Add logic for workers that get destroyed
    public class BasicEconomyBot : IBot
    {
        private const uint MaxWorkersPerMineralDeposit = 2;
        private const int MaxWorkersPerVespeneGeyser = 3;

        private Dictionary<ulong, List<ulong>> workersByMineralDeposit = new Dictionary<ulong, List<ulong>>();

        private Dictionary<ulong, List<ulong>> workersByVespeneGeyser = new Dictionary<ulong, List<ulong>>();

        private bool first = true;

        public bool AutoBuildWorkers { get; set; }

        public Race Race => Race.NoRace;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            var commands = new List<Command>();
            
            var mainBases = new List<Building>();
            var vespeneBuildings = new List<Building>();
            var workers = new List<Unit>();

            foreach (var unit in gameState.Units)
            {
                if (unit is Building building)
                {
                    if (building.IsMainBase)
                    {
                        mainBases.Add(building);
                    }
                    else if (building.Type == TerranBuildingType.Refinery ||
                             building.Type == ProtossBuildingType.Assimilator ||
                             building.Type == ZergBuildingType.Extractor)
                    {
                        if (building.IsBuilt)
                        { 
                            vespeneBuildings.Add(building);
                        }
                    }
                }
                else if (unit.IsWorker)
                {
                    workers.Add(unit);
                }
            }

            // Ignore empty vespene-harvesting buildings or those from bases that were destroyed or relocated
            vespeneBuildings.RemoveAll(v => !mainBases.Any(b => b.GetDistance(v) < 20f) || v.Raw.VespeneContents == 0);

            var deposits = gameState.MapData.GetControlledDeposits(mainBases);
            var mineralDeposits = new List<Unit>();

            foreach (var deposit in deposits)
            {
                var closestBase = mainBases.SingleOrDefault(b => b.GetDistance(deposit.Center) < 10f);

                if (closestBase?.IsBuilt == true)
                {
                    mineralDeposits.AddRange(deposit.Resources.Where(r => r.IsMineralDeposit));
                }
            }
            
            // First update, ignore the default worker orders, which are to mass on the center mineral deposit
            // (this ends up causing problems since we have to keep track of who is harvesting what ourselves)
            if (first)
            {
                foreach (var worker in workers)
                {
                    worker.Raw.Orders.Clear();
                }
            }

            foreach (var mineralDeposit in mineralDeposits)
            {
                if (!workersByMineralDeposit.ContainsKey(mineralDeposit.Tag))
                {
                    workersByMineralDeposit.Add(mineralDeposit.Tag, new List<ulong>());
                }
            }

            foreach (var vespeneBuilding in vespeneBuildings)
            {
                if (!workersByVespeneGeyser.ContainsKey(vespeneBuilding.Tag))
                {
                    workersByVespeneGeyser.Add(vespeneBuilding.Tag, new List<ulong>());
                }

                if (workersByVespeneGeyser[vespeneBuilding.Tag].Count == 0)
                {
                    // SCVs start off harvesting from the Refinery they have just built, so this should catch them
                    var existingWorkers = workers.Where(w => w.Raw.Orders.Count > 0 && w.Raw.Orders[0].TargetUnitTag == vespeneBuilding.Tag)
                                                 .Select(w => w.Tag).ToList();
                    
                    foreach (var existingWorker in existingWorkers)
                    {
                        AssignWorkerToVespene(existingWorker, vespeneBuilding.Tag);
                    }
                }
            }

            if (mainBases.Count == 0)
            {
                // Accept death as inevitable.
                return new List<Command>();

                // TODO: Surrender?
            }

            if (this.AutoBuildWorkers && (!IsFullyHarvestingMineralDeposits() || !IsFullyHarvestingVespeneGeysers()))
            {
                var cost = gameState.Translator.GetCost(TerranUnitType.SCV);
                if (cost.IsMet(gameState))
                {
                    var builder = cost.GetBuilder(gameState);

                    // TODO: Add 'BuildWorker' function?
                    if (builder is TerranBuilding commandCenter)
                    {
                        commands.Add(commandCenter.Train(TerranUnitType.SCV));
                    }
                    else if (builder is ZergUnit larvae)
                    {
                        commands.Add(larvae.Train(ZergUnitType.Drone));
                    }
                    else if (builder is ProtossBuilding nexus)
                    {
                        commands.Add(nexus.Train(ProtossUnitType.Probe));
                    }
                }
            }

            CheckForKilledWorkers(workers);
            SetIdleWorkersToHarvest(gameState, workers, mineralDeposits, vespeneBuildings, commands);

            if (first)
            {
                first = false;

                // Make sure workers don't automatically harvest minerals, since we're managing assignments ourselves
                foreach (var mainBase in mainBases)
                {
                    commands.Add(mainBase.RallyWorkers(mainBase.Raw.Pos.X, mainBase.Raw.Pos.Y));
                }
            }

            return commands;
        }
        
        /// <summary>
        /// Ensures the bot realizes that the unit in question is no longer harvesting.
        /// </summary>
        public void RemoveWorkerFromHarvestAssignments(Unit unit)
        {
            foreach (var pair in workersByMineralDeposit)
            {
                if (pair.Value.Contains(unit.Tag))
                {
                    pair.Value.Remove(unit.Tag);
                }
            }

            foreach (var pair in workersByVespeneGeyser)
            {
                if (pair.Value.Contains(unit.Tag))
                {
                    pair.Value.Remove(unit.Tag);
                }
            }
        }

        private void CheckForKilledWorkers(List<Unit> workers)
        {
            var activeHarvesters = workersByMineralDeposit.SelectMany(w => w.Value).Concat(workersByVespeneGeyser.SelectMany(w => w.Value)).ToList();

            foreach (var worker in workers)
            {
                activeHarvesters.Remove(worker.Tag);
            }

            foreach (var harvester in activeHarvesters)
            {
                RemoveWorkerFromHarvestAssignments(harvester);
            }
        }

        /// <summary>
        /// Ensures the bot realizes that the unit in question is no longer harvesting.
        /// </summary>
        private void RemoveWorkerFromHarvestAssignments(ulong tag)
        {
            foreach (var pair in workersByMineralDeposit)
            {
                if (pair.Value.Contains(tag))
                {
                    pair.Value.Remove(tag);
                }
            }

            foreach (var pair in workersByVespeneGeyser)
            {
                if (pair.Value.Contains(tag))
                {
                    pair.Value.Remove(tag);
                }
            }
        }

        private void SetIdleWorkersToHarvest(
            GameState gameState,
            List<Unit> workers,
            List<Unit> minerals,
            List<Building> vespeneBuildings,
            List<Command> commands)
        {
            var idleWorkers = workers.Where(w => w.Raw.Orders.Count == 0).ToList();
            var mineralsByTag = minerals.ToDictionary(m => m.Tag);
            var vespeneBuildingsByTag = vespeneBuildings.ToDictionary(vb => vb.Tag);

            while (!IsFullyHarvestingMineralDeposits() && idleWorkers.Count > 0)
            {
                var mineralDeposit = MineralsNeedingWorkers().First();
                var lastIdleWorker = idleWorkers.Last();

                commands.Add(lastIdleWorker.Harvest(mineralsByTag[mineralDeposit]));
                AssignWorkerToMinerals(lastIdleWorker.Tag, mineralDeposit);
                idleWorkers.Remove(lastIdleWorker);
            }

            while (!IsFullyHarvestingVespeneGeysers() && idleWorkers.Count > 0)
            {
                var vespeneBuilding = VespeneBuildingsNeedingWorkers().First();
                var lastIdleWorker = idleWorkers.Last();

                commands.Add(lastIdleWorker.Harvest(vespeneBuildingsByTag[vespeneBuilding]));
                AssignWorkerToVespene(lastIdleWorker.Tag, vespeneBuilding);
                idleWorkers.Remove(lastIdleWorker);
            }
        }

        private void AssignWorkerToMinerals(ulong tag, ulong mineralDepositTag)
        {
            RemoveWorkerFromHarvestAssignments(tag);
            workersByMineralDeposit[mineralDepositTag].Add(tag);
        }

        private void AssignWorkerToVespene(ulong tag, ulong vespeneStructureTag)
        {
            RemoveWorkerFromHarvestAssignments(tag);
            workersByVespeneGeyser[vespeneStructureTag].Add(tag);
        }

        private bool IsFullyHarvestingMineralDeposits()
        {
            return MineralsNeedingWorkers().Count == 0;
        }

        private bool IsFullyHarvestingVespeneGeysers()
        {
            return VespeneBuildingsNeedingWorkers().Count == 0;
        }

        private IReadOnlyList<ulong> MineralsNeedingWorkers()
        {
            return workersByMineralDeposit.Where(pair => pair.Value.Count < MaxWorkersPerMineralDeposit).Select(pair => pair.Key).ToList();
        }

        private IReadOnlyList<ulong> VespeneBuildingsNeedingWorkers()
        {
            return workersByVespeneGeyser.Where(pair => pair.Value.Count < MaxWorkersPerVespeneGeyser).Select(pair => pair.Key).ToList();
        }
    }
}
