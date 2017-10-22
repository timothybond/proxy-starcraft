namespace ProxyStarcraft
{
    public class UseUnitTargetSpecialAbilityCommand : UnitTargetCommand
    {
        public SpecialAbilityType Ability { get; set; }
        public UseUnitTargetSpecialAbilityCommand(Unit unit, Unit target, SpecialAbilityType ability) : base(unit, target)
        {
            this.Ability = ability;
        }
    }
}
