using System.Collections.Generic;

namespace ProxyStarcraft.Map
{
    /// <summary>
    /// Represents a contiguous portion of the map that is somehow logically distinct from other areas, including its neighbors.
    /// </summary>
    public abstract class Area
    {
        protected Area(int id, IEnumerable<Location> locations, Location center)
        {
            this.Id = id;
            this.Locations = new List<Location>(locations);
            this.Center = center;
        }

        /// <summary>
        /// Unique identifier of the area.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Location that represents the average of all of the locations in this Area.
        /// 
        /// If the average location is not actually a part of this area, then this property
        /// will be whichever contained location is closest to the average.
        /// </summary>
        public Location Center { get; private set; }

        /// <summary>
        /// All of the locations that make up this Area.
        /// </summary>
        public IReadOnlyList<Location> Locations { get; private set; }

        /// <summary>
        /// Areas that directly connect to this Area.
        /// </summary>
        public abstract IReadOnlyList<Area> Neighbors { get; }

        /// <summary>
        /// Whether it is possible to build in this Area.
        /// </summary>
        public abstract bool CanBuild { get; }
    }
}
