namespace ProxyStarcraft
{
    public class VespeneBuildLocation : IBuildLocation
    {
        public VespeneBuildLocation(Unit vespeneGeyser)
        {
            this.VespeneGeyser = vespeneGeyser;
        }

        public Unit VespeneGeyser { get; private set; }
    }
}
