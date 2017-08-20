using System;

using ProxyStarcraft.Proto;
using System.Collections.Generic;

namespace ProxyStarcraft
{
    public static class UnitExtensions
    {
        /// <summary>
        /// Gets the edge-to-edge distance between two units. I think this should be the most useful thing in
        /// most situations, but I'm not sure. (For example, if a unit has X range, does that mean when its
        /// center is X units away it can attack or when its edges are?)
        /// </summary>
        public static float GetDistance(this Unit self, Unit other)
        {
            var x = self.Pos.X - other.Pos.X;
            var y = self.Pos.Y - other.Pos.Y;

            var centerToCenter = Math.Sqrt(x * x + y * y);

            return (float)(centerToCenter - self.Radius - other.Radius);
        }

        /// <summary>
        /// Determines which of the specified units is the closest.
        /// In the event of a tie, the earliest one in the enumeration is returned.
        /// Uses GetDistance, and the same question about edge-to-edge distance therefore applies.
        /// </summary>
        public static Unit GetClosest(this Unit self, IEnumerable<Unit> others)
        {
            Unit closest = null;
            var minDistance = 99999f;
            
            foreach (var other in others)
            {
                var distance = self.GetDistance(other);
                if (distance < minDistance)
                {
                    closest = other;
                    minDistance = distance;
                }
            }

            return closest;
        }
    }
}
