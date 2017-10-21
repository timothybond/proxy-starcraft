using System;

namespace ProxyStarcraft
{
    public class ProtossBuilding : Building
    {
        public ProtossBuilding(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.Value is ProtossBuildingType protossBuilding)
            {
                this.ProtossBuildingType = protossBuilding;
            }
            else
            {
                throw new ArgumentException($"Expected a ProtossBuildingType, got '{unitType.ToString()}'.");
            }
        }

        public ProtossBuildingType ProtossBuildingType { get; private set; }

        public override BuildingType BuildingType => this.ProtossBuildingType;

        public override BuildingOrUnitType Type => this.ProtossBuildingType;
    }
}
