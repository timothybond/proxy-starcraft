using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents one <see cref="TerranUnitType"/>, <see cref="ProtossUnitType"/>, <see cref="ZergUnitType"/>,
    /// <see cref="TerranBuildingType"/>, <see cref="ProtossBuildingType"/>, or <see cref="ZergBuildingType"/>.
    /// 
    /// Replace if a better way to represent a Union type is determined.
    /// </summary>
    public class BuildingOrUnitType
    {
        public static implicit operator BuildingOrUnitType(TerranBuildingType building) => new BuildingOrUnitType(building);

        public static implicit operator BuildingOrUnitType(ProtossBuildingType building) => new BuildingOrUnitType(building);

        public static implicit operator BuildingOrUnitType(ZergBuildingType building) => new BuildingOrUnitType(building);

        public static implicit operator BuildingOrUnitType(TerranUnitType unit) => new BuildingOrUnitType(unit);

        public static implicit operator BuildingOrUnitType(ProtossUnitType unit) => new BuildingOrUnitType(unit);

        public static implicit operator BuildingOrUnitType(ZergUnitType unit) => new BuildingOrUnitType(unit);

        public static implicit operator BuildingOrUnitType(BuildingType building)
        {
            if (building.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return building.TerranBuilding;
            }
            else if (building.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return building.ProtossBuilding;
            }
            else if (building.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return building.ZergBuilding;
            }

            throw new ArgumentException("BuildingType was 'Unspecified'.");
        }

        public static implicit operator BuildingOrUnitType(UnitType unit)
        {
            if (unit.TerranUnit != TerranUnitType.Unspecified)
            {
                return unit.TerranUnit;
            }
            else if (unit.ProtossUnit != ProtossUnitType.Unspecified)
            {
                return unit.ProtossUnit;
            }
            else if (unit.ZergUnit != ZergUnitType.Unspecified)
            {
                return unit.ZergUnit;
            }

            throw new ArgumentException("UnitType was 'Unspecified'.");
        }

        public BuildingOrUnitType(TerranBuildingType building)
        {
            if (building == TerranBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid TerranBuildingType - 'Unspecified'.", "building");
            }

            this.TerranBuilding = building;
        }

        public BuildingOrUnitType(ProtossBuildingType building)
        {
            if (building == ProtossBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossBuildingType - 'Unspecified'.", "building");
            }

            this.ProtossBuilding = building;
        }

        public BuildingOrUnitType(ZergBuildingType building)
        {
            if (building == ZergBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ZergBuildingType - 'Unspecified'.", "building");
            }

            this.ZergBuilding = building;
        }

        public BuildingOrUnitType(TerranUnitType unit)
        {
            if (unit == TerranUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid TerranUnitType - 'Unspecified'.", "building");
            }

            this.TerranUnit = unit;
        }

        public BuildingOrUnitType(ProtossUnitType unit)
        {
            if (unit == ProtossUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossUnitType - 'Unspecified'.", "building");
            }

            this.ProtossUnit = unit;
        }

        public BuildingOrUnitType(ZergUnitType unit)
        {
            if (unit == ZergUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid ZergUnitType - 'Unspecified'.", "building");
            }

            this.ZergUnit = unit;
        }

        public TerranBuildingType TerranBuilding { get; private set; }

        public ProtossBuildingType ProtossBuilding { get; private set; }

        public ZergBuildingType ZergBuilding { get; private set; }

        public TerranUnitType TerranUnit { get; private set; }

        public ProtossUnitType ProtossUnit { get; private set; }

        public ZergUnitType ZergUnit { get; private set; }

        public override string ToString()
        {
            if (this.TerranUnit != TerranUnitType.Unspecified)
            {
                return this.TerranUnit.ToString();
            }
            else if (this.ProtossUnit != ProtossUnitType.Unspecified)
            {
                return this.ProtossUnit.ToString();
            }
            else if (this.ZergUnit != ZergUnitType.Unspecified)
            {
                return this.ZergUnit.ToString();
            }
            else if (this.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return this.TerranBuilding.ToString();
            }
            else if (this.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return this.ProtossBuilding.ToString();
            }
            else if (this.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return this.ZergBuilding.ToString();
            }

            return "Unspecified";
        }
    }
}
