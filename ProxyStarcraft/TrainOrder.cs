using ProxyStarcraft.Proto;
using System;

namespace ProxyStarcraft
{
    public class TrainOrder : IOrder
    {
        private TrainOrder(Unit builder)
        {
            this.Unit = builder;
        }

        public TrainOrder(Unit builder, TerranUnit unit) : this(builder)
        {
            if (unit == TerranUnit.Unspecified)
            {
                throw new ArgumentException("Cannot create a train order for 'Unspecified'.", "unit");
            }

            this.TerranUnit = unit;
        }

        public TrainOrder(Unit builder, ProtossUnit unit) : this(builder)
        {
            if (unit == ProtossUnit.Unspecified)
            {
                throw new ArgumentException("Cannot create a train order for 'Unspecified'.", "unit");
            }

            this.ProtossUnit = unit;
        }

        public TrainOrder(Unit builder, ZergUnit unit) : this(builder)
        {
            if (unit == ZergUnit.Unspecified)
            {
                throw new ArgumentException("Cannot create a train order for 'Unspecified'.", "unit");
            }

            this.ZergUnit = unit;
        }

        public Unit Unit { get; private set; }

        public TerranUnit TerranUnit { get; private set; }

        public ProtossUnit ProtossUnit { get; private set; }

        public ZergUnit ZergUnit { get; private set; }
    }
}
