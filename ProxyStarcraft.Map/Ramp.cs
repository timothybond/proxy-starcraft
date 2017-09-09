namespace ProxyStarcraft.Map
{
    public class Ramp : Area
    {
        protected Ramp(int id, Location center) : base(id, center)
        {
        }

        public override bool CanBuild => false;
    }
}
