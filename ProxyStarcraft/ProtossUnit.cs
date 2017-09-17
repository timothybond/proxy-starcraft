using System;

namespace ProxyStarcraft
{
    public class ProtossUnit : Unit
    {
        public ProtossUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.ProtossUnit == ProtossUnitType.Unspecified)
            {
                throw new ArgumentException($"Expected a ProtossUnitType, got '{unitType.ToString()}'.");
            }

            this.ProtossUnitType = unitType.ProtossUnit;
        }

        public ProtossUnitType ProtossUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.ProtossUnitType;

        public BuildCommand Build(ProtossBuildingType building, int x, int y)
        {
            return base.Build(building, x, y);
        }
    }
}
