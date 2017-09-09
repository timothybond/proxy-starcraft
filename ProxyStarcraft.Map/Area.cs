using System.Collections.Generic;

namespace ProxyStarcraft.Map
{
    public abstract class Area
    {
        protected Area(int id, Location center)
        {
            this.Id = id;
            this.Center = center;
            this.Neighbors = new List<Area>();
        }

        public int Id { get; private set; }

        public Location Center { get; private set; }

        public List<Area> Neighbors { get; private set; }

        public abstract bool CanBuild { get; }
    }
}
