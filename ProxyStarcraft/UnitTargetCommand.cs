namespace ProxyStarcraft
{
    public abstract class UnitTargetCommand : Command
    {
        public UnitTargetCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit)
        {
            this.Target = target;
        }

        public Unit Target { get; private set; }
    }
}
