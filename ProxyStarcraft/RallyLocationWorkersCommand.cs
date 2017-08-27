using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class RallyWorkersLocationCommand : ICommand
    {
        public RallyWorkersLocationCommand(Unit unit, float x, float y)
        {
            Unit = unit;
            X = x;
            Y = y;
        }

        public Unit Unit { get; private set; }

        public float X { get; private set; }

        public float Y { get; private set; }
    }
}
