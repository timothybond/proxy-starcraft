namespace ProxyStarcraft
{
    public class AttackMoveCommand : LocationTargetCommand
    {
        public AttackMoveCommand(Unit unit, float x, float y) : base(unit, x, y)
        {
        }
    }
}
