namespace ProxyStarcraft
{
    public class BuildCommand : Command
    {
        private BuildCommand(Unit unit, IBuildLocation buildLocation, uint abilityId) : base(abilityId, unit)
        {
            this.BuildLocation = buildLocation;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location.
        /// </summary>
        public BuildCommand(Unit unit, BuildingType building, IBuildLocation buildLocation, uint abilityId) : this(unit, buildLocation, abilityId)
        {
            this.Building = building;
        }
        
        public IBuildLocation BuildLocation { get; private set; }

        public BuildingType Building { get; private set; }
    }
}
