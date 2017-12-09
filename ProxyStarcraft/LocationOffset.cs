using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents the distance between two map locations, essentially as an integer vector.
    /// </summary>
    public struct LocationOffset
    {
        public int X;

        public int Y;

        public int DotProduct(LocationOffset other)
        {
            return this.X * other.X + this.Y * other.Y;
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(this.X * this.X + this.Y * this.Y);
            }
        }

        public static explicit operator LocationOffset(Vector vector) =>
            new LocationOffset { X = (int)Math.Round(vector.X), Y = (int)Math.Round(vector.Y) };

        public static LocationOffset operator +(LocationOffset offset, LocationOffset other)
        {
            return new LocationOffset { X = offset.X + other.X, Y = offset.Y + other.Y };
        }

        public static LocationOffset operator -(LocationOffset offset, LocationOffset other)
        {
            return new LocationOffset { X = offset.X - other.X, Y = offset.Y - other.Y };
        }

        public static LocationOffset operator *(LocationOffset offset, int ratio)
        {
            return new LocationOffset { X = offset.X * ratio, Y = offset.Y * ratio };
        }

        public static LocationOffset operator /(LocationOffset offset, int ratio)
        {
            return new LocationOffset { X = offset.X / ratio, Y = offset.Y / ratio };
        }
    }
}
