using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class NoTargetCommand : Command
    {
        public NoTargetCommand(uint abilityId, Unit unit) : base(abilityId, unit)
        {
        }
    }
}
