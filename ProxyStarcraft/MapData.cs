using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Map;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class MapData
    {
        // I believe this uses 0 for 'not buildable' and 255 for 'buildable'.
        private MapArray<byte> placementGrid;

        // Strangely, this appears to be 0 for 'can move to/through' and 255 for 'can't move to/through'
        private MapArray<byte> pathingGrid;

        private MapArray<byte> heightGrid;

        private MapArray<Unit> structuresAndDeposits;

        private MapArray<byte> areaGrid;

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

        private List<Area> areas;

        private List<Deposit> deposits;

        public MapData(StartRaw startingData)
        {
            this.Raw = startingData;
            this.Size = startingData.MapSize;

            pathingGrid = new MapArray<byte>(startingData.PathingGrid.Data.ToByteArray(), this.Size);
            placementGrid = new MapArray<byte>(startingData.PlacementGrid.Data.ToByteArray(), this.Size);
            heightGrid = new MapArray<byte>(startingData.TerrainHeight.Data.ToByteArray(), this.Size);
            creepGrid = new MapArray<byte>(startingData.TerrainHeight.Data.ToByteArray(), this.Size);

            this.structuresAndDeposits = new MapArray<Unit>(this.Size);

            GeneratePadding(startingData);

            this.areas = GetAreas();
            this.deposits = new List<Deposit>();

            this.structurePadding = new MapArray<bool>(this.Size);
            this.resourcePadding = new MapArray<bool>(this.Size);
        }

        public MapData(MapData prior, List<Unit> units, Translator translator, Dictionary<uint, UnitTypeData> unitTypes, ImageData creep)
        {
            this.Raw = prior.Raw;
            this.Size = prior.Size;
            this.placementGrid = prior.placementGrid;
            this.pathingGrid = prior.pathingGrid;
            this.heightGrid = prior.heightGrid;
            this.padding = prior.padding;
            this.areaGrid = prior.areaGrid;
            this.areas = prior.areas;
            this.creepGrid = new MapArray<byte>(creep.Data.ToByteArray(), this.Size);

            this.deposits = GetDeposits(units);

            this.structuresAndDeposits = new MapArray<Unit>(this.Size);
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

                    for (var x = originX; x < originX + structureSize.X; x++)
                    {
                        for (var y = originY; y < originY + structureSize.Y; y++)
                        {
                            structuresAndDeposits[x, y] = unit;
                            SetAdjacentSpaces(structurePadding, x, y);

                            if (unit.IsMineralDeposit || unit.IsVespeneGeyser || unit.IsVespeneBuilding)
                            {
                                SetAdjacentSpaces(resourcePadding, x, y, 3);
                            }
                        }
                    }
                }
            }
        }

        public StartRaw Raw { get; private set; }

        public Size2DI Size { get; private set; }

        public MapArray<byte> PlacementGrid => new MapArray<byte>(this.placementGrid);

        public MapArray<byte> PathingGrid => new MapArray<byte>(this.pathingGrid);

        public MapArray<byte> HeightGrid => new MapArray<byte>(this.heightGrid);

        public MapArray<byte> AreaGrid => this.areaGrid;

        public MapArray<byte> CreepGrid => this.creepGrid;

        public IReadOnlyList<Area> Areas => this.areas;

        public IReadOnlyList<Deposit> Deposits => this.deposits;
        
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
        public Location? BreadthFirstSearch(Func<MapData, Location, bool> locationCondition, IEnumerable<Location> startingLocations, Func<MapData, Location, bool> adjacentLocationFilter = null)
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

        /// <summary>
        /// Builds a list of <see cref="Area"/>s that have references to their neighbors.
        /// </summary>
        public List<Area> GetAreas() // TODO: Break up this function
        {
            this.areaGrid = GetAreaGrid();
            var neighbors = GetNeighbors(areaGrid);

            var mesas = new Dictionary<byte, Mesa>();
            var ramps = new Dictionary<byte, Ramp>();
            var edges = new Dictionary<byte, Edge>();

            var maxAreaId = areaGrid.Data.Max();

            // Need to get the center of each area. To avoid translation confusion,
            // I'm just going to create arrays with an extra space even though there's
            // no relevant area 0 (which instead represents impassible terrain).
            var centers = new Location[maxAreaId + 1];
            var locationLists = new List<Location>[maxAreaId + 1];
            var xTotals = new int[maxAreaId + 1];
            var yTotals = new int[maxAreaId + 1];
            var counts = new int[maxAreaId + 1];

            for (var x = 0; x < this.Size.X; x++)
            {
                for (var y = 0; y < this.Size.Y; y++)
                {
                    var areaId = areaGrid[x, y];
                    if (areaId != 0)
                    {
                        xTotals[areaId] += x;
                        yTotals[areaId] += y;
                        counts[areaId] += 1;

                        locationLists[areaId] = locationLists[areaId] ?? new List<Location>();
                        locationLists[areaId].Add(new Location { X = x, Y = y });
                    }
                }
            }

            // Build mesas first - ramps need mesa info in their constructor,
            // and for convenience mesas can add neighbors post-construction
            for (byte areaId = 1; areaId <= maxAreaId; areaId++)
            {
                // This part can really be done once for both ramps and mesas
                var center = new Location
                {
                    X = xTotals[areaId] / counts[areaId],
                    Y = yTotals[areaId] / counts[areaId]
                };

                if (locationLists[areaId].Contains(center))
                {
                    centers[areaId] = center;
                }
                else
                {
                    centers[areaId] = center.GetClosest(locationLists[areaId]);
                }

                if (CanBuild(locationLists[areaId][0]))
                {
                    var height = this.HeightGrid[locationLists[areaId][0]];

                    mesas[areaId] = new Mesa(areaId, locationLists[areaId], centers[areaId], height);
                }
            }

            for (byte areaId = 1; areaId <= maxAreaId; areaId++)
            {
                if (!CanBuild(locationLists[areaId][0]))
                {
                    var topAndBottomNeighborIds =
                        neighbors
                            .Where(pair => pair.Item1 == areaId || pair.Item2 == areaId)
                            .Select(pair => pair.Item1 == areaId ? pair.Item2 : pair.Item1).ToList();

                    if (topAndBottomNeighborIds.Count != 2)
                    {
                        // This isn't really a ramp. Using the 'Edge' class for miscellaneous non-buildable
                        // areas for now. This does assume that an Edge won't connect to a Ramp.
                        var neighborMesas = topAndBottomNeighborIds.Select(id => mesas[id]).ToArray();
                        edges[areaId] = new Edge(areaId, locationLists[areaId], centers[areaId], neighborMesas);

                        foreach (var neighborMesa in neighborMesas)
                        {
                            neighborMesa.AddNeighbor(edges[areaId]);
                        }
                    }
                    else
                    {
                        var topAndBottomNeighbors = topAndBottomNeighborIds.Select(id => mesas[id]).OrderBy(mesa => mesa.Height).ToArray();

                        ramps[areaId] = new Ramp(areaId, locationLists[areaId], centers[areaId], topAndBottomNeighbors[1], topAndBottomNeighbors[0]);
                        mesas[topAndBottomNeighborIds[0]].AddNeighbor(ramps[areaId]);
                        mesas[topAndBottomNeighborIds[1]].AddNeighbor(ramps[areaId]);
                    }
                }
            }

            // At some point there might be 'mesas' that are adjacent, because we want to
            // subdivide large areas with multiple mining locations even if there's no ramp.
            foreach (var neighbor in neighbors)
            {
                if (mesas.ContainsKey(neighbor.Item1) && mesas.ContainsKey(neighbor.Item2))
                {
                    mesas[neighbor.Item1].AddNeighbor(mesas[neighbor.Item2]);
                    mesas[neighbor.Item2].AddNeighbor(mesas[neighbor.Item1]);
                }
            }

            return mesas.Values.Concat<Area>(ramps.Values).ToList();
        }

        /// <summary>
        /// Builds a list of resource deposits. Requires that the 'areas' and 'areaGrid' fields be set.
        /// </summary>
        private List<Deposit> GetDeposits(IReadOnlyList<Unit> units)
        {
            if (this.areas == null || this.areaGrid == null)
            {
                throw new InvalidOperationException();
            }
            
            var resourcesByArea = units.Where(u => u.IsMineralDeposit || u.IsVespeneGeyser)
                                       .GroupBy(m => areaGrid[(int)m.X, (int)m.Y]);

            var deposits = new List<Deposit>();

            foreach (var resourceArea in resourcesByArea)
            {
                var resources = resourceArea.ToList();

                while (resources.Count > 0)
                {
                    var depositResources = new List<Unit>();
                    var nextResource = resources[0];
                    resources.RemoveAt(0);
                    
                    while (nextResource != null)
                    {
                        depositResources.Add(nextResource);
                        resources.Remove(nextResource);

                        nextResource = resources.FirstOrDefault(r => depositResources.Any(d => r.GetDistance(d) < 5f));
                    }

                    var area = this.areas.First(a => a.Id == areaGrid[(int)depositResources[0].X, (int)depositResources[0].Y]);
                    var center = new Location
                    {
                        X = (int)(depositResources.Sum(u => u.X) / depositResources.Count),
                        Y = (int)(depositResources.Sum(u => u.Y) / depositResources.Count)
                    };

                    deposits.Add(new Deposit(area, center, depositResources));
                }
            }

            return deposits;
        }
        
        public List<Deposit> GetControlledDeposits(List<Building> bases)
        {
            // TODO: Allow less-orthodox base placement? This assumes they will always be at the center of the minerals, basically.
            // TODO: Stop using magic numbers for "very close to" everywhere.
            return this.Deposits.Where(d => bases.Any(b => b.GetDistance(d.Center) < 10f)).ToList();
        }

        /// <summary>
        /// Determines which areas are neighbors of other areas.
        /// </summary>
        /// <param name="areas">A map representation where each contiguous area has the same number (i.e. the output of <see cref="GetAreaGrid"/>).</param>
        /// <returns>A distinct set of neighbor relationships as byte-tuples (with the lower-value id coming first in each tuple).</returns>
        private static HashSet<(byte, byte)> GetNeighbors(MapArray<byte> areas)
        {
            /* Neighbors are areas with bordering spaces. I'm pretty sure we don't
             * have to worry about diagonals because I don't think they come up specifically,
             * and I don't think you can move between two diagonal neighbor spaces if there
             * are no other open spaces around.
             * 
             * As such, this just goes from left-to-right in each row and bottom-to-top
             * in each column and whenever the adjacent numbers are different and non-zero,
             * that's a neighboring relationship.
             */
            var neighbors = new HashSet<(byte, byte)>();

            // TODO: Get rid of code duplication
            for (var x = 0; x < areas.Size.X; x++)
            {
                for (var y = 1; y < areas.Size.Y; y++)
                {
                    if (areas[x, y - 1] != 0 && areas[x, y] != 0 && areas[x, y - 1] != areas[x, y])
                    {
                        var first = areas[x, y - 1];
                        var second = areas[x, y];

                        // Consistently put the lesser number first to avoid duplication
                        if (first < second)
                        {
                            neighbors.Add((first, second));
                        }
                        else
                        {
                            neighbors.Add((second, first));
                        }
                    }
                }
            }

            for (var y = 0; y < areas.Size.Y; y++)
            {
                for (var x = 1; x < areas.Size.X; x++)
                {
                    if (areas[x - 1, y] != 0 && areas[x, y] != 0 && areas[x - 1, y] != areas[x, y])
                    {
                        var first = areas[x - 1, y];
                        var second = areas[x, y];

                        // Consistently put the lesser number first to avoid duplication
                        if (first < second)
                        {
                            neighbors.Add((first, second));
                        }
                        else
                        {
                            neighbors.Add((second, first));
                        }
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Builds a representation of the map where each distinct area has a different value.
        /// </summary>
        /// <param name="mapData">The representation of the map. (Only the size, pathing grid, and placement grid are used.)</param>
        /// <returns>A represntation of the map where all spaces that are the same 'area' have the same numeric value.
        /// Locations that are not accessible to ground units have a value of 0.</returns>
        private MapArray<byte> GetAreaGrid()
        {
            /* Pick a starting location and fan out to adjacent locations.
             * 
             * If it's a buildable zone, find all spots that are also buildable,
             * adjacent, and at the same height. This will be a 'mesa'.
             * 
             * If it's not buildable but it can be traversed then it's a ramp,
             * find all adjacent spots that can also be traversed and are not buildable.
             *
             *  We can probably use the starting locations on the map as safe places to begin,
             *  since they'll be base locations.
             */

            // TODO: Add support for "islands" - this mechanism will only capture areas connected to the starting location

            // Resulting array of areas
            MapArray<byte> areas = new MapArray<byte>(this.Size);

            // Area id - will be incremented as we move to other areas
            // (and before assigning the first area as well)
            byte currentId = 0;

            var locations = new HashSet<Location>();
            var otherAreaLocations = new HashSet<Location>();

            var startPosition = this.Raw.StartLocations[0];

            otherAreaLocations.Add(new Location { X = (int)startPosition.X, Y = (int)startPosition.Y });

            while (otherAreaLocations.Count > 0)
            {
                var lastLocation = otherAreaLocations.First();
                otherAreaLocations.Remove(lastLocation);
                currentId++;
                areas[lastLocation] = currentId;

                AddAdjacentLocations(lastLocation, locations, areas);

                while (locations.Count > 0)
                {
                    var location = locations.First();
                    locations.Remove(location);
                    otherAreaLocations.Remove(location);

                    if (CanBuild(lastLocation) == CanBuild(location))
                    {
                        areas[location] = currentId;
                        AddAdjacentLocations(location, locations, areas);
                    }
                    else
                    {
                        otherAreaLocations.Add(location);
                    }
                }
            }

            return areas;
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
