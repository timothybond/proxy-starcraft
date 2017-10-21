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
            switch (building.Value)
            {
                case TerranBuildingType terranBuilding:
                    return new BuildingOrUnitType(terranBuilding);
                case ProtossBuildingType protossBuilding:
                    return new BuildingOrUnitType(protossBuilding);
                case ZergBuildingType zergBuilding:
                    return new BuildingOrUnitType(zergBuilding);
                default:
                    throw new NotImplementedException();
            }
        }

        public static implicit operator BuildingOrUnitType(UnitType unit)
        {
            switch (unit.Value)
            {
                case TerranUnitType terranUnit:
                    return new BuildingOrUnitType(terranUnit);
                case ProtossUnitType protossUnit:
                    return new BuildingOrUnitType(protossUnit);
                case ZergUnitType zergUnit:
                    return new BuildingOrUnitType(zergUnit);
                default:
                    throw new NotImplementedException();
            }
        }

        public static explicit operator UnitType(BuildingOrUnitType buildingOrUnit)
        {
            switch (buildingOrUnit.Value)
            {
                case TerranUnitType terranUnit:
                    return new UnitType(terranUnit);
                case ProtossUnitType protossUnit:
                    return new UnitType(protossUnit);
                case ZergUnitType zergUnit:
                    return new UnitType(zergUnit);
                default:
                    throw new InvalidOperationException($"Attempted to convert a BuildingOrUnitType to a UnitType, but was BuildingType '{buildingOrUnit}'.");
            }
        }

        public static explicit operator BuildingType(BuildingOrUnitType buildingOrUnit)
        {
            switch (buildingOrUnit.Value)
            {
                case TerranBuildingType terranBuilding:
                    return new BuildingType(terranBuilding);
                case ProtossBuildingType protossBuilding:
                    return new BuildingType(protossBuilding);
                case ZergBuildingType zergBuilding:
                    return new BuildingType(zergBuilding);
                default:
                    throw new InvalidOperationException($"Attempted to convert a BuildingOrUnitType to a UnitType, but was UnitType '{buildingOrUnit}'.");
            }
        }

        public BuildingOrUnitType(TerranBuildingType building)
        {
            this.Value = building;
        }

        public BuildingOrUnitType(ProtossBuildingType building)
        {
            this.Value = building;
        }

        public BuildingOrUnitType(ZergBuildingType building)
        {
            this.Value = building;
        }

        public BuildingOrUnitType(TerranUnitType unit)
        {
            this.Value = unit;
        }

        public BuildingOrUnitType(ProtossUnitType unit)
        {
            this.Value = unit;
        }

        public BuildingOrUnitType(ZergUnitType unit)
        {
            this.Value = unit;
        }

        public object Value { get; private set; }

        public bool IsBuildingType =>
            this.Value is TerranBuildingType || this.Value is ProtossBuildingType || this.Value is ZergBuildingType;

        public bool IsUnitType =>
            this.Value is TerranUnitType || this.Value is ProtossUnitType || this.Value is ZergUnitType;

        public override string ToString()
        {
            return this.Value.ToString();
        }

        public static bool operator == (BuildingOrUnitType first, BuildingOrUnitType second)
        {
            if (first.Value is TerranUnitType firstTerranUnit && second.Value is TerranUnitType secondTerranUnit)
            {
                return firstTerranUnit == secondTerranUnit;
            }
            else if (first.Value is ProtossUnitType firstProtossUnit && second.Value is ProtossUnitType secondProtossUnit)
            {
                return firstProtossUnit == secondProtossUnit;
            }
            else if (first.Value is ZergUnitType firstZergUnit && second.Value is ZergUnitType secondZergUnit)
            {
                return firstZergUnit == secondZergUnit;
            }
            else if (first.Value is TerranBuildingType firstTerranBuilding && second.Value is TerranBuildingType secondTerranBuilding)
            {
                return firstTerranBuilding == secondTerranBuilding;
            }
            else if (first.Value is ProtossBuildingType firstProtossBuilding && second.Value is ProtossBuildingType secondProtossBuilding)
            {
                return firstProtossBuilding == secondProtossBuilding;
            }
            else if (first.Value is ZergBuildingType firstZergBuilding && second.Value is ZergBuildingType secondZergBuilding)
            {
                return firstZergBuilding == secondZergBuilding;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(BuildingOrUnitType first, BuildingOrUnitType second)
        {
            return !(first == second);
        }
    }
}
