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
        public BuildCommand(Unit unit, TerranBuilding building, int x, int y) : this(unit, x, y)
        {
            if (building == TerranBuilding.Unspecified)
            {
                throw new ArgumentException("Cannot create a build command for 'Unspecified'.", "building");
            }

            this.TerranBuilding = building;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location
        /// (specified as the bottom-left square of the building).
        /// </summary>
        public BuildCommand(Unit unit, ProtossBuilding building, int x, int y) : this(unit, x, y)
        {
            if (building == ProtossBuilding.Unspecified)
            {
                throw new ArgumentException("Cannot create a build command for 'Unspecified'.", "building");

            }
            this.ProtossBuilding = building;
        }

        /// <summary>
        /// Commands a unit to construct the specified building at the given location
        /// (specified as the bottom-left square of the building).
        /// </summary>
        public BuildCommand(Unit unit, ZergBuilding building, int x, int y) : this(unit, x, y)
        {
            if (building == ZergBuilding.Unspecified)
            {
                throw new ArgumentException("Cannot create a build command for 'Unspecified'.", "building");
            }

            this.ZergBuilding = building;
        }

        public Unit Unit { get; private set; }

        public int X { get; set; }

        public int Y { get; set; }

        public TerranBuilding TerranBuilding { get; private set; }

        public ProtossBuilding ProtossBuilding { get; private set; }

        public ZergBuilding ZergBuilding { get; private set; }
    }
}
