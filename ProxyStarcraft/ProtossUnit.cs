using System;

namespace ProxyStarcraft
{
    public class ProtossUnit : Unit
    {
        public ProtossUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.Value is ProtossUnitType protossUnit)
            {
                this.ProtossUnitType = protossUnit;
            }
            else
            {
                throw new ArgumentException($"Expected a ProtossUnitType, got '{unitType.ToString()}'.");
            }
        }

        public ProtossUnitType ProtossUnitType { get; private set; }

        public override BuildingOrUnitType Type => this.ProtossUnitType;

        public BuildCommand Build(ProtossBuildingType building, IBuildLocation location)
        {
            return base.Build(building, location);
        }
    }
}
