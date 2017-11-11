using System;

namespace ProxyStarcraft
{
    public struct Vector
    {
        public double X;

        public double Y;

        public double Length
        {
            get
            {
                return Math.Sqrt(this.X * this.X + this.Y * this.Y);
            }
        }

        public Vector ToUnitVector()
        {
            var length = this.Length;
            return new Vector { X = this.X / length, Y = this.Y / length };
        }

        public double DotProduct(LocationOffset other)
        {
            return this.X * other.X + this.Y * other.Y;
        }

        public static implicit operator Vector(LocationOffset offset) => new Vector { X = offset.X, Y = offset.Y };
    }
}
