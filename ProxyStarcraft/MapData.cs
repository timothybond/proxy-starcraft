using Google.Protobuf.Collections;
using ProxyStarcraft.Proto;
using System;
using System.Collections.Generic;

namespace ProxyStarcraft
{
    public class MapData
    {
        // I believe this uses 0 for 'not buildable' and 255 for 'buildable'.
        private byte[,] placementGrid;

        // Not totally sure what the expected values are here
        private byte[,] pathingGrid;

        private Unit[,] structuresAndDeposits;

        // One space of padding around each non-buildable space,
        // usable as a primitive strategy to avoid blocking things like ramps
        private bool[,] padding;

        // Same as above but for known buildings
        private bool[,] structurePadding;

        public MapData(StartRaw startingData)
        {
            this.Raw = startingData;
            this.Size = startingData.MapSize;

            pathingGrid = new byte[this.Size.X, this.Size.Y];
            placementGrid = new byte[this.Size.X, this.Size.Y];
            
            var pathingBytes = startingData.PathingGrid.Data.ToByteArray();
            var placementBytes = startingData.PlacementGrid.Data.ToByteArray();

            // The bytes for this image aren't in the format that we would want -
            // for convience and clarity we would like a 2d array where the x and y coordinates
            // match the locations as used elsewhere in the engine, i.e., bytes ordered from
            // bottom to top and then from left to right.
            //
            // The actual bytes are given from left to right and then from top to bottom.
            // We can fix this by transposing them, and then inverting the y-values.
            for (var i = 0; i < pathingBytes.Length; i++)
            {
                pathingGrid[i % this.Size.X, this.Size.Y - 1 - i / this.Size.X] = pathingBytes[i];
                placementGrid[i % this.Size.X, this.Size.Y - 1 - i / this.Size.X] = placementBytes[i];
            }

            structuresAndDeposits = new Unit[this.Size.X, this.Size.Y];

            GeneratePadding(startingData);

            this.structurePadding = new bool[this.Size.X, this.Size.Y];
        }

        public MapData(MapData prior, RepeatedField<Unit> units, Translator translator, Dictionary<uint, UnitTypeData> unitTypes)
        {
            this.Raw = prior.Raw;
            this.Size = prior.Size;
            this.placementGrid = prior.placementGrid;
            this.pathingGrid = prior.pathingGrid;
            this.padding = prior.padding;

            this.structuresAndDeposits = new Unit[this.Size.X, this.Size.Y];
            this.structurePadding = new bool[this.Size.X, this.Size.Y];

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

        public StartRaw Raw { get; private set; }

        public Size2DI Size { get; private set; }

        // TODO: Check that the pathing grid is what I think it is, because I have become suspicious
        public bool CanTraverse(Location location)
        {
            return pathingGrid[location.X, location.Y] == 0;
        }

        public bool CanBuild(Location location)
        {
            return placementGrid[location.X, location.Y] != 0;
        }

        public bool CanBuild(Size2DI size, Location location)
        {
            return CanBuild(size, location.X, location.Y, true);
        }

        public bool CanBuild(Size2DI size, int originX, int originY)
        {
            return CanBuild(size, originX, originY, true);
        }

        public bool CanBuild(Size2DI size, Location location, bool includePadding)
        {
            return CanBuild(size, location.X, location.Y, includePadding);
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
            this.padding = new bool[Size.X, Size.Y];

            for (var x = 0; x < Size.X; x++)
            {
                for (var y = 0; y < Size.Y; y++)
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
            xVals.Remove(Size.X);

            var yVals = new List<int> { y - 1, y, y + 1 };
            yVals.Remove(-1);
            yVals.Remove(Size.Y);
            
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
