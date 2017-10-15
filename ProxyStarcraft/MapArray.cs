using System;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents a stream of data about the map, that can be accessed via (X, Y) coordinates or Location instances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MapArray<T>
    {
        // Raw data array. These come from the API and are literally bitmaps,
        // with bytes given from left to right and from top to bottom.
        private T[] data;

        // Needed to translate array positions into coordinates.
        private Size2DI size;

        public MapArray(T[] data, Size2DI size)
        {
            this.size = size;
            this.data = data;
        }

        public MapArray(Size2DI size)
        {
            this.size = size;
            this.data = new T[size.X * size.Y];
        }

        public MapArray(MapArray<T> other)
        {
            this.size = new Size2DI(other.size);
            this.data = (T[])other.data.Clone();
        }

        public T[] Data => (T[])this.data.Clone();

        public Size2DI Size => this.size;

        public T this[int x, int y]
        {
            get
            {
                return data[IndexOf(x, y)];
            }
            set
            {
                data[IndexOf(x, y)] = value;
            }
        }

        public T this[Location location]
        {
            get
            {
                return this[location.X, location.Y];
            }
            set
            {
                this[location.X, location.Y] = value;
            }
        }

        private int IndexOf(int x, int y)
        {
            if (x < 0 || y < 0 || x >= this.size.X || y >= this.size.Y)
            {
                throw new IndexOutOfRangeException();
            }

            var row = this.size.Y - 1 - y;
            return row * this.size.X + x;
        }
    }
}
