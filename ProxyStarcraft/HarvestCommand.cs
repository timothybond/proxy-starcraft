using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class HarvestCommand : ICommand
    {
        public HarvestCommand(Unit unit, Unit target)
        {
            this.Unit = unit;
            this.Target = target;
        }

        public Unit Unit { get; private set; }

        public Unit Target { get; private set; }
    }
}
