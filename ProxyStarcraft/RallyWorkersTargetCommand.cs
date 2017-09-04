using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class RallyWorkersTargetCommand : UnitTargetCommand
    {
        public RallyWorkersTargetCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit, target)
        {
        }
    }
}
