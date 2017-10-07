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

            if (!gameState.Units.Any(u => IsBuilderType(u) && !u.IsBuildingSomething && u.IsFinishedBuilding))
            {
                return false;
            }

            if (this.Prerequisite != null && !gameState.Units.Any(u => IsPrerequisiteType(u) && u.IsFinishedBuilding))
            {
                return false;
            }

            return true;
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

            return false;
        }
    }
}
