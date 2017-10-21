using System;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents one of <see cref="TerranUnitType"/>, <see cref="ProtossUnitType"/>, or <see cref="ZergUnitType"/>.
    /// 
    /// Replace if a better way to represent a Union type is determined.
    /// </summary>
    public class UnitType
    {
        public static implicit operator UnitType(TerranUnitType unit) => new UnitType(unit);

        public static implicit operator UnitType(ProtossUnitType unit) => new UnitType(unit);

        public static implicit operator UnitType(ZergUnitType unit) => new UnitType(unit);

        public UnitType(TerranUnitType unit)
        {
            this.Value = unit;
        }

        public UnitType(ProtossUnitType unit)
        {
            this.Value = unit;
        }

        public UnitType(ZergUnitType unit)
        {
            this.Value = unit;
        }

        public object Value { get; private set; }

        public static bool operator ==(UnitType first, UnitType second)
        {
            if (first.Value is TerranUnitType firstTerranUnit && second.Value is TerranUnitType secondTerranUnit)
            {
                return firstTerranUnit == secondTerranUnit;
            }
            else if (first.Value is ProtossUnitType firstProtossUnit && second.Value is ProtossUnitType secondProtossUnit)
            {
                return firstProtossUnit == secondProtossUnit;
            }
            else if (first.Value is ZergUnitType firstZergUnit && second.Value is ZergUnitType secondZergUnit)
            {
                return firstZergUnit == secondZergUnit;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(UnitType first, UnitType second)
        {
            return !(first == second);
        }
    }
}
