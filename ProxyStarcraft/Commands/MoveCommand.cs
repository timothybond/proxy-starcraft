namespace ProxyStarcraft.Commands
{
    public class MoveCommand : LocationTargetCommand
    {
        public MoveCommand(Unit unit, float x, float y) : base(unit, x, y)
        {
        }
    }
}
