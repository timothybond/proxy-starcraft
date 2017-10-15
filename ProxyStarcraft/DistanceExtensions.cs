using System;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public static class DistanceExtensions
    {
        /// <summary>
        /// Gets the horizontal distance between two points.
        /// </summary>
        public static float GetDistance(this Point2D self, Point2D other)
        {
            var x = self.X - other.X;
            var y = self.Y - other.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Gets the horizontal distance between two points.
        /// </summary>
        public static float GetDistance(this Point2D self, Point other)
        {
            var other2d = new Point2D { X = other.X, Y = other.Y };
            return GetDistance(self, other2d);
        }

        /// <summary>
        /// Gets the horizontal distance between two points.
        /// </summary>
        public static float GetDistance(this Point self, Point2D other)
        {
            var self2d = new Point2D { X = self.X, Y = self.Y };
            return GetDistance(self2d, other);
        }

        /// <summary>
        /// Gets the horizontal distance between two points.
        /// </summary>
        public static float GetDistance(this Point self, Point other)
        {
            var self2d = new Point2D { X = self.X, Y = self.Y };
            var other2d = new Point2D { X = other.X, Y = other.Y };
            return GetDistance(self2d, other2d);
        }
    }
}
