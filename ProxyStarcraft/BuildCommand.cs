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
        public BuildCommand(Unit unit, TerranBuilding building, int x, int y) : this(unit, x, y)
        {
            if (building == TerranBuilding.Unspecified)
            {
                throw new ArgumentException("Cannot create a build command for 'Unspecified'.", "building");
            }

            this.TerranBuilding = building;
        }

        public BuildCommand(Unit unit, ProtossBuilding building, int x, int y) : this(unit, x, y)
        {
            if (building == ProtossBuilding.Unspecified)
            {
                throw new ArgumentException("Cannot create a build command for 'Unspecified'.", "building");

            }
            this.ProtossBuilding = building;
        }

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
