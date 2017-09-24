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

            if (!gameState.Units.Any(u => u.Type == this.Builder && !u.IsBuildingSomething && u.Raw.BuildProgress == 1.0))
            {
                return false;
            }

            if (this.Prerequisite != null && !gameState.Units.Any(u => u.Type == this.Prerequisite && u.Raw.BuildProgress == 1.0))
            {
                return false;
            }

            return true;
        }
    }
}
