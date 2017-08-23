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
                    // TODO: Add, with appropriate size, to structuresAndDeposits
                }
            }
        }
    }
}
