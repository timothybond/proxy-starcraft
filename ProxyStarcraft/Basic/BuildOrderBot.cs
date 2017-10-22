using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class BuildOrderBot : ChainableBot
    {
        private Race race;
        private List<BuildingOrUnitType> buildOrder = new List<BuildingOrUnitType>();
        private int current = 0;

        public BuildOrderBot(IProductionStrategy productionStrategy, IBot nextBot, Race race) : base(nextBot)
        {
            this.ProductionStrategy = productionStrategy;
        }

        public BuildOrderBot(IProductionStrategy placementStrategy, Race race) : this(placementStrategy, null, race)
        {
        }

        public override Race Race => this.race;
        
        public IProductionStrategy ProductionStrategy { get; private set; }

        public override bool IsFinished => NextUnitToBuild() == null;
        
        public void Add(BuildingOrUnitType target)
        {
            buildOrder.Add(target);
        }

        protected override IReadOnlyList<Command> ActHelper(GameState gameState)
        {
            var nextUnitToBuild = NextUnitToBuild();

            if (nextUnitToBuild == null)
            {
                return new List<Command>();
            }

            var cost = gameState.Translator.GetCost(nextUnitToBuild);

            if (!cost.IsMet(gameState))
            {
                return new List<Command>();
            }

            this.current += 1;

            var builder = gameState.Units.First(u => u.Type == cost.Builder && !u.IsBuildingSomething && u.Raw.BuildProgress == 1.0);

            if (nextUnitToBuild.IsBuildingType)
            {
                var buildingType = (BuildingType)nextUnitToBuild;
                var location = this.ProductionStrategy.GetPlacement(buildingType, gameState);
                return new List<Command> { builder.Build(buildingType, location) };
            }
            else
            {
                return new List<Command> { builder.Train((UnitType)nextUnitToBuild) };
            }
        }

        private BuildingOrUnitType NextUnitToBuild()
        {
            if (this.current >= buildOrder.Count)
            {
                return null;
            }

            return buildOrder[this.current];
        }
    }
}
