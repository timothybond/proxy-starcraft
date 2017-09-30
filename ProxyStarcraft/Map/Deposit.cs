using System.Collections.Generic;

namespace ProxyStarcraft.Map
{
    public class Deposit
    {
        private List<Unit> resources;

        public Deposit(Area area, Location center, IEnumerable<Unit> resources)
        {
            this.resources = new List<Unit>(resources);
            this.Area = area;
            this.Center = center;
        }

        public Area Area { get; private set; }

        public Location Center { get; private set; }

        public IReadOnlyList<Unit> Resources => this.resources;
    }
}
