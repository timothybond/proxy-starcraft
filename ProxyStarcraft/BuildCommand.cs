namespace ProxyStarcraft
{
    public class BuildCommand : Command
    {
        private BuildCommand(Unit unit, IBuildLocation buildLocation) : base(unit)
        {
            this.BuildLocation = buildLocation;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location.
        /// </summary>
        public BuildCommand(Unit unit, BuildingType building, IBuildLocation buildLocation) : this(unit, buildLocation)
        {
            this.Building = building;
        }
        
        public IBuildLocation BuildLocation { get; private set; }

        public BuildingType Building { get; private set; }
    }
}
