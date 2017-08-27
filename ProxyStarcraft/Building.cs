using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents one of TerranBuilding, ProtossBuilding, or ZergBuilding.
    /// 
    /// Replace if a better way to represent a Union type is determined.
    /// </summary>
    public class Building
    {
        public static implicit operator Building(TerranBuilding building) => new Building(building);

        public static implicit operator Building(ProtossBuilding building) => new Building(building);

        public static implicit operator Building(ZergBuilding building) => new Building(building);

        public Building(TerranBuilding building)
        {
            if (building == TerranBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid TerranBuilding type - 'Unspecified'.", "building");
            }

            this.TerranBuilding = building;
        }

        public Building(ProtossBuilding building)
        {
            if (building == ProtossBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossBuilding type - 'Unspecified'.", "building");
            }

            this.ProtossBuilding = building;
        }

        public Building(ZergBuilding building)
        {
            if (building == ZergBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid ZergBuilding type - 'Unspecified'.", "building");
            }

            this.ZergBuilding = building;
        }

        public TerranBuilding TerranBuilding { get; private set; }

        public ProtossBuilding ProtossBuilding { get; private set; }

        public ZergBuilding ZergBuilding { get; private set; }
    }
}
