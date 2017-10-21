using NUnit.Framework;

namespace ProxyStarcraft.Tests.BuildingTypeTests
{
    public class EqualsOperator
    {
        #region BuildingType comparisons

        [Test]
        [TestCase(TerranBuildingType.Barracks)]
        [TestCase(TerranBuildingType.CommandCenter)]
        public void HandlesBuildingTypeWithTerranBuilding(TerranBuildingType building)
        {
            var buildingType = new BuildingType(building);
            var otherBuildingType = new BuildingType(building);

            Assert.IsTrue(buildingType == otherBuildingType);
        }

        [Test]
        [TestCase(ProtossBuildingType.Pylon)]
        [TestCase(ProtossBuildingType.WarpGate)]
        public void HandlesBuildingTypeWithProtossBuilding(ProtossBuildingType building)
        {
            var buildingType = new BuildingType(building);
            var otherBuildingType = new BuildingType(building);

            Assert.IsTrue(buildingType == otherBuildingType);
        }

        [Test]
        [TestCase(ZergBuildingType.CreepTumor)]
        [TestCase(ZergBuildingType.InfestationPit)]
        public void HandlesBuildingTypeWithZergBuilding(ZergBuildingType building)
        {
            var buildingType = new BuildingType(building);
            var otherBuildingType = new BuildingType(building);

            Assert.IsTrue(buildingType == otherBuildingType);
        }

        #endregion

        #region Enum comparisons

        [Test]
        [TestCase(TerranBuildingType.Barracks)]
        [TestCase(TerranBuildingType.CommandCenter)]
        public void HandlesTerranBuildingType(TerranBuildingType building)
        {
            var buildingType = new BuildingType(building);

            Assert.IsTrue(buildingType == building);
        }

        [Test]
        [TestCase(ProtossBuildingType.Pylon)]
        [TestCase(ProtossBuildingType.WarpGate)]
        public void HandlesProtossBuildingType(ProtossBuildingType building)
        {
            var buildingType = new BuildingType(building);

            Assert.IsTrue(buildingType == building);
        }

        [Test]
        [TestCase(ZergBuildingType.CreepTumor)]
        [TestCase(ZergBuildingType.InfestationPit)]
        public void HandlesZergBuildingType(ZergBuildingType building)
        {
            var buildingType = new BuildingOrUnitType(building);

            Assert.IsTrue(buildingType == building);
        }

        #endregion
    }
}
