namespace ProxyStarcraft
{
    public class UseTargettedAbilityCommand : UnitTargetCommand
    {
        public UseTargettedAbilityCommand(uint abilityId, Unit unit, Unit target) : base(abilityId, unit, target)
        {
        }
    }
}
