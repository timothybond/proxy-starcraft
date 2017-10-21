namespace ProxyStarcraft
{
    public class UseUnitTargetAbilityCommand : UnitTargetCommand
    {
        public UseUnitTargetAbilityCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit, target)
        {
        }
    }
}
