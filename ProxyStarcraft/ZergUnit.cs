using System;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class ZergUnit : Unit2
    {
        public ZergUnit(Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.ZergUnit == ZergUnitType.Unspecified)
            {
                throw new ArgumentException($"Expected a ZergUnitType, got '{unitType.ToString()}'.");
            }

            this.ZergUnitType = unitType.ZergUnit;
        }

        public ZergUnitType ZergUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.ZergUnitType;

        public BuildCommand Build(ZergBuildingType building, int x, int y)
        {
            return base.Build(building, x, y);
        }
    }
}
