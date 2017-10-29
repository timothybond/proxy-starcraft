namespace ProxyStarcraft.Commands
{
    public class UseLocationTargetSpecialAbilityCommand : LocationTargetCommand
    {
        public SpecialAbilityType Ability { get; set; }
        public UseLocationTargetSpecialAbilityCommand(Unit unit, float x, float y, SpecialAbilityType ability) : base(unit, x, y)
        {
            this.Ability = ability;
        }
    }
}
