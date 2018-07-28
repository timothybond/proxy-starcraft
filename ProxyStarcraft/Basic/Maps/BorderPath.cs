using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft.Basic.Maps
{
    /// <summary>
    /// Helper class for putting together strands of borders. May be useful for other things in the
    /// future as well ("Border" may be a misnomer). Does not allow repeat entries of the same location.
    /// </summary>
    public class BorderPath
    {
        private List<Location> locations;
        private HashSet<Location> locationSet;

        public BorderPath(Location location)
        {
            this.locations = new List<Location>() { location };
            this.locationSet = new HashSet<Location>() { location };
        }

        public BorderPath(IEnumerable<Location> locations)
        {
            this.locations = new List<Location>(locations);
            this.locationSet = new HashSet<Location>(locations);

            for (var i = 1; i < this.locations.Count; i++)
            {
                if (!this.locations[i - 1].IsAdjacentTo(this.locations[i]))
                {
                    throw new ArgumentException("Locations must already be adjacent and in order.");
                }
            }
        }

        public BorderPath Copy()
        {
            return new BorderPath(new List<Location>(this.locations));
        }

        public IReadOnlyList<Location> Locations
        {
            get
            {
                return new List<Location>(this.locations);
            }
        }

        public Location Start
        {
            get
            {
                return this.locations[0];
            }
        }

        public Location End
        {
            get
            {
                return this.locations[this.locations.Count - 1];
            }
        }

        public bool Contains(Location location)
        {
            return this.locationSet.Contains(location);
        }

        public bool CanAdd(Location location)
        {
            return !this.Contains(location) && (this.Start.IsAdjacentTo(location) || this.End.IsAdjacentTo(location));
        }

        public void Add(Location location, Location edge)
        {
            if (this.locations.Count == 0)
            {
                this.locations.Add(location);
                this.locationSet.Add(location);
                return;
            }

            if (this.Contains(location))
            {
                throw new ArgumentException(string.Format("Location {0} already in path.", location));
            }

            if (!location.IsAdjacentTo(edge))
            {
                throw new ArgumentException("Location is not adjacent to specified edge of path.");
            }

            if (this.End == edge)
            {
                this.locations.Add(location);
                this.locationSet.Add(location);
                return;
            }

            if (this.Start == edge)
            {
                this.locations.Insert(0, location);
                this.locationSet.Add(location);
                return;
            }

            throw new ArgumentException(string.Format("Edge {0} is not either end of this path ({1} or {2}).", edge, this.Start, this.End));
        }

        public bool CanAdd(BorderPath path)
        {
            if (this.locations.Any(path.Contains))
            {
                throw new ArgumentException("Paths have overlapping sets of locations.");
            }

            return
                this.Start.IsAdjacentTo(path.Start) ||
                this.End.IsAdjacentTo(path.End) ||
                this.Start.IsAdjacentTo(path.End) ||
                this.End.IsAdjacentTo(path.Start);
        }

        public bool CanAdd(BorderPath path, Location edge, Location otherPathEdge)
        {
            if (this.Start != edge && this.End != edge)
            {
                return false;
            }

            if (path.Start != otherPathEdge && path.End != otherPathEdge)
            {
                return false;
            }

            if (!edge.IsAdjacentTo(otherPathEdge))
            {
                return false;
            }

            return true;
        }

        public void Add(BorderPath path, Location edge, Location otherPathEdge)
        {
            if (this.Start != edge && this.End != edge)
            {
                throw new ArgumentException(string.Format("{0} is not one of this path's edges (either {1} or {2}).", edge, this.Start, this.End));
            }

            if (path.Start != otherPathEdge && path.End != otherPathEdge)
            {
                throw new ArgumentException(string.Format("{0} is not one of the added path's edges (either {1} or {2}).", otherPathEdge, path.Start, path.End));
            }

            if (!edge.IsAdjacentTo(otherPathEdge))
            {
                throw new ArgumentException(string.Format("Specified edges {0} and {1} are not adjacent.", edge, otherPathEdge));
            }

            // If they are start-to-start or end-to-end then reverse the new one,
            // except in the special case where the start and the end of this one
            // are equal (i.e. it has only 1 item)
            if (this.Locations.Count > 1 &&
                (this.Start == edge && path.Start == otherPathEdge) ||
                (this.End == edge && path.End == otherPathEdge))
            {
                path = path.Reverse();
            }

            if (this.End == edge)
            {
                this.locations.AddRange(path.locations);
            }
            else
            {
                this.locations.InsertRange(0, path.locations);
            }

            path.locations.ForEach(l => this.locationSet.Add(l));
        }

        public BorderPath Reverse()
        {
            return new BorderPath(Enumerable.Reverse(this.locations));
        }
    }
}
