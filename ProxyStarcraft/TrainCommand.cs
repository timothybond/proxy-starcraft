using System;

namespace ProxyStarcraft
{
    public class TrainCommand : NoTargetCommand
    {
        public TrainCommand(Unit builder, TerranUnitType unit, uint abilityId) : base(abilityId, builder)
        {
            if (unit == TerranUnitType.Unspecified)
            {
                throw new ArgumentException("Cannot create a train command for 'Unspecified'.", "unit");
            }

            this.TerranUnit = unit;
        }

        public TrainCommand(Unit builder, ProtossUnitType unit, uint abilityId) : base(abilityId, builder)
        {
            if (unit == ProtossUnitType.Unspecified)
            {
                throw new ArgumentException("Cannot create a train command for 'Unspecified'.", "unit");
            }

            this.ProtossUnit = unit;
        }

        public TrainCommand(Unit builder, ZergUnitType unit, uint abilityId) : base(abilityId, builder)
        {
            if (unit == ZergUnitType.Unspecified)
            {
                throw new ArgumentException("Cannot create a train command for 'Unspecified'.", "unit");
            }

            this.ZergUnit = unit;
        }
        
        public TerranUnitType TerranUnit { get; private set; }

        public ProtossUnitType ProtossUnit { get; private set; }

        public ZergUnitType ZergUnit { get; private set; }
    }
}
