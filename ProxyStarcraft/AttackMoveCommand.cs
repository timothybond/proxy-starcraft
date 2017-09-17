namespace ProxyStarcraft
{
    public class AttackMoveCommand : LocationTargetCommand
    {
        public AttackMoveCommand(uint abilityId, Unit unit, float x, float y) : base(abilityId, unit, x, y)
        {
        }
    }
}
