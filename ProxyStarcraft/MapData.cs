using Google.Protobuf.Collections;
using ProxyStarcraft.Proto;
using System;
using System.Collections.Generic;

namespace ProxyStarcraft
{
    public class MapData
    {
        private Size2DI mapSize;

        private byte[,] placementGrid;

        private byte[,] pathingGrid;

        private Unit[,] structuresAndDeposits;

        // One space of padding around each non-buildable space,
        // usable as a primitive strategy to avoid blocking things like ramps
        private bool[,] padding;

        private bool[,] structurePadding;

        public MapData(StartRaw startingData)
        {
            this.mapSize = startingData.MapSize;

            placementGrid = new byte[this.mapSize.X, this.mapSize.Y];
            pathingGrid = new byte[this.mapSize.X, this.mapSize.Y];

            Buffer.BlockCopy(startingData.PathingGrid.Data.ToByteArray(), 0, pathingGrid, 0, pathingGrid.Length);
            Buffer.BlockCopy(startingData.PlacementGrid.Data.ToByteArray(), 0, placementGrid, 0, placementGrid.Length);

            structuresAndDeposits = new Unit[this.mapSize.X, this.mapSize.Y];
        }

        public MapData(MapData prior, RepeatedField<Unit> units, Translator translator, Dictionary<uint, UnitTypeData> unitTypes)
        {
            this.mapSize = prior.mapSize;
            this.placementGrid = prior.placementGrid;
            this.pathingGrid = prior.pathingGrid;

            structuresAndDeposits = new Unit[this.mapSize.X, this.mapSize.Y];

            foreach (var unit in units)
            {
                var unitType = unitTypes[unit.UnitType];
                if (unitType.Attributes.Contains(Proto.Attribute.Structure))
                {
                    var structureSize = translator.GetStructureSize(unit);
                    var originX = (int)Math.Round(unit.Pos.X - structureSize.X * 0.5f);
                    var originY = (int)Math.Round(unit.Pos.Y - structureSize.Y * 0.5f);

                    for (var x = originX; x < originX + structureSize.X; x++)
                    {
                        for (var y = originY; y < originY + structureSize.Y; y++)
                        {
                            structuresAndDeposits[x, y] = unit;
                            SetAdjacentSpaces(structurePadding, x, y);
                        }
                    }
                }
            }
        }

        public bool CanBuild(Size2DI size, int originX, int originY)
        {
            return CanBuild(size, originX, originY, true);
        }

        public bool CanBuild(Size2DI size, int originX, int originY, bool includePadding)
        {
            for (var x = originX; x < originX + size.X; x++)
            {
                for (var y = originY; y < originY + size.Y; y++)
                {
                    if (placementGrid[x, y] == 0 ||
                        structuresAndDeposits[x, y] != null)
                    {
                        return false;
                    }

                    if (includePadding && (padding[x, y] || structurePadding[x, y]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void GeneratePadding(StartRaw startingData)
        {
            this.padding = new bool[mapSize.X, mapSize.Y];

            for (var x = 0; x < mapSize.X; x++)
            {
                for (var y = 0; y < mapSize.Y; y++)
                {
                    if (placementGrid[x, y] == 0) // TODO: determine what means 'not placeable', which I think is 0
                    {
                        SetAdjacentSpaces(this.padding, x, y);
                    }
                }
            }
        }

        private void SetAdjacentSpaces(bool[,] targetArray, int x, int y)
        {
            var xVals = new List<int> { x - 1, x, x + 1 };
            xVals.Remove(-1);
            xVals.Remove(mapSize.X);

            var yVals = new List<int> { y - 1, y, y + 1 };
            yVals.Remove(-1);
            yVals.Remove(mapSize.Y);
            
            foreach (var xVal in xVals)
            {
                foreach (var yVal in yVals)
                {
                    targetArray[xVal, yVal] = true;
                }
            }
        }
    }
}
