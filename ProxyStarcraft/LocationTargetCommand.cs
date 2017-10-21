namespace ProxyStarcraft
{
    public abstract class LocationTargetCommand : Command
    {
        public LocationTargetCommand(Unit unit, float x, float y) : base(unit)
        {
            this.X = x;
            this.Y = y;
        }

        public float X { get; private set; }

        public float Y { get; private set; }
    }
}
