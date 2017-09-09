using System;

namespace ProxyStarcraft.Map
{
    public class Mesa : Area
    {
        protected Mesa(int id, Location center, int height) : base(id, center)
        {
            this.Height = height;
        }

        public override bool CanBuild => throw new NotImplementedException();

        public int Height { get; private set; }
    }
}
