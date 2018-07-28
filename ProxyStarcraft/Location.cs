using System;
using System.Collections.Generic;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents a square on the map (i.e., in terms of the grid that's visible when placing buildings).
    /// </summary>
    public struct Location
    {
        public int X;

        public int Y;

        public Location GetClosest(IReadOnlyList<Location> locations)
        {
            if (locations.Count == 0)
            {
                throw new ArgumentException("Cannot supply an empty list of locations.", "locations");
            }

            Location closest = locations[0];
            var closestDistance = DistanceTo(closest);

            for (var i = 1; i < locations.Count; i++)
            {
                var distance = DistanceTo(locations[i]);
                if (distance < closestDistance)
                {
                    closest = locations[i];
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public Location GetFurthest(IReadOnlyList<Location> locations)
        {
            if (locations.Count == 0)
            {
                throw new ArgumentException("Cannot supply an empty list of locations.", "locations");
            }

            Location furthest = locations[0];
            var furthestDistance = DistanceTo(furthest);

            for (var i = 1; i < locations.Count; i++)
            {
                var distance = DistanceTo(locations[i]);
                if (distance > furthestDistance)
                {
                    furthest = locations[i];
                    furthestDistance = distance;
                }
            }

            return furthest;
        }

        public override string ToString()
        {
            return "(" + this.X + ", " + this.Y + ")";
        }

        public override int GetHashCode()
        {
            // I believe the maximum map size is 256x256, so this should be safe
            return this.Y * 4096 + this.X;
        }

        public override bool Equals(object obj)
        {
            return obj is Location l && this.X == l.X && this.Y == l.Y;
        }

        public double DistanceTo(Location location)
        {
            var x = this.X - location.X;
            var y = this.Y - location.Y;

            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Determines whether the given location is adjacent to this one (including diagonals).
        /// </summary>
        /// <param name="other">The location to check against.</param>
        /// <returns><c>true</c> if they are no more than one space apart in any direction,
        /// <c>false</c> if not or if they are the same location.</returns>
        public bool IsAdjacentTo(Location other)
        {
            return IsAdjacentTo(other, true);
        }

        /// <summary>
        /// Determines whether the given location is adjacent to this one, optionally including diagonals.
        /// </summary>
        /// <param name="other">The location to check against.</param>
        /// <param name="includeDiagonals">Whether to consider diagonals adjacent</param>
        /// <returns><c>true</c> if they are no more than one space apart in any direction,
        /// <c>false</c> if not or if they are the same location.</returns>
        public bool IsAdjacentTo(Location other, bool includeDiagonals)
        {
            if (this.Equals(other))
            {
                return false;
            }

            var deltaX = this.X - other.X;
            var deltaY = this.Y - other.Y;

            if (!includeDiagonals)
            {
                if (deltaX != 0 && deltaY != 0)
                {
                    return false;
                }
            }

            return deltaX >= -1 && deltaX <= 1 && deltaY >= -1 && deltaY <= 1;
        }

        public IReadOnlyList<Location> AdjacentLocations(Size2DI mapSize)
        {
            return AdjacentLocations(mapSize, true);
        }

        public IReadOnlyList<Location> AdjacentLocations(Size2DI mapSize, bool includeDiagonals)
        {
            var results = new List<Location>();

            var xVals = new List<int> { this.X - 1, this.X, this.X + 1 };
            xVals.Remove(-1);
            xVals.Remove(mapSize.X);

            var yVals = new List<int> { this.Y - 1, this.Y, this.Y + 1 };
            yVals.Remove(-1);
            yVals.Remove(mapSize.Y);

            foreach (var x in xVals)
            {
                foreach (var y in yVals)
                {
                    if (!includeDiagonals && x != this.X && y != this.Y)
                    {
                        continue;
                    }

                    if (x != this.X || y != this.Y)
                    {
                        results.Add(new Location { X = x, Y = y });
                    }
                }
            }

            return results;
        }

        public static implicit operator Point2D(Location location) => new Point2D { X = location.X + 0.5f, Y = location.Y + 0.5f };

        public static Location operator +(Location location, LocationOffset offset)
        {
            return new Location { X = location.X + offset.X, Y = location.Y + offset.Y };
        }

        public static LocationOffset operator -(Location location, Location other)
        {
            return new LocationOffset { X = location.X - other.X, Y = location.Y - other.Y };
        }

        public static bool operator ==(Location location, Location other)
        {
            return location.Equals(other);
        }

        public static bool operator !=(Location location, Location other)
        {
            return !location.Equals(other);
        }
    }
}
