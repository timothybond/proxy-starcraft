using System;
using System.Collections.Generic;

namespace ProxyStarcraft.Map
{
    /// <summary>
    /// An <see cref="Area"/> of uniform height that can be built on.
    /// </summary>
    public class Mesa : Area
    {
        private List<Area> neighbors = new List<Area>();

        public Mesa(int id, IEnumerable<Location> locations, Location center, int height) : base(id, locations, center)
        {
            this.Height = height;
        }

        public int Height { get; private set; }

        public override bool CanBuild => true;

        public override IReadOnlyList<Area> Neighbors => this.neighbors;

        public void AddNeighbor(Area neighbor)
        {
            this.neighbors.Add(neighbor);
        }
    }
}
