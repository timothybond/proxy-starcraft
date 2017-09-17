using System;

namespace ProxyStarcraft
{
    public class TerranUnit : Unit
    {
        public TerranUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.TerranUnit == TerranUnitType.Unspecified)
            {
                throw new ArgumentException($"Expected a TerranUnitType, got '{unitType.ToString()}'.");
            }

            this.TerranUnitType = unitType.TerranUnit;
        }

        public TerranUnitType TerranUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.TerranUnitType;

        public BuildCommand Build(TerranBuildingType building, int x, int y)
        {
            return base.Build(building, x, y);
        }
    }
}
