using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class RallyTargetCommand : UnitTargetCommand
    {
        public RallyTargetCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit, target)
        {
        }
    }
}
