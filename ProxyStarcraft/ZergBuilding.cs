using System;

namespace ProxyStarcraft
{
    public class ZergBuilding : Building
    {
        public ZergBuilding(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.Value is ZergBuildingType zergBuilding)
            {
                this.ZergBuildingType = zergBuilding;
            }
            else
            {
                throw new ArgumentException($"Expected a ZergBuildingType, got '{unitType}'.");
            }
        }

        public ZergBuildingType ZergBuildingType { get; private set; }

        public override BuildingOrUnitType Type => this.ZergBuildingType;

        public override BuildingType BuildingType => this.ZergBuildingType;
    }
}
