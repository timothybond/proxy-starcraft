namespace ProxyStarcraft
{
    public class BuildCommand : Command
    {
        private BuildCommand(Unit unit, int x, int y, uint abilityId) : base(abilityId, unit)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location
        /// (specified as the bottom-left square of the building).
        /// </summary>
        public BuildCommand(Unit unit, BuildingType building, int x, int y, uint abilityId) : this(unit, x, y, abilityId)
        {
            this.Building = building;
        }
        
        public int X { get; set; }

        public int Y { get; set; }

        public BuildingType Building { get; private set; }
    }
}
