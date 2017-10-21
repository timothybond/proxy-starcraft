using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents one of <see cref="TerranBuildingType"/>, <see cref="ProtossBuildingType"/>, or <see cref="ZergBuildingType"/>.
    /// 
    /// Replace if a better way to represent a Union type is determined.
    /// </summary>
    public class BuildingType
    {
        public static implicit operator BuildingType(TerranBuildingType building) => new BuildingType(building);

        public static implicit operator BuildingType(ProtossBuildingType building) => new BuildingType(building);

        public static implicit operator BuildingType(ZergBuildingType building) => new BuildingType(building);

        public BuildingType(TerranBuildingType building)
        {
            if (building == TerranBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid TerranBuildingType - 'Unspecified'.", "building");
            }

            this.Value = building;
        }

        public BuildingType(ProtossBuildingType building)
        {
            if (building == ProtossBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossBuildingType - 'Unspecified'.", "building");
            }

            this.Value = building;
        }

        public BuildingType(ZergBuildingType building)
        {
            if (building == ZergBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ZergBuildingType - 'Unspecified'.", "building");
            }

            this.Value = building;
        }

        public object Value { get; private set; }

        public static bool operator ==(BuildingType first, BuildingType second)
        {
            if (first.Value is TerranBuildingType firstTerranBuilding && second.Value is TerranBuildingType secondTerranBuilding)
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

        public static bool operator !=(BuildingType first, BuildingType second)
        {
            return !(first == second);
        }
    }
}
