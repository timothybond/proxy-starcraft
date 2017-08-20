using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class AttackOrder : IOrder
    {
        public AttackOrder(Unit unit, Unit target)
        {
            Unit = unit;
            Target = target;
        }

        public Unit Unit { get; private set; }

        public Unit Target { get; private set; }
    }
}
