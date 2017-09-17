namespace ProxyStarcraft
{
    public abstract class Command
    {
        public Command(uint abilityId, Unit unit)
        {
            this.AbilityId = abilityId;
            this.Unit = unit;
        }

        public Unit Unit { get; private set; }

        public uint AbilityId { get; private set; }
    }
}
