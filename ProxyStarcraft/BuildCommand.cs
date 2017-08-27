using ProxyStarcraft.Proto;
using System;

namespace ProxyStarcraft
{
    public class BuildCommand : ICommand
    {
        private BuildCommand(Unit unit, int x, int y)
        {
            this.Unit = unit;
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location
        /// (specified as the bottom-left square of the building).
        /// </summary>
        public BuildCommand(Unit unit, Building building, int x, int y) : this(unit, x, y)
        {
            this.Building = building;
        }
        
        public Unit Unit { get; private set; }

        public int X { get; set; }

        public int Y { get; set; }

        public Building Building { get; private set; }
    }
}
