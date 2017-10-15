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

        public double DistanceTo(Location location)
        {
            var x = this.X - location.X;
            var y = this.Y - location.Y;

            return Math.Sqrt(x * x + y * y);
        }

        public IReadOnlyList<Location> AdjacentLocations(Size2DI mapSize)
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
    }
}
