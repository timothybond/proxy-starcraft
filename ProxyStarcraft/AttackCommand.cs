namespace ProxyStarcraft
{
    public class AttackCommand : UnitTargetCommand
    {
        public AttackCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit, target)
        {
        }
    }
}
