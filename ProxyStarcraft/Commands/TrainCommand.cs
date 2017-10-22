namespace ProxyStarcraft.Commands
{
    public class TrainCommand : NoTargetCommand
    {
        public TrainCommand(Unit builder, UnitType target) : base(builder)
        {
            this.Target = target;
        }
        
        public UnitType Target { get; private set; }
    }
}
