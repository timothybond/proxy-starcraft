using System;

namespace ProxyStarcraft
{
    public class ZergUnit : Unit
    {
        public ZergUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.Value is ZergUnitType zergUnit)
            {
                this.ZergUnitType = zergUnit;
            }
            else
            {
                throw new ArgumentException($"Expected a ZergUnitType, got '{unitType.ToString()}'.");
            }
        }

        public ZergUnitType ZergUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.ZergUnitType;

        public BuildCommand Build(ZergBuildingType building, IBuildLocation location)
        {
            return base.Build(building, location);
        }
    }
}
