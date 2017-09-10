using System;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class TerranBuilding : Building
    {
        public TerranBuilding(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
            var unitType = translator.GetBuildingOrUnitType(unit.UnitType);

            if (unitType.TerranBuilding == TerranBuildingType.Unspecified)
            {
                throw new ArgumentException($"Expected a TerranBuildingType, got '{unitType.ToString()}'.");
            }

            this.TerranBuildingType = unitType.TerranBuilding;
        }

        public TerranBuildingType TerranBuildingType { get; private set; }

        public override BuildingType BuildingType => this.TerranBuildingType;

        public override BuildingOrUnitType Type => this.TerranBuildingType;
    }
}
