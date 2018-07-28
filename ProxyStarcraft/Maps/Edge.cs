using System.Collections.Generic;

namespace ProxyStarcraft.Maps
{
    /// <summary>
    /// An <see cref="Area"/> that isn't buildable but also isn't a ramp between two areas.
    /// 
    /// So far I have mostly seen this as little bits of space that seem like they are
    /// mistakes in the pathing grid or just single-space areas around the edges of
    /// <see cref="Mesa"/>s, hence the name. Alternately this could be a ramp that
    /// connects to more than one <see cref="Mesa"/>.
    /// </summary>
    public class Edge : Area
    {
        private List<Area> neighbors;

        public Edge(int id, IEnumerable<Location> locations, Location center, IEnumerable<Area> neighbors) : base(id, locations, center)
        {
            this.neighbors = new List<Area>(neighbors);
        }

        public override IReadOnlyList<Area> Neighbors => this.neighbors;

        public override bool CanBuild => false;
    }
}
