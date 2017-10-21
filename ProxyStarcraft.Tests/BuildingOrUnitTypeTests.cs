using NUnit.Framework;

namespace ProxyStarcraft.Tests.BuildingOrUnitTypeTests
{
    public class EqualsOperator
    {
        #region BuildingOrUnitType comparisons

        [Test]
        [TestCase(TerranBuildingType.Barracks)]
        [TestCase(TerranBuildingType.CommandCenter)]
        public void HandlesBuildingOrUnitTypeWithTerranBuilding(TerranBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var otherBuildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        [Test]
        [TestCase(ProtossBuildingType.Pylon)]
        [TestCase(ProtossBuildingType.WarpGate)]
        public void HandlesBuildingOrUnitTypeWithProtossBuilding(ProtossBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var otherBuildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        [Test]
        [TestCase(ZergBuildingType.CreepTumor)]
        [TestCase(ZergBuildingType.InfestationPit)]
        public void HandlesBuildingOrUnitTypeWithZergBuilding(ZergBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var otherBuildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        [Test]
        [TestCase(TerranUnitType.Marine)]
        [TestCase(TerranUnitType.SiegeTank)]
        public void HandlesBuildingOrUnitTypeWithTerranUnit(TerranUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var otherBuildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        [Test]
        [TestCase(ProtossUnitType.MothershipCore)]
        [TestCase(ProtossUnitType.HighTemplar)]
        public void HandlesBuildingOrUnitTypeWithProtossUnit(ProtossUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var otherBuildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        [Test]
        [TestCase(ZergUnitType.Roach)]
        [TestCase(ZergUnitType.Cocoon)]
        public void HandlesBuildingOrUnitTypeWithZergUnit(ZergUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var otherBuildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == otherBuildingOrUnitType);
        }

        #endregion

        #region BuildingType comparisons

        [Test]
        [TestCase(TerranBuildingType.Barracks)]
        [TestCase(TerranBuildingType.CommandCenter)]
        public void HandlesBuildingTypeWithTerranBuilding(TerranBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var buildingType = new BuildingType(building);

            Assert.IsTrue(buildingOrUnitType == buildingType);
        }

        [Test]
        [TestCase(ProtossBuildingType.Pylon)]
        [TestCase(ProtossBuildingType.WarpGate)]
        public void HandlesBuildingTypeWithProtossBuilding(ProtossBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var buildingType = new BuildingType(building);

            Assert.IsTrue(buildingOrUnitType == buildingType);
        }

        [Test]
        [TestCase(ZergBuildingType.CreepTumor)]
        [TestCase(ZergBuildingType.InfestationPit)]
        public void HandlesBuildingTypeWithZergBuilding(ZergBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);
            var buildingType = new BuildingType(building);

            Assert.IsTrue(buildingOrUnitType == buildingType);
        }

        #endregion

        #region UnitType comparisons

        [Test]
        [TestCase(TerranUnitType.Marine)]
        [TestCase(TerranUnitType.SiegeTank)]
        public void HandlesUnitTypeWithTerranUnit(TerranUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var unitType = new UnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unitType);
        }

        [Test]
        [TestCase(ProtossUnitType.MothershipCore)]
        [TestCase(ProtossUnitType.HighTemplar)]
        public void HandlesUnitTypeWithProtossUnit(ProtossUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var unitType = new UnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unitType);
        }

        [Test]
        [TestCase(ZergUnitType.Roach)]
        [TestCase(ZergUnitType.Cocoon)]
        public void HandlesUnitTypeWithZergUnit(ZergUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);
            var unitType = new UnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unitType);
        }

        #endregion

        #region Enum Comparisons

        [Test]
        [TestCase(TerranBuildingType.Barracks)]
        [TestCase(TerranBuildingType.CommandCenter)]
        public void HandlesTerranBuildingType(TerranBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == building);
        }

        [Test]
        [TestCase(ProtossBuildingType.Pylon)]
        [TestCase(ProtossBuildingType.WarpGate)]
        public void HandlesProtossBuildingType(ProtossBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == building);
        }

        [Test]
        [TestCase(ZergBuildingType.CreepTumor)]
        [TestCase(ZergBuildingType.InfestationPit)]
        public void HandlesZergBuildingType(ZergBuildingType building)
        {
            var buildingOrUnitType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingOrUnitType == building);
        }
        
        [Test]
        [TestCase(TerranUnitType.Marine)]
        [TestCase(TerranUnitType.SiegeTank)]
        public void HandlesTerranUnitType(TerranUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unit);
        }

        [Test]
        [TestCase(ProtossUnitType.MothershipCore)]
        [TestCase(ProtossUnitType.HighTemplar)]
        public void HandlesProtossUnitType(ProtossUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unit);
        }

        [Test]
        [TestCase(ZergUnitType.Roach)]
        [TestCase(ZergUnitType.Cocoon)]
        public void HandlesZergUnitType(ZergUnitType unit)
        {
            var buildingOrUnitType = new BuildingOrUnitType(unit);

            Assert.IsTrue(buildingOrUnitType == unit);
        }

        #endregion
    }
}
