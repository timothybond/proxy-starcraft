using System.Linq;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents everything needed to create a unit or structure, including prerequisites.
    /// </summary>
    public class UnitCost
    {
        public UnitCost(uint minerals, uint vespene, float supply, BuildingOrUnitType builder, BuildingOrUnitType prerequisite)
        {
            this.Minerals = minerals;
            this.Vespene = vespene;
            this.Supply = supply;
            this.Builder = builder;
            this.Prerequisite = prerequisite;
        }

        public uint Minerals { get; private set; }

        public uint Vespene { get; private set; }

        public float Supply { get; private set; }

        public BuildingOrUnitType Builder { get; private set; }

        public BuildingOrUnitType Prerequisite { get; private set; }

        /// <summary>
        /// Determines whether the cost/requirements are currently met and therefore the unit in question can be built.
        /// </summary>
        public bool IsMet(GameState gameState)
        {
            if (gameState.Response.Observation.PlayerCommon.Minerals < this.Minerals)
            {
                return false;
            }

            if (gameState.Response.Observation.PlayerCommon.Vespene < this.Vespene)
            {
                return false;
            }

            if (gameState.Response.Observation.PlayerCommon.FoodCap < gameState.Response.Observation.PlayerCommon.FoodUsed + this.Supply)
            {
                return false;
            }
            
            if (this.Prerequisite != null && !gameState.Units.Any(u => IsPrerequisiteType(u) && u.IsBuilt))
            {
                // This is handled separately in the GetBuilder(...) logic.
                // TODO: Find cleaner way to do this.
                if (this.Prerequisite != TerranBuildingType.TechLab &&
                    this.Prerequisite != TerranBuildingType.BarracksTechLab &&
                    this.Prerequisite != TerranBuildingType.FactoryTechLab &&
                    this.Prerequisite != TerranBuildingType.StarportTechLab)
                {
                    return false;
                }
            }

            if (GetBuilder(gameState) == null)
            {
                return false;
            }

            return true;
        }

        public Unit GetBuilder(GameState gameState)
        {
            if (this.Prerequisite == TerranBuildingType.TechLab ||
                this.Prerequisite == TerranBuildingType.BarracksTechLab ||
                this.Prerequisite == TerranBuildingType.FactoryTechLab ||
                this.Prerequisite == TerranBuildingType.StarportTechLab)
            {
                var builder = gameState.Units.FirstOrDefault(
                    u =>
                    IsUnitOfType(u, this.Builder) && !u.IsBuildingSomething && u.IsBuilt &&
                    u.Raw.AddOnTag != 0 &&
                    IsUnitOfType(gameState.Units.Single(a => a.Tag == u.Raw.AddOnTag), TerranBuildingType.TechLab));
                return builder;
            }
            
            return gameState.Units.FirstOrDefault(u => IsUnitOfType(u, this.Builder) && !u.IsBuildingSomething && u.IsBuilt);
        }

        private bool IsBuilderType(Unit unit)
        {
            return IsUnitOfType(unit, this.Builder);
        }

        private bool IsPrerequisiteType(Unit unit)
        {
            return IsUnitOfType(unit, this.Prerequisite);
        }

        private bool IsUnitOfType(Unit unit, BuildingOrUnitType type)
        {
            if (unit.Type == type)
            {
                return true;
            }

            // Handle buildings that get upgraded to 'different types' but still count
            // TODO: Handle this better, elsewhere
            if (type == TerranBuildingType.CommandCenter &&
                (unit.Type == TerranBuildingType.PlanetaryFortress ||
                 unit.Type == TerranBuildingType.OrbitalCommand))
            {
                return true;
            }

            if (type == ZergBuildingType.Hatchery &&
                (unit.Type == ZergBuildingType.Lair ||
                 unit.Type == ZergBuildingType.Hive))
            {
                return true;
            }

            if (type == ZergBuildingType.Lair &&
                unit.Type == ZergBuildingType.Hive)
            {
                return true;
            }

            if (type == ZergBuildingType.HydraliskDen &&
                unit.Type == ZergBuildingType.LurkerDen)
            {
                return true;
            }

            if (type == ZergBuildingType.Spire &&
                unit.Type == ZergBuildingType.GreaterSpire)
            {
                return true;
            }

            if (type == TerranBuildingType.TechLab &&
                (unit.Type == TerranBuildingType.BarracksTechLab ||
                 unit.Type == TerranBuildingType.FactoryTechLab ||
                 unit.Type == TerranBuildingType.StarportTechLab))
            {
                return true;
            }

            if (type == TerranBuildingType.Reactor &&
                (unit.Type == TerranBuildingType.BarracksReactor ||
                 unit.Type == TerranBuildingType.FactoryReactor ||
                 unit.Type == TerranBuildingType.StarportReactor))
            {
                return true;
            }

            return false;
        }
    }
}
