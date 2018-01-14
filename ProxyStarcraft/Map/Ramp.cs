using System.Collections.Generic;

namespace ProxyStarcraft.Maps
{
    /// <summary>
    /// An <see cref="Area"/> that cannot be built on, and that connects two areas of distinct elevations.
    /// </summary>
    public class Ramp : Area
    {
        public Ramp(int id, IEnumerable<Location> locations, Location center, Mesa top, Mesa bottom) : base(id, locations, center)
        {
            this.Top = top;
            this.Bottom = bottom;
        }
        
        public Mesa Top { get; private set; }

        public Mesa Bottom { get; private set; }

        public override bool CanBuild => false;

        public override IReadOnlyList<Area> Neighbors => new List<Area> { this.Top, this.Bottom };
    }
}
