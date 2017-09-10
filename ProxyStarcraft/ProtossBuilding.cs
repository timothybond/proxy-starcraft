using System;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class ProtossBuilding : Building
    {
        public ProtossBuilding(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.ProtossBuilding == ProtossBuildingType.Unspecified)
            {
                throw new ArgumentException($"Expected a ProtossBuildingType, got '{unitType.ToString()}'.");
            }

            this.ProtossBuildingType = unitType.ProtossBuilding;
        }

        public ProtossBuildingType ProtossBuildingType { get; private set; }

        public override BuildingType BuildingType => this.ProtossBuildingType;

        public override BuildingOrUnitType Type => this.ProtossBuildingType;
    }
}
