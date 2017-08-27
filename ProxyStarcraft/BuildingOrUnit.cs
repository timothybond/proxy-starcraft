using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents one of TerranBuilding, ProtossBuilding, ZergBuilding, TerranUnit, ProtossUnit, or ZergUnit.
    /// 
    /// Replace if a better way to represent a Union type is determined.
    /// </summary>
    public class BuildingOrUnit
    {
        public static implicit operator BuildingOrUnit(TerranBuilding building) => new BuildingOrUnit(building);

        public static implicit operator BuildingOrUnit(ProtossBuilding building) => new BuildingOrUnit(building);

        public static implicit operator BuildingOrUnit(ZergBuilding building) => new BuildingOrUnit(building);

        public static implicit operator BuildingOrUnit(TerranUnit unit) => new BuildingOrUnit(unit);

        public static implicit operator BuildingOrUnit(ProtossUnit unit) => new BuildingOrUnit(unit);

        public static implicit operator BuildingOrUnit(ZergUnit unit) => new BuildingOrUnit(unit);

        public BuildingOrUnit(TerranBuilding building)
        {
            if (building == TerranBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid TerranBuilding type - 'Unspecified'.", "building");
            }

            this.TerranBuilding = building;
        }

        public BuildingOrUnit(ProtossBuilding building)
        {
            if (building == ProtossBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossBuilding type - 'Unspecified'.", "building");
            }

            this.ProtossBuilding = building;
        }

        public BuildingOrUnit(ZergBuilding building)
        {
            if (building == ZergBuilding.Unspecified)
            {
                throw new ArgumentException("Invalid ZergBuilding type - 'Unspecified'.", "building");
            }

            this.ZergBuilding = building;
        }

        public BuildingOrUnit(TerranUnit unit)
        {
            if (unit == TerranUnit.Unspecified)
            {
                throw new ArgumentException("Invalid TerranUnit type - 'Unspecified'.", "building");
            }

            this.TerranUnit = unit;
        }

        public BuildingOrUnit(ProtossUnit unit)
        {
            if (unit == ProtossUnit.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossUnit type - 'Unspecified'.", "building");
            }

            this.ProtossUnit = unit;
        }

        public BuildingOrUnit(ZergUnit unit)
        {
            if (unit == ZergUnit.Unspecified)
            {
                throw new ArgumentException("Invalid ZergUnit type - 'Unspecified'.", "building");
            }

            this.ZergUnit = unit;
        }

        public TerranBuilding TerranBuilding { get; private set; }

        public ProtossBuilding ProtossBuilding { get; private set; }

        public ZergBuilding ZergBuilding { get; private set; }

        public TerranUnit TerranUnit { get; private set; }

        public ProtossUnit ProtossUnit { get; private set; }

        public ZergUnit ZergUnit { get; private set; }
    }
}
