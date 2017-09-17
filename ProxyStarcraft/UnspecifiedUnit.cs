namespace ProxyStarcraft
{
    public class UnspecifiedUnit : Unit
    {
        public UnspecifiedUnit(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
        }

        public override BuildingOrUnitType Type => null;
    }
}
