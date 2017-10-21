namespace ProxyStarcraft
{
    public abstract class UnitTargetCommand : Command
    {
        public UnitTargetCommand(Unit unit, Unit target) : base(unit)
        {
            this.Target = target;
        }

        public Unit Target { get; private set; }
    }
}
