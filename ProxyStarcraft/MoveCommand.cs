using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class MoveCommand : LocationTargetCommand
    {
        public MoveCommand(uint abilityId, Unit unit, float x, float y) : base(abilityId, unit, x, y)
        {
        }
    }
}
