
using System;
using System.Collections.Generic;

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
    }
}
