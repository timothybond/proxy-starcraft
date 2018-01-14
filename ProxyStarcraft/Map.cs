using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class Map
    {
        // I believe this uses 0 for 'not buildable' and 255 for 'buildable'.
        private MapArray<byte> placementGrid;

        // Keep a copy of the original so we don't have to worry about cleaning off buildings, etc.
        private MapArray<byte> placementGridOriginal;

        // Strangely, this appears to be 0 for 'can move to/through' and 255 for 'can't move to/through'
        private MapArray<byte> pathingGrid;

        private MapArray<byte> heightGrid;

        private MapArray<Unit> structuresAndDeposits;

        private List<Unit> structuresAndDepositsList;
        
        private MapArray<byte> creepGrid;

        // One space of padding around each non-buildable space,
        // usable as a primitive strategy to avoid blocking things like ramps
        private MapArray<bool> padding;

        /// <summary>
        /// Three-by-three box of padding around each (initial) resource location, which blocks main base placement.
        /// </summary>
        private MapArray<bool> resourcePadding;

        // Same as above but for known buildings
        private MapArray<bool> structurePadding;
        
        // TODO: Avoid using nullable type for this
        private Location? playerStartLocation;

        // TODO: Avoid using nullable type for this
        private Location? enemyStartLocation;

        public Map(StartRaw startingData)
        {
            this.Raw = startingData;
            this.Size = startingData.MapSize;

            placementGridOriginal = new MapArray<byte>(startingData.PlacementGrid.Data.ToByteArray(), this.Size);
            placementGrid = new MapArray<byte>(placementGridOriginal);
            pathingGrid = new MapArray<byte>(startingData.PathingGrid.Data.ToByteArray(), this.Size);
            heightGrid = new MapArray<byte>(startingData.TerrainHeight.Data.ToByteArray(), this.Size);
            creepGrid = new MapArray<byte>(startingData.TerrainHeight.Data.ToByteArray(), this.Size);

            this.structuresAndDeposits = new MapArray<Unit>(this.Size);
            this.structuresAndDepositsList = new List<Unit>();

            GeneratePadding(startingData);
            
            this.structurePadding = new MapArray<bool>(this.Size);
            this.resourcePadding = new MapArray<bool>(this.Size);

            // Not gonna work for anything but 1-on-1 games, but it seems like
            // it doesn't give this player's location, just the enemy's.
            var enemyStart = startingData.StartLocations.Single();
            this.enemyStartLocation = new Location { X = (int)enemyStart.X, Y = (int)enemyStart.Y };
        }

        public Map(Map prior, List<Unit> units, Translator translator, Dictionary<uint, UnitTypeData> unitTypes, ImageData creep)
        {
            this.playerStartLocation = prior.playerStartLocation;
            this.enemyStartLocation = prior.enemyStartLocation;
            this.Raw = prior.Raw;
            this.Size = prior.Size;
            this.placementGridOriginal = prior.placementGridOriginal;
            this.placementGrid = new MapArray<byte>(this.placementGridOriginal);
            this.pathingGrid = prior.pathingGrid;
            this.heightGrid = prior.heightGrid;
            this.padding = prior.padding;
            this.creepGrid = new MapArray<byte>(creep.Data.ToByteArray(), this.Size);
            
            this.structuresAndDeposits = new MapArray<Unit>(this.Size);
            this.structuresAndDepositsList = new List<Unit>();
            this.structurePadding = new MapArray<bool>(this.Size);
            this.resourcePadding = new MapArray<bool>(this.Size);

            foreach (var unit in units)
            {
                var unitType = unitTypes[unit.Raw.UnitType];
                if (unitType.Attributes.Contains(Proto.Attribute.Structure))
                {
                    var structureSize = translator.GetStructureSize(unit);
                    var originX = (int)Math.Round(unit.X - structureSize.X * 0.5f);
                    var originY = (int)Math.Round(unit.Y - structureSize.Y * 0.5f);
                    var origin = new Location { X = originX, Y = originY };

                    BuildingType buildingType = (unit.Type?.IsBuildingType ?? false) ? (BuildingType)unit.Type : null;

                    this.structuresAndDepositsList.Add(unit);
                    StoreBuildingLocation(origin, structureSize, unit, buildingType);
                }

                // Check unit orders to find planned buildings
                if (unit.IsWorker)
                {
                    var currentlyBuilding = translator.CurrentlyBuilding(unit);

                    if (currentlyBuilding != null &&
                        currentlyBuilding.IsBuildingType &&
                        currentlyBuilding != TerranBuildingType.Refinery &&
                        currentlyBuilding != ProtossBuildingType.Assimilator &&
                        currentlyBuilding != ZergBuildingType.Extractor)
                    {
                        var building = (BuildingType)currentlyBuilding;
                        var buildingSize = translator.GetBuildingSize(building);
                        var targetX = (int)Math.Round(unit.Raw.Orders[0].TargetWorldSpacePos.X - (buildingSize.X * 0.5f));
                        var targetY = (int)Math.Round(unit.Raw.Orders[0].TargetWorldSpacePos.Y - (buildingSize.Y * 0.5f));

                        var targetLocation = new Location { X = targetX, Y = targetY };

                        StoreBuildingLocation(targetLocation, buildingSize, null, building);
                    }
                }
            }

            if (!this.playerStartLocation.HasValue)
            {
                var mainBase = units.Single(u => u.Raw.Alliance == Alliance.Self && u.IsMainBase);
                this.playerStartLocation = new Location { X = (int)mainBase.X, Y = (int)mainBase.Y };
            }
        }

        // Note: includes natural structures, which is why the argument type is 'Unit'.
        // Also the 'structure' arg can be optional for unbuilt structures, so a separate type is required.
        private void StoreBuildingLocation(Location origin, Size2DI size, Unit structure, BuildingType type)
        {
            for (var x = origin.X; x < origin.X + size.X; x++)
            {
                for (var y = origin.Y; y < origin.Y + size.Y; y++)
                {
                    structuresAndDeposits[x, y] = structure;
                    SetAdjacentSpaces(structurePadding, x, y);

                    if (structure != null && 
                        (structure.IsMineralDeposit || structure.IsVespeneGeyser || structure.IsVespeneBuilding))
                    {
                        SetAdjacentSpaces(resourcePadding, x, y, 3);
                    }

                    // Reserve space for Tech Labs
                    if (type == TerranBuildingType.Barracks ||
                        type == TerranBuildingType.Factory ||
                        type == TerranBuildingType.Starport)
                    {
                        // Note: I'm not sure if this will come up, but this could theoretically go off the edge of the map
                        StoreBuildingLocation(new Location { X = origin.X + 3, Y = origin.Y }, new Size2DI { X = 2, Y = 2 }, null, TerranBuildingType.TechLab);
                    }
                }
            }
        }

        public StartRaw Raw { get; private set; }

        public Size2DI Size { get; private set; }

        public Location PlayerStartLocation => this.playerStartLocation.Value;

        public Location EnemyStartLocation => this.enemyStartLocation.Value;

        public MapArray<byte> PlacementGrid => new MapArray<byte>(this.placementGrid);

        public MapArray<byte> PathingGrid => new MapArray<byte>(this.pathingGrid);

        public MapArray<byte> HeightGrid => new MapArray<byte>(this.heightGrid);
        
        public MapArray<byte> CreepGrid => this.creepGrid;
        
        public IReadOnlyList<Unit> StructuresAndDeposits => this.structuresAndDepositsList;
        
        public bool CanTraverse(Location location)
        {
            return pathingGrid[location.X, location.Y] == 0;
        }

        public bool CanBuild(Location location)
        {
            return placementGrid[location.X, location.Y] != 0;
        }
        
        public bool CanBuild(Size2DI size, Location location, bool requireCreep = false, bool hasAddOn = false, bool includeResourcePadding = false, bool includePadding = true)
        {
            var offsets = new List<LocationOffset>();

            for (var x = 0; x < size.X; x++)
            {
                for (var y = 0; y < size.Y; y++)
                {
                    offsets.Add(new LocationOffset { X = x, Y = y });
                }
            }

            // This assumes (as is currently true) that all add-ons are 2x2 buildings
            // directly to the right of this one and with the same bottom Y-coordinate.
            if (hasAddOn)
            {
                offsets.Add(new LocationOffset { X = size.X, Y = 0 });
                offsets.Add(new LocationOffset { X = size.X, Y = 1 });
                offsets.Add(new LocationOffset { X = size.X + 1, Y = 0 });
                offsets.Add(new LocationOffset { X = size.X + 1, Y = 1 });
            }

            return CanBuild(location, offsets, requireCreep, includeResourcePadding, includePadding);
        }
        
        public bool CanBuild(Location origin, IReadOnlyList<LocationOffset> offsets, bool requireCreep = false, bool includeResourcePadding = false, bool includePadding = true)
        {
            foreach (var offset in offsets)
            {
                var location = origin + offset;

                if (placementGrid[location] == 0 ||
                        structuresAndDeposits[location] != null)
                {
                    return false;
                }

                if (includePadding && (padding[location] || structurePadding[location]))
                {
                    return false;
                }

                if (includeResourcePadding && resourcePadding[location])
                {
                    return false;
                }

                if (requireCreep && creepGrid[location] == 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void GeneratePadding(StartRaw startingData)
        {
            this.padding = new MapArray<bool>(this.Size);

            for (var x = 0; x < Size.X; x++)
            {
                for (var y = 0; y < Size.Y; y++)
                {
                    if (placementGrid[x, y] == 0)
                    {
                        SetAdjacentSpaces(this.padding, x, y);
                    }
                }
            }
        }

        private void SetAdjacentSpaces(MapArray<bool> targetArray, int x, int y, int size = 1)
        {
            var xVals = new List<int> { x - size, x, x + size };
            xVals.RemoveAll(n => n < 0 || n >= Size.X);

            var yVals = new List<int> { y - size, y, y + size };
            yVals.RemoveAll(n => n < 0 || n >= Size.Y);
            
            foreach (var xVal in xVals)
            {
                foreach (var yVal in yVals)
                {
                    targetArray[xVal, yVal] = true;
                }
            }
        }

        #region Map Analyzer Code

        /// <summary>
        /// Performs a breadth-first search from the set of starting locations, for a location meeting a specified condition.
        /// </summary>
        /// <param name="locationCondition">The condition that we're looking for in a location.</param>
        /// <param name="startingLocations"></param>
        /// <param name="adjacentLocationFilter">Any filters for which adjacent locations are worth considering (e.g., if you only want to check ground-unit-traversable locations). If null, all adjacent locations are checked.</param>
        /// <returns>The location closest to any of the starting locations that meets the condition.</returns>
        public Location? BreadthFirstSearch(Func<Map, Location, bool> locationCondition, IEnumerable<Location> startingLocations, Func<Map, Location, bool> adjacentLocationFilter = null)
        {
            if (adjacentLocationFilter == null)
            {
                adjacentLocationFilter = (m, l) => true;
            }

            var locations = new HashSet<Location>(startingLocations);

            var pastLocations = new HashSet<Location>();
            var nextLocations = new HashSet<Location>();

            // This is essentially a breadth-first search of map locations
            while (locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    if (locationCondition(this, location))
                    {
                        return location;
                    }
                    
                    var adjacentLocations = location.AdjacentLocations(this.Size);

                    foreach (var adjacentLocation in adjacentLocations)
                    {
                        if (!pastLocations.Contains(adjacentLocation) && !locations.Contains(adjacentLocation) && adjacentLocationFilter(this, adjacentLocation))
                        {
                            nextLocations.Add(adjacentLocation);
                        }
                    }

                    pastLocations.Add(location);
                }

                locations = nextLocations;
                nextLocations = new HashSet<Location>();
            }

            return null;
        }
        
        private void AddAdjacentLocations(Location location, HashSet<Location> locations, MapArray<byte> areas)
        {
            foreach (var adjacentLocation in AdjacentLocations(location))
            {
                if (areas[adjacentLocation] == 0 &&
                    (this.CanBuild(adjacentLocation) || this.CanTraverse(adjacentLocation)))
                {
                    locations.Add(adjacentLocation);
                }
            }
        }

        private IEnumerable<Location> AdjacentLocations(Location location)
        {
            var results = new List<Location>();

            var xVals = new List<int> { location.X - 1, location.X, location.X + 1 };
            xVals.Remove(-1);
            xVals.Remove(this.Size.X);

            var yVals = new List<int> { location.Y - 1, location.Y, location.Y + 1 };
            yVals.Remove(-1);
            yVals.Remove(this.Size.Y);

            foreach (var x in xVals)
            {
                foreach (var y in yVals)
                {
                    if (x != location.X || y != location.Y)
                    {
                        yield return new Location { X = x, Y = y };
                    }
                }
            }
        }

        #endregion
    }
}
