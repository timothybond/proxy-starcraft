using System;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class ZergBuilding : Building
    {
        public ZergBuilding(Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.ZergBuilding == ZergBuildingType.Unspecified)
            {
                throw new ArgumentException($"Expected a ZergBuildingType, got '{unitType.ToString()}'.");
            }

            this.ZergBuildingType = unitType.ZergBuilding;
        }

        public ZergBuildingType ZergBuildingType { get; private set; }

        public override BuildingOrUnitType Type => this.ZergBuildingType;

        public override BuildingType BuildingType => this.ZergBuildingType;
    }
}
