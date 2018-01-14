using System.Collections.Generic;

namespace ProxyStarcraft.Maps
{
    public class Deposit
    {
        private List<Unit> resources;

        private Area area;

        public Deposit(Area area, Location center, IEnumerable<Unit> resources)
        {
            this.resources = new List<Unit>(resources);
            this.Area = area;
            this.Center = center;
        }

        public Area Area
        {
            get
            {
                return this.area;
            }
            set
            {
                this.area = value;
                this.area.AddDeposit(this);
            }
        }

        public Location Center { get; private set; }

        public IReadOnlyList<Unit> Resources => this.resources;
    }
}
