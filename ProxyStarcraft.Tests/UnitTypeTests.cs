using NUnit.Framework;

namespace ProxyStarcraft.Tests.UnitTypeTests
{
    public class EqualsOperator
    {
        #region UnitType comparisons

        [Test]
        [TestCase(TerranUnitType.Marine)]
        [TestCase(TerranUnitType.SiegeTank)]
        public void HandlesUnitTypeWithTerranUnit(TerranUnitType unit)
        {
            var unitType = new UnitType(unit);
            var otherUnitType = new UnitType(unit);

            Assert.IsTrue(unitType == otherUnitType);
        }

        [Test]
        [TestCase(ProtossUnitType.MothershipCore)]
        [TestCase(ProtossUnitType.HighTemplar)]
        public void HandlesUnitTypeWithProtossUnit(ProtossUnitType unit)
        {
            var unitType = new UnitType(unit);
            var otherUnitType = new UnitType(unit);

            Assert.IsTrue(unitType == otherUnitType);
        }

        [Test]
        [TestCase(ZergUnitType.Roach)]
        [TestCase(ZergUnitType.Cocoon)]
        public void HandlesUnitTypeWithZergUnit(ZergUnitType unit)
        {
            var unitType = new UnitType(unit);
            var otherUnitType = new UnitType(unit);

            Assert.IsTrue(unitType == otherUnitType);
        }

        #endregion

        #region Enum Comparisons
        
        [Test]
        [TestCase(TerranUnitType.Marine)]
        [TestCase(TerranUnitType.SiegeTank)]
        public void HandlesTerranUnitType(TerranUnitType unit)
        {
            var unitType = new UnitType(unit);

            Assert.IsTrue(unitType == unit);
        }

        [Test]
        [TestCase(ProtossUnitType.MothershipCore)]
        [TestCase(ProtossUnitType.HighTemplar)]
        public void HandlesProtossUnitType(ProtossUnitType unit)
        {
            var unitType = new UnitType(unit);

            Assert.IsTrue(unitType == unit);
        }

        [Test]
        [TestCase(ZergUnitType.Roach)]
        [TestCase(ZergUnitType.Cocoon)]
        public void HandlesZergUnitType(ZergUnitType unit)
        {
            var unitType = new UnitType(unit);

            Assert.IsTrue(unitType == unit);
        }

        #endregion
    }
}
