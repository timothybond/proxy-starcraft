using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class UnspecifiedUnit : Unit2
    {
        public UnspecifiedUnit(Unit unit, Translator translator) : base(unit, translator)
        {
        }

        public override BuildingOrUnitType Type => null;
    }
}
