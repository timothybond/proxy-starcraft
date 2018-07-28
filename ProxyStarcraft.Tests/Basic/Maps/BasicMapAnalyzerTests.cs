using Google.Protobuf;
using NUnit.Framework;
using ProxyStarcraft.Basic;
using ProxyStarcraft.Proto;
using System.IO;

namespace ProxyStarcraft.Tests.Basic.Maps.BasicMapAnalyzerTests
{
    // Note: these are brittle tests because they require the same exact numbers for each area.
    public class GetInitial
    {
        private static readonly Size2DI MapSize = new Size2DI { X = 200, Y = 176 };

        private string dataFolder;

        [SetUp]
        public void Setup()
        {
            this.dataFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "Basic", "Maps");
        }

        [Test]
        public void GetsExpectedAreas()
        {
            var startRaw = new StartRaw();

            startRaw.MapSize = MapSize;
            startRaw.PlacementGrid = LoadImageData("placement.dat");
            startRaw.PathingGrid = LoadImageData("pathing.dat");
            startRaw.TerrainHeight = LoadImageData("height.dat");

            // Needs to be a valid passable space
            startRaw.StartLocations.Add(new Point2D { X = 40, Y = 120 });

            var map = new Map(startRaw);

            var mapAnalyzer = new BasicMapAnalyzer();
            var mapData = mapAnalyzer.GetInitial(map);

            var areas = LoadMapArray("areas-input.dat");

            Assert.AreEqual(areas.Data, mapData.AreaGrid.Data);
        }

        [Test]
        public void GetsExpectedAreasWithSizeLimit()
        {
            var startRaw = new StartRaw();

            startRaw.MapSize = MapSize;
            startRaw.PlacementGrid = LoadImageData("placement.dat");
            startRaw.PathingGrid = LoadImageData("pathing.dat");
            startRaw.TerrainHeight = LoadImageData("height.dat");

            // Needs to be a valid passable space
            startRaw.StartLocations.Add(new Point2D { X = 40, Y = 120 });

            var map = new Map(startRaw);

            var mapAnalyzer = new BasicMapAnalyzer(1000);
            var mapData = mapAnalyzer.GetInitial(map);

            var areas = LoadMapArray("areas-output.dat");

            Assert.AreEqual(areas.Data, mapData.AreaGrid.Data);
        }

        private ImageData LoadImageData(string filename)
        {
            var dataFileBytes = File.ReadAllBytes(Path.Combine(dataFolder, filename));
            return new ImageData() { Data = ByteString.CopyFrom(dataFileBytes), Size = MapSize };
        }

        private MapArray<byte> LoadMapArray(string filename)
        {
            var dataFileBytes = File.ReadAllBytes(Path.Combine(dataFolder, filename));
            return new MapArray<byte>(dataFileBytes, MapSize);
        }
    }
}
