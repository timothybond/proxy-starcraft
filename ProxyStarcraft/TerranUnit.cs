using System;

namespace ProxyStarcraft
{
    public class TerranUnit : Unit
    {
        public TerranUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.Value is TerranUnitType terranUnit)
            {
                this.TerranUnitType = terranUnit;
            }
            else
            {
                throw new ArgumentException($"Expected a TerranUnitType, got '{unitType.ToString()}'.");
            }
        }

        public TerranUnitType TerranUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.TerranUnitType;

        public BuildCommand Build(TerranBuildingType building, IBuildLocation location)
        {
            return base.Build(building, location);
        }
    }
}
