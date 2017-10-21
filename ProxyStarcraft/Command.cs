namespace ProxyStarcraft
{
    public abstract class Command
    {
        public Command(Unit unit)
        {
            this.Unit = unit;
        }

        public Unit Unit { get; private set; }
    }
}
