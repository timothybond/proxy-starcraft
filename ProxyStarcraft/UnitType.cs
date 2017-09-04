using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (unit == TerranUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid TerranUnitType - 'Unspecified'.", "unit");
            }

            this.TerranUnit = unit;
        }

        public UnitType(ProtossUnitType unit)
        {
            if (unit == ProtossUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid ProtossUnitType - 'Unspecified'.", "unit");
            }

            this.ProtossUnit = unit;
        }

        public UnitType(ZergUnitType unit)
        {
            if (unit == ZergUnitType.Unspecified)
            {
                throw new ArgumentException("Invalid ZergUnitType - 'Unspecified'.", "unit");
            }

            this.ZergUnit = unit;
        }

        public TerranUnitType TerranUnit { get; private set; }

        public ProtossUnitType ProtossUnit { get; private set; }

        public ZergUnitType ZergUnit { get; private set; }
    }
}
