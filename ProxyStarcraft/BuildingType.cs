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

            this.TerranBuilding = building;
        }

        public BuildingType(ProtossBuildingType building)
        {
            if (building == ProtossBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossBuildingType - 'Unspecified'.", "building");
            }

            this.ProtossBuilding = building;
        }

        public BuildingType(ZergBuildingType building)
        {
            if (building == ZergBuildingType.Unspecified)
            {
                throw new ArgumentException("Invalid ZergBuildingType - 'Unspecified'.", "building");
            }

            this.ZergBuilding = building;
        }

        public TerranBuildingType TerranBuilding { get; private set; }

        public ProtossBuildingType ProtossBuilding { get; private set; }

        public ZergBuildingType ZergBuilding { get; private set; }

        public static bool operator ==(BuildingType first, BuildingType second)
        {
            // TODO: Express this better, possibly getting rid of the == operator
            // (might be better to just change the way the 'union type' is implemented)
            if (Equals(first, null) && Equals(second, null))
            {
                return true;
            }

            if (Equals(first, null) || Equals(second, null))
            {
                return false;
            }

            return first.TerranBuilding == second.TerranBuilding &&
                   first.ProtossBuilding == second.ProtossBuilding &&
                   first.ZergBuilding == second.ZergBuilding;
        }

        public static bool operator !=(BuildingType first, BuildingType second)
        {
            return !(first == second);
        }
    }
}
