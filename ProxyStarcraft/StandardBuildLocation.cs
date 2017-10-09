namespace ProxyStarcraft
{
    public class StandardBuildLocation : IBuildLocation
    {
        public StandardBuildLocation(Location location)
        {
            this.Location = location;
        }

        public Location Location { get; private set; }
    }
}
