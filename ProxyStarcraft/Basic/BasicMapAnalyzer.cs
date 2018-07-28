using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Maps;
using ProxyStarcraft.Basic.Maps;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class BasicMapAnalyzer : IMapAnalyzer<BasicMapData>
    {
        private readonly int maxAreaSize;

        public BasicMapAnalyzer() : this(int.MaxValue)
        {
        }

        public BasicMapAnalyzer(int maxAreaSize)
        {
            this.maxAreaSize = maxAreaSize;
        }

        public BasicMapData Get(BasicMapData prior, Map map)
        {
            var deposits = GetDeposits(map.StructuresAndDeposits, prior.Areas, prior.AreaGrid);
            return new BasicMapData(prior.Areas, prior.AreaGrid, deposits);
        }

        public BasicMapData GetInitial(Map map)
        {
            var areaGrid = GetAreaGrid(map);
            var areas = GetAreas(map, areaGrid);
            var deposits = GetDeposits(map.StructuresAndDeposits, areas, areaGrid);
            return new BasicMapData(areas, areaGrid, deposits);
        }

        /// <summary>
        /// Builds a list of <see cref="Area"/>s that have references to their neighbors.
        /// </summary>
        private List<Area> GetAreas(Map map, MapArray<byte> areaGrid) // TODO: Break up this function
        {
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

            for (var x = 0; x < map.Size.X; x++)
            {
                for (var y = 0; y < map.Size.Y; y++)
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

                if (map.CanBuild(locationLists[areaId][0]))
                {
                    var height = map.HeightGrid[locationLists[areaId][0]];

                    mesas[areaId] = new Mesa(areaId, locationLists[areaId], centers[areaId], height);
                }
            }

            for (byte areaId = 1; areaId <= maxAreaId; areaId++)
            {
                if (!map.CanBuild(locationLists[areaId][0]))
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
        /// Builds a representation of the map where each distinct area has a different value.
        /// </summary>
        /// <returns>A represntation of the map where all spaces that are the same 'area' have the same numeric value.
        /// Locations that are not accessible to ground units have a value of 0.</returns>
        private MapArray<byte> GetAreaGrid(Map map)
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
            MapArray<byte> areas = new MapArray<byte>(map.Size);

            // Area id - will be incremented as we move to other areas
            // (and before assigning the first area as well)
            byte currentId = 0;

            var locations = new HashSet<Location>();
            var otherAreaLocations = new HashSet<Location>();

            var startPosition = map.Raw.StartLocations[0];

            otherAreaLocations.Add(new Location { X = (int)startPosition.X, Y = (int)startPosition.Y });

            while (otherAreaLocations.Count > 0)
            {
                var lastLocation = otherAreaLocations.First();
                otherAreaLocations.Remove(lastLocation);
                currentId++;
                areas[lastLocation] = currentId;

                AddAdjacentLocations(map, lastLocation, locations, areas, false);

                while (locations.Count > 0)
                {
                    var location = locations.First();
                    locations.Remove(location);
                    otherAreaLocations.Remove(location);

                    if (map.CanBuild(lastLocation) == map.CanBuild(location))
                    {
                        areas[location] = currentId;
                        AddAdjacentLocations(map, location, locations, areas, false);
                    }
                    else
                    {
                        otherAreaLocations.Add(location);
                    }
                }
            }

            

            return areas;
        }

        /// <summary>
        /// Breaks large areas up into smaller areas until no area is larger than the specified limit.
        /// 
        /// For each large area - find the pair of border spaces that have the largest difference
        /// between their linear distance and their distance around the perimeter, and use that
        /// as a line to split the area.
        /// </summary>
        private void DecomposeLargeAreas(MapArray<byte> areas, int areaSizeLimit)
        {
            var areaCountsById = GetAreaCountsById(areas);

            while (areaCountsById.Values.Max() > areaSizeLimit)
            {
                var areaToBreakUp = areaCountsById.OrderByDescending(pair => pair.Value).First().Key;

                var singleArea = new MapArray<byte>(areas);

                for (var x = 0; x < singleArea.Size.X; x++)
                {
                    for (var y = 0; y < singleArea.Size.Y; y++)
                    {
                        singleArea[x, y] = singleArea[x, y] == areaToBreakUp ? (byte)1 : (byte)0;
                    }
                }

                // TODO: Extract this to a function
                var border = new MapArray<byte>(singleArea);

                var borderLocations = new List<Location>();

                for (var x = 0; x < singleArea.Size.X; x++)
                {
                    for (var y = 0; y < singleArea.Size.Y; y++)
                    {
                        var location = new Location { X = x, Y = y };
                        var adjacentLocations = location.AdjacentLocations(areas.Size, false);
                        if (adjacentLocations.All(l => singleArea[l] != 0) &&
                            adjacentLocations.Count == 4)
                        {
                            border[location] = 0;
                        }
                        else if (border[location] != 0)
                        {
                            borderLocations.Add(location);
                        }
                    }
                }
                
                borderLocations = GetBorderPath(borderLocations, border.Size).Locations.ToList();

                var bestReduction = 0.0;
                var bestStart = new Location { X = 0, Y = 0 };
                var bestEnd = new Location { X = 0, Y = 0 };

                for (var i = 0; i < borderLocations.Count; i++)
                {
                    var bestReductionForLocation = 0.0;
                    var bestDistanceAround = 0;
                    var bestDistanceAcross = 0.0;

                    for (var j = 0; j < borderLocations.Count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        // The border is in order now, although it wraps around
                        var distanceAround = Math.Min(Math.Abs(i - j), borderLocations.Count - Math.Abs(i - j));
                        var distanceAcross = borderLocations[i].DistanceTo(borderLocations[j]);

                        var reduction = distanceAround - distanceAcross;

                        if (reduction > bestReductionForLocation)
                        {
                            // TODO: Handle special case where this line goes outside of the area (i.e., ignore that result)
                            var possibleBorderLocations = GetNewBorderLocations(areas, areaToBreakUp, borderLocations[i], borderLocations[j]);

                            if (possibleBorderLocations.Any(l => areas[l] != areaToBreakUp))
                            {
                                continue;
                            }

                            bestReductionForLocation = reduction;
                            bestDistanceAround = distanceAround;
                            bestDistanceAcross = distanceAcross;

                            if (reduction > bestReduction)
                            {
                                bestReduction = reduction;
                                bestStart = borderLocations[i];
                                bestEnd = borderLocations[j];
                            }
                        }
                    }
                }

                if (bestReduction == 0.0)
                {
                    throw new NotImplementedException();
                }

                // Okay, we have a line to use to break up this area.
                // Draw it and determine which Locations make up the border.
                var newBorderLocations = GetNewBorderLocations(areas, areaToBreakUp, bestStart, bestEnd);

                var newAreaId = (byte)(areaCountsById.Keys.Max() + 1);

                foreach (var newBorderLocation in newBorderLocations)
                {
                    areas[newBorderLocation] = newAreaId;
                }
                
                // Flip one of the two sides of the border to the new id (doesn't really matter which one)
                foreach (var location in borderLocations)
                {
                    if (areas[location] == areaToBreakUp)
                    {
                        // Now that the new border is in place, this and all contiguous spaces
                        // that are part of the same area can switch to the new area.
                        var newAreaLocations = new List<Location>();
                        var newAreaLocationSet = new HashSet<Location>();
                        newAreaLocations.Add(location);
                        newAreaLocationSet.Add(location);

                        // TODO: Refactor to combine with BasicMapAnalyzer.AddAdjacentLocations
                        while (newAreaLocations.Count > 0)
                        {
                            var newAreaLocation = newAreaLocations.First();
                            newAreaLocations.Remove(newAreaLocation);

                            areas[newAreaLocation] = newAreaId;

                            foreach (var adjacentLocation in newAreaLocation.AdjacentLocations(areas.Size, false))
                            {
                                if (areas[adjacentLocation] == areaToBreakUp && !newAreaLocationSet.Contains(adjacentLocation))
                                {
                                    newAreaLocations.Add(adjacentLocation);
                                    newAreaLocationSet.Add(adjacentLocation);
                                }
                            }
                        }

                        break;
                    }
                }

                areaCountsById = GetAreaCountsById(areas);
            }
        }

        private BorderPath GetBorderPath(List<Location> border, Size2DI mapSize)
        {
            var remainingLocations = new List<Location>();
            var paths = new List<BorderPath>();
            var borderSet = new HashSet<Location>(border);

            foreach (var location in border)
            {
                var adjacentLocationCount = location.AdjacentLocations(mapSize).Where(borderSet.Contains).Count();

                if (location.Equals(new Location { X = 94, Y = 18 }))
                {
                    int test = 0;
                    test += 1;
                }

                if (adjacentLocationCount == 2)
                {
                    AddLocationToBorderPaths(paths, location);
                }
                else
                {
                    remainingLocations.Add(location);
                }
            }

            bool pathsUpdated = true;

            while (pathsUpdated)
            {
                pathsUpdated = false;

                for (var i = remainingLocations.Count - 1; i >= 0; i--)
                {
                    var location = remainingLocations[i];

                    foreach (var path in paths)
                    {
                        bool addToPath = false;
                        Location edge = path.Start;

                        foreach (var pathEdge in new[] { path.Start, path.End })
                        {
                            if (pathEdge.IsAdjacentTo(location))
                            {
                                var adjacentLocationCount = pathEdge.AdjacentLocations(mapSize).Where(borderSet.Contains).Count();
                                if (adjacentLocationCount == 2)
                                {
                                    addToPath = true;
                                    edge = pathEdge;
                                }
                            }
                        }

                        if (addToPath)
                        {
                            path.Add(location, edge);
                            remainingLocations.RemoveAt(i);
                            pathsUpdated = true;
                            break;
                        }
                    }
                }
            }

            // Let's just consider anything left in remainingLocations to be worthy of its own BorderPath
            foreach (var remainingLocation in remainingLocations)
            {
                paths.Add(new BorderPath(remainingLocation));
            }

            pathsUpdated = true;

            // Now, combine any paths where there's only one possibility.
            // (Actually, not sure if this is possible to occur yet.)
            // Presumably we're looking for a case where a path edge is only adjacent to one other path edge.
            while (pathsUpdated)
            {
                pathsUpdated = false;

                var pathEdges = new HashSet<Location>(paths.SelectMany(p => new[] { p.Start, p.End }));

                for (var i = paths.Count - 1; i >= 0; i--)
                {
                    var path = paths[i];

                    foreach (var currentPathEdge in new[] { path.Start, path.End })
                    {
                        var adjacentEdges = currentPathEdge.AdjacentLocations(mapSize).Where(pathEdges.Contains).Where(p => !path.Contains(p)).ToList();

                        if (adjacentEdges.Count == 1)
                        {
                            var pathToCombine = paths.Single(p => p.Contains(adjacentEdges[0]));
                            pathToCombine.Add(path, adjacentEdges[0], currentPathEdge);
                            paths.RemoveAt(i);
                            pathsUpdated = true;
                            break;
                        }
                    }

                    if (pathsUpdated)
                    {
                        break;
                    }
                }
            }

            if (paths.Count == 1)
            {
                return paths[0];
            }

            // The hard part! We were apparently left with multiple possibilities
            // and we need to try them until we find one that works. Will do later.
            return GetSingleBorderPathRecursive(paths);
        }

        private BorderPath GetSingleBorderPathRecursive(List<BorderPath> paths)
        {
            if (paths.Count == 0)
            {
                throw new ArgumentException("No border paths specified.");
            }

            if (paths.Count == 1)
            {
                var result = paths[0];
                if (result.Start.IsAdjacentTo(result.End))
                {
                    return result;
                }

                return null;
            }

            var newPaths = paths.Select(p => p.Copy()).ToList();

            var path = newPaths[0];
            newPaths.RemoveAt(0);

            var choicesToCombine = newPaths.Where(p => p.CanAdd(path)).ToList();
            if (choicesToCombine.Count == 0)
            {
                return null;
            }

            foreach (var choice in choicesToCombine)
            {
                var possibleEdgeCombinations = new[]
                {
            new[] { path.Start, choice.Start },
            new[] { path.Start, choice.End },
            new[] { path.End, choice.Start },
            new[] { path.End, choice.End }
        };

                foreach (var edgeCombination in possibleEdgeCombinations)
                {
                    var edge = edgeCombination[0];
                    var otherPathEdge = edgeCombination[1];

                    if (path.CanAdd(choice, edge, otherPathEdge))
                    {
                        var modifiedPath = path.Copy();
                        modifiedPath.Add(choice, edge, otherPathEdge);

                        var pathsWithChoiceApplied = newPaths.Where(p => p != choice).ToList();
                        pathsWithChoiceApplied.Add(modifiedPath);

                        var result = GetSingleBorderPathRecursive(pathsWithChoiceApplied);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        // Adds a location to a growing set of border paths.
        // If it is adjacent to one of the existing paths, it will be added to it.
        // If not, a new path will be started.
        // Assumes that the location only has two adjacent spaces, so it is safe to add anywhere possible.
        public void AddLocationToBorderPaths(List<BorderPath> paths, Location location)
        {
            foreach (var path in paths)
            {
                if (path.CanAdd(location))
                {
                    var edge = location.IsAdjacentTo(path.Start) ? path.Start : path.End;

                    path.Add(location, edge);
                    return;
                }
            }

            paths.Add(new BorderPath(new[] { location }));
        }

        /// <summary>
        /// Determines which Locations make up the new border used to split the given area along the given line.
        /// </summary>
        private List<Location> GetNewBorderLocations(MapArray<byte> areas, int areaId, Location start, Location end)
        {
            var results = new List<Location>();

            var deltaX = end.X - start.X;
            var deltaY = end.Y - start.Y;

            // Might just be a vertical line
            if (deltaX == 0)
            {
                var x = start.X;
                var minY = Math.Min(start.Y, end.Y);
                var maxY = Math.Max(start.Y, end.Y);

                for (int y = minY; y <= maxY; y++)
                {
                    results.Add(new Location { X = x, Y = y });
                }

                return results;
            }

            // Or a horizontal line
            if (deltaY == 0)
            {
                var y = start.Y;
                var minX = Math.Min(start.X, end.X);
                var maxX = Math.Max(start.X, end.X);

                for (int x = minX; x <= maxX; x++)
                {
                    results.Add(new Location { X = x, Y = y });
                }

                return results;
            }

            // Proper diagonal line, so we actually have to figure out some intersections,
            // Let's just figure out the slope and then determine what Y values it hits as it crosses each X boundary.
            if (end.X < start.X)
            {
                var temp = start;
                start = end;
                end = temp;
                deltaX = end.X - start.X;
            }

            var slope = (double)(end.Y - start.Y) / (end.X - start.X);

            var horizontalIntersections = new List<double>();

            for (var i = 0; i < deltaX; i++)
            {
                // Assuming the line goes from the middle of the location values (i.e., the integer value of the location plus 0.5),
                // then we can add 0.5 to get the first vertical boundary and 1.0 for each one after that.
                var y = (0.5 + i) * slope + 0.5 + start.Y;
                horizontalIntersections.Add(y);
            }

            Func<double, double> roundingFunction = slope > 0 ? (Func<double, double>)Math.Ceiling : (Func<double, double>)Math.Floor;

            var currentX = start.X;
            var currentY = start.Y;

            for (var i = 0; i < horizontalIntersections.Count; i++)
            {
                var nextY = (int)roundingFunction(horizontalIntersections[i]);

                var minY = Math.Min(currentY, nextY);
                var maxY = Math.Max(currentY, nextY);

                for (var y = minY; y <= maxY; y++)
                {
                    results.Add(new Location { X = currentX, Y = y });
                }

                currentX += 1;
                currentY = nextY;
            }

            for (var y = Math.Min(currentY, end.Y); y <= Math.Max(currentY, end.Y); y++)
            {
                results.Add(new Location { X = currentX, Y = y });
            }

            return results;
        }

        /// <summary>
        /// Builds a list of resource deposits. Requires that the 'areas' and 'areaGrid' fields be set.
        /// </summary>
        private List<Deposit> GetDeposits(IReadOnlyList<Unit> units, IReadOnlyList<Area> areas, MapArray<byte> areaGrid)
        {
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

                    var area = areas.First(a => a.Id == areaGrid[(int)depositResources[0].X, (int)depositResources[0].Y]);
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

        private void AddAdjacentLocations(Map map, Location location, HashSet<Location> locations, MapArray<byte> areas)
        {
            AddAdjacentLocations(map, location, locations, areas, true);
        }

        private void AddAdjacentLocations(Map map, Location location, HashSet<Location> locations, MapArray<byte> areas, bool includeDiagonals)
        {
            foreach (var adjacentLocation in AdjacentLocations(map, location, includeDiagonals))
            {
                if (areas[adjacentLocation] == 0 &&
                    (map.CanBuild(adjacentLocation) || map.CanTraverse(adjacentLocation)))
                {
                    locations.Add(adjacentLocation);
                }
            }
        }

        private IEnumerable<Location> AdjacentLocations(Map map, Location location, bool includeDiagonals)
        {
            var results = new List<Location>();

            var xVals = new List<int> { location.X - 1, location.X, location.X + 1 };
            xVals.Remove(-1);
            xVals.Remove(map.Size.X);

            var yVals = new List<int> { location.Y - 1, location.Y, location.Y + 1 };
            yVals.Remove(-1);
            yVals.Remove(map.Size.Y);

            foreach (var x in xVals)
            {
                foreach (var y in yVals)
                {
                    if (x != location.X || y != location.Y)
                    {
                        if (includeDiagonals || x == location.X || y == location.Y)
                        {
                            yield return new Location { X = x, Y = y };
                        }
                    }
                }
            }
        }

        private Dictionary<int, int> GetAreaCountsById(MapArray<byte> areas)
        {
            var areaCountsById = new Dictionary<int, int>();

            foreach (var a in areas.Data)
            {
                if (a == 0)
                {
                    continue;
                }

                if (!areaCountsById.ContainsKey(a))
                {
                    areaCountsById.Add(a, 0);
                }

                areaCountsById[a] = areaCountsById[a] + 1;
            }

            return areaCountsById;
        }
    }
}
