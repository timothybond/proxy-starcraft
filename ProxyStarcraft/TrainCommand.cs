using System;

namespace ProxyStarcraft
{
    public class TrainCommand : NoTargetCommand
    {
        public TrainCommand(Unit builder, UnitType target, uint abilityId) : base(abilityId, builder)
        {
            this.Target = target;
        }
        
        public UnitType Target { get; private set; }
    }
}
