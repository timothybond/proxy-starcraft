using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class RallyLocationCommand : LocationTargetCommand
    {
        public RallyLocationCommand(uint abilityId, Unit unit, float x, float y) : base(abilityId, unit, x, y)
        {
        }
    }
}
