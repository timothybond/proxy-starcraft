using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class RallyTargetCommand : ICommand
    {
        public RallyTargetCommand(Unit unit, Unit target)
        {
            Unit = unit;
            Target = target;
        }

        public Unit Unit { get; private set; }

        public Unit Target { get; private set; }
    }
}
