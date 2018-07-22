<Query Kind="Program">
  <Reference Relative="..\ProxyStarcraft\bin\Debug\Google.Protobuf.dll">D:\Dev\proxy-starcraft-private\ProxyStarcraft\bin\Debug\Google.Protobuf.dll</Reference>
  <Reference Relative="..\ProxyStarcraft\bin\Debug\ProxyStarcraft.dll">D:\Dev\proxy-starcraft-private\ProxyStarcraft\bin\Debug\ProxyStarcraft.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Drawing.dll</Reference>
  <Reference Relative="..\ProxyStarcraft\bin\Debug\System.ValueTuple.dll">D:\Dev\proxy-starcraft-private\ProxyStarcraft\bin\Debug\System.ValueTuple.dll</Reference>
  <Namespace>ProxyStarcraft</Namespace>
  <Namespace>ProxyStarcraft.Proto</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Runtime.InteropServices</Namespace>
</Query>

private const string PathingData = "D:\\Temp\\pathing.dat";
private const string PlacementData = "D:\\Temp\\placement.dat";
private const string HeightData = "D:\\Temp\\height.dat";
private const string AreaData = "D:\\Temp\\areas.dat";
private const string NeighborData = "D:\\Temp\\neighbors.dat";
private const string OpenSpaceData = "D:\\Temp\\openspace.dat";
private static readonly Size2DI MapSize = new Size2DI { X = 200, Y = 176 };
private static readonly Random random = new Random();

void Main()
{
//	var openSpaceBytes = File.ReadAllBytes(OpenSpaceData);
//	var openSpace = new MapArray<byte>(openSpaceBytes, MapSize);
	
	var areaBytes = File.ReadAllBytes(AreaData);
	var areas = new MapArray<byte>(areaBytes, MapSize);
	
	AroundVersusAcross(areas, 1000);
	
	//RisingWater(openSpace);
	//var pathingData = new MapArray<byte>(File.ReadAllBytes(PathingData), MapSize);
	//WaypointPressure(pathingData, 20, new Location { X = 40, Y = 40 }, 50);
	
//	var pathing = new MapArray<byte>(File.ReadAllBytes(PathingData), MapSize);
//	Display(pathing);
//	Display(PostProcess(paths));
//	Display(GetWaypoints(paths));
//	GetGrayscaleImage(openSpace).Save("D:\\Temp\\openspace.bmp");
//	GetGrayscaleImage(PostProcess(paths)).Save("D:\\Temp\\paths.bmp");
//	GetGrayscaleImage(GetWaypoints(paths)).Save("D:\\Temp\\waypoints.bmp");
}

// Approach: Take only the outer spaces in each area. For each combination of two
// such spaces, figure out the closest way to get around the area from one to the
// other, and compare to the closest direct path through the area. If the discrepency
// is high enough, draw a line and split the area. Avoid splitting on spaces that are
// already boundaries between two reachable areas.
public void AroundVersusAcross(MapArray<byte> areas, int areaSizeLimit)
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
				var adjacentLocations = AdjacentLocations(location, false);
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
		
		var borderImage = GetImage(border);
		//borderImage.Save("D:/Temp/border.bmp");
		Display(border);
		
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
		
		GetImage(areas).Dump();
		
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

private void AddAdjacentLocations(Map map, Location location, HashSet<Location> locations, MapArray<byte> areas, bool includeDiagonals)
{
  	foreach (var adjacentLocation in location.AdjacentLocations(areas.Size, includeDiagonals))
  	{
      	if (areas[adjacentLocation] == 0 &&
        	(map.CanBuild(adjacentLocation) || map.CanTraverse(adjacentLocation)))
      	{
        	locations.Add(adjacentLocation);
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

// Determines which Locations make up the new border used to split the given area along the given line
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

/* Okay, this is turning out to be harder than I thought to do properly.
Suppose we treat adjacency as a graph edge between nodes that are Locations.
We want to make one continous path through the graph that hits every Location
once.

A lot of the graph is very simple - many locations have exactly two neighbors
and therefore no decisions need to be made.

For cases where that isn't true, we start running into multiple possibilities
and we have to probably just look at all the different ones.

This is not (so far at least) robust to scenarios where the area gets down
to 1 location width (which can apparently happen, although I think it's only
in cases where there's a destructable object right now). So far I have only
seen that happen for small areas.
*/
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

// Helper class for putting together strands of borders. May be useful for other things in the
// future as well ("Border" may be a misnomer). Does not allow repeat entries of the same location.
public class BorderPath
{
	private List<Location> locations;
	private HashSet<Location> locationSet;
	
	public BorderPath(Location location)
	{
		this.locations = new List<Location>() { location };
		this.locationSet = new HashSet<Location>() { location };
	}
	
	public BorderPath(IEnumerable<Location> locations)
	{
		this.locations = new List<Location>(locations);
		this.locationSet = new HashSet<Location>(locations);
		
		for (var i = 1; i < this.locations.Count; i++)
		{
			if (!this.locations[i - 1].IsAdjacentTo(this.locations[i]))
			{
				throw new ArgumentException("Locations must already be adjacent and in order.");
			}
		}
	}
	
	public BorderPath Copy()
	{
		return new BorderPath(new List<Location>(this.locations));
	}
	
	public IReadOnlyList<Location> Locations
	{
		get
		{
			return new List<Location>(this.locations);
		}
	}
	
	public Location Start
	{
		get
		{
			return this.locations[0];
		}
	}
	
	public Location End
	{
		get
		{
			return this.locations[this.locations.Count - 1];
		}
	}
	
	public bool Contains(Location location)
	{
		return this.locationSet.Contains(location);
	}
	
	public bool CanAdd(Location location)
	{
		return !this.Contains(location) && (this.Start.IsAdjacentTo(location) || this.End.IsAdjacentTo(location));
	}
	
	public void Add(Location location, Location edge)
	{
		if (this.locations.Count == 0)
		{
			this.locations.Add(location);
			this.locationSet.Add(location);
			return;
		}
		
		if (this.Contains(location))
		{
			throw new ArgumentException(string.Format("Location {0} already in path.", location));
		}
		
		if (!location.IsAdjacentTo(edge))
		{
			throw new ArgumentException("Location is not adjacent to specified edge of path.");
		}
		
		if (this.End == edge)
		{
			this.locations.Add(location);
			this.locationSet.Add(location);
			return;
		}
		
		if (this.Start == edge)
		{
			this.locations.Insert(0, location);
			this.locationSet.Add(location);
			return;
		}
		
		throw new ArgumentException(string.Format("Edge {0} is not either end of this path ({1} or {2}).", edge, this.Start, this.End));
	}
	
	public bool CanAdd(BorderPath path)
	{
		if (this.locations.Any(path.Contains))
		{
			throw new ArgumentException("Paths have overlapping sets of locations.");
		}
		
		return
			this.Start.IsAdjacentTo(path.Start) ||
			this.End.IsAdjacentTo(path.End) ||
			this.Start.IsAdjacentTo(path.End) ||
			this.End.IsAdjacentTo(path.Start);
	}
	
	public bool CanAdd(BorderPath path, Location edge, Location otherPathEdge)
	{
		if (this.Start != edge && this.End != edge)
		{
			return false;
		}
		
		if (path.Start != otherPathEdge && path.End != otherPathEdge)
		{
			return false;
		}
		
		if (!edge.IsAdjacentTo(otherPathEdge))
		{
			return false;
		}
		
		return true;
	}
	
	public void Add(BorderPath path, Location edge, Location otherPathEdge)
	{
		if (this.Start != edge && this.End != edge)
		{
			throw new ArgumentException(string.Format("{0} is not one of this path's edges (either {1} or {2}).", edge, this.Start, this.End));
		}
		
		if (path.Start != otherPathEdge && path.End != otherPathEdge)
		{
			throw new ArgumentException(string.Format("{0} is not one of the added path's edges (either {1} or {2}).", otherPathEdge, path.Start, path.End));
		}
		
		if (!edge.IsAdjacentTo(otherPathEdge))
		{
			throw new ArgumentException(string.Format("Specified edges {0} and {1} are not adjacent.", edge, otherPathEdge));
		}
		
		// If they are start-to-start or end-to-end then reverse the new one,
		// except in the special case where the start and the end of this one
		// are equal (i.e. it has only 1 item)
		if (this.Locations.Count > 1 &&
			(this.Start == edge && path.Start == otherPathEdge) ||
			(this.End == edge && path.End == otherPathEdge))
		{
			path = path.Reverse();
		}
		
		if (this.End == edge)
		{
			this.locations.AddRange(path.locations);
		}
		else
		{
			this.locations.InsertRange(0, path.locations);
		}
		
		path.locations.ForEach(l => this.locationSet.Add(l));
	}

	public BorderPath Reverse()
	{
		return new BorderPath(Enumerable.Reverse(this.locations));
	}
}

// Approach: model the open-space distance as altitude and repeatedly raise the water level.
// Anything that gets 'cut off' by the rising water, within certain paramters, is a different area.
public void RisingWater(MapArray<byte> openSpace)
{
	for (var i = 0; i < 20; i++)
	{
		var risingWater = new MapArray<byte>(openSpace);
		for (var x = 0; x < risingWater.Size.X; x++)
		{
			for (var y = 0; y < risingWater.Size.Y; y++)
			{
				if (risingWater[x, y] < i)
				{
					risingWater[x, y] = 0;
				}
			}
		}
		
		Display(risingWater);
	}
}

// TODO: Approach: drop a bunch of point objects onto the map. Have them repel each other.
// Have unpassable spaces repel them also. Allow them to flow until some equilibrium is reached.
public void WaypointPressure(MapArray<byte> pathingData, int waypointCount, Location startLocation, int steps)
{
	// Pre-bake the forces from unpassable spaces,
	// rendering them only at per-space granularity,
	// so we don't have to keep recalculating them.
	MapArray<float> pressureX = new MapArray<float>(pathingData.Size);
	MapArray<float> pressureY = new MapArray<float>(pathingData.Size);
	
	var waypoints = new List<Point2D>();
	
	for (var i = 0; i < waypointCount; i++)
	{
		var x = startLocation.X + random.NextDouble();
		var y = startLocation.Y + random.NextDouble();
		
		waypoints.Add(new Point2D { X = (float)x, Y = (float)y });
	}
	
	for (var s = 0; s < steps; s++)
	{
		var nextWaypoints = new List<Point2D>();
		
		for (var i = 0; i < waypointCount; i++)
		{
			var w = waypoints[i];
			var totalForceX = 0f;
			var totalForceY = 0f;
			
			// Get force from nearby waypoints
			for (var j = 0; j < waypointCount; j++)
			{
				if (j == i)
				{
					continue;
				}
				
				var distance = Distance(w, waypoints[j]);
				
				//if (distance < 10.0)
				//{
					var totalForce = new Vector { X = w.X - waypoints[j].X, Y = w.Y - waypoints[j].Y }.Unit() / (distance * distance);
					
					totalForceX += totalForce.X;
					totalForceY += totalForce.Y;
				//}
			}
			var deltaX = totalForceX / 10f;
			var deltaY = totalForceY / 10f;
			var newX = (float)w.X + deltaX;
			var newY = (float)w.Y + deltaY;
			
			if (float.IsNaN(newX) || float.IsNaN(newY))
			{
				throw new InvalidOperationException();
			}
			
			newX = Math.Max(newX, .001f);
			newX = Math.Min(newX, pathingData.Size.X - .001f);
			newY = Math.Max(newY, .001f);
			newY = Math.Min(newY, pathingData.Size.Y - .001f);
			
			if (float.IsNaN(newX) || float.IsNaN(newY))
			{
				throw new InvalidOperationException();
			}
			
			for (int m = (int)Math.Ceiling(Math.Max(Math.Abs(deltaX), Math.Abs(deltaY))); m > 0; m--)
			{
				// Guard against nonpassable spaces
				if (w.X + m < pathingData.Size.X && pathingData[(int)w.X + m, (int)w.Y] != 0)
				{
					newX = Math.Min((int)waypoints[i].X + .999f, newX);
				}
				if (w.X - m > 0 && pathingData[(int)w.X - m, (int)w.Y] != 0)
				{
					newX = Math.Max((int)w.X + .001f, newX);
				}
				if (w.Y + m < pathingData.Size.Y && pathingData[(int)w.X, (int)w.Y + m] != 0)
				{
					newY = Math.Min((int)w.Y + .999f, newY);
				}
				if (w.Y - m > 0 && pathingData[(int)w.X, (int)w.Y - m] != 0)
				{
					newY = Math.Max((int)w.Y + .001f, newY);
				}
			}
			
			if (float.IsNaN(newX) || float.IsNaN(newY))
			{
				throw new InvalidOperationException();
			}
			
			nextWaypoints.Add(new Point2D { X = newX, Y = newY });
		}
		
		waypoints = nextWaypoints;
		
		MapArray<byte> locations = new MapArray<byte>(pathingData.Size);
		
		foreach (var waypoint in waypoints)
		{
			locations[(int)waypoint.X, (int)waypoint.Y] += 1;
		}
		
		Display(locations);
	}
}

public struct Vector
{
	public float X;
	
	public float Y;
	
	public float Magnitude
	{
		get
		{
			return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
		}
	}
	
	public Vector Unit()
	{
		var magnitude = this.Magnitude;
		return new Vector { X = this.X / magnitude, Y = this.Y / magnitude };
	}
	
	public static Vector operator /(Vector self, float ratio)
	{
		return new Vector { X = self.X / ratio, Y = self.Y / ratio };
	}
	
	public static Vector operator *(Vector self, float ratio)
	{
		return new Vector { X = self.X * ratio, Y = self.Y * ratio };
	}
}

public float Sigmoid(float x)
{
	return (float)(2f / (1f + Math.Pow(Math.E, -1f * x)) - 1f);
}

public float Distance(Point2D first, Point2D second)
{
	return (float)Math.Sqrt((first.X - second.X) * (first.X - second.X) + (first.Y - second.Y) * (first.Y - second.Y));
}

public struct Size
{
	public int Width;
	
	public int Height;
}

public MapArray<byte> PostProcess(MapArray<byte> paths)
{
	var result = new MapArray<byte>(paths);
	
	for (var x = 0; x < MapSize.X; x++)
	{
		for (var y = 0; y < MapSize.Y; y++)
		{
			var location = new Location { X = x, Y = y };
			
			if (paths[location] != 0 && !AdjacentLocations(location).Any(l => paths[l] != 0))
			{
				result[location] = 0;
			}
		}
	}
	
	return result;
}

public MapArray<byte> GetWaypoints(MapArray<byte> paths)
{
	var result = new MapArray<byte>(paths.Size);
	
	for (var x = 0; x < MapSize.X; x++)
	{
		for (var y = 0; y < MapSize.Y; y++)
		{
			var location = new Location { X = x, Y = y };
			
			if (paths[location] != 0 && NonZeroNeighborCount(location, paths) > 2)
			{
				result[location] = 255;
			}
		}
	}
	
	return result;
}

public void DrawLine(MapArray<byte> image, Location start, Location end)
{
	double startX = start.X + 0.5;
	double startY = start.Y + 0.5;
	
	double endX = end.X + 0.5;
	double endY = end.Y + 0.5;
	
	double distance = Math.Sqrt((startX - endX) * (startX - endX) + (startY - endY) * (startY - endY));
	
	double stepX = (endX - startX) / (2 * distance);
	double stepY = (endY - startY) / (2 * distance);
	
	double nextX = startX;
	double nextY = startY;
	
	for (int i = 0; i < 2 * distance; i++)
	{
		image[(int)nextX, (int)nextY] = byte.MaxValue;
		nextX += stepX;
		nextY += stepY;
	}
}

#region Rendering

private static void Display(MapArray<byte> data)
{
	Display(data, true);
}

private static void Display(MapArray<byte> data, bool grayscale)
{
	if (grayscale)
	{
		GetGrayscaleImage(data).Dump();
	}
	else
	{
		GetImage(data).Dump();
	}
}

private static Bitmap GetImage(MapArray<byte> data)
{
	var bitmap = new Bitmap(
	data.Size.X,
	data.Size.Y,
	data.Size.X,
	PixelFormat.Format8bppIndexed,
	Marshal.UnsafeAddrOfPinnedArrayElement(data.Data, 0));
	
	// For some reason the default indexed palette has a bunch of identical transparent entries in it, but they aren't the last ones
	var last = bitmap.Palette.Entries.Length - 1;
	
	var palette = bitmap.Palette;
	
	for (var i = 0; i < last; i++)
	{
		if (palette.Entries[i] == System.Drawing.Color.FromArgb(0, 0, 0, 0))
		{
			palette.Entries[i] = bitmap.Palette.Entries[last];
			palette.Entries[last] = System.Drawing.Color.FromArgb(0, 0, 0, 0);
			last--;
		}
	}
	
	bitmap.Palette = palette;
	
	return bitmap;
}

private static Bitmap GetGrayscaleImage(MapArray<byte> data)
{
	var maxValue = data.Data.Max();
	var step = byte.MaxValue / maxValue;
	
	var bitmap = new Bitmap(
		data.Size.X,
		data.Size.Y,
		data.Size.X,
		PixelFormat.Format8bppIndexed,
		Marshal.UnsafeAddrOfPinnedArrayElement(data.Data, 0));
	
	var palette = bitmap.Palette;
	
	for (var i = 0; i < maxValue; i++)
	{
		palette.Entries[i] = System.Drawing.Color.FromArgb(i * step, i * step, i * step);
	}
	
	bitmap.Palette = palette;
	
	return bitmap;
}

#endregion

private List<Location> NonZeroNeighbors(Location location, MapArray<byte> data)
{
	return AdjacentLocations(location).Where(l => data[l] != 0).ToList();
}

private int NonZeroNeighborCount(Location location, MapArray<byte> data)
{
	return NonZeroNeighbors(location, data).Count;
}

private static IReadOnlyList<Location> AdjacentLocations(Location location)
{
	return AdjacentLocations(location, true);
}

private static IReadOnlyList<Location> AdjacentLocations(Location location, bool includeDiagonals)
{
	var results = new List<Location>();
	
	var xVals = new List<int> { location.X - 1, location.X, location.X + 1 };
	xVals.Remove(-1);
	xVals.Remove(MapSize.X);
	
	var yVals = new List<int> { location.Y - 1, location.Y, location.Y + 1 };
	yVals.Remove(-1);
	yVals.Remove(MapSize.Y);
	
	foreach (var x in xVals)
	{
		foreach (var y in yVals)
		{
			if (x != location.X || y != location.Y)
			{
				if (includeDiagonals || x == location.X || y == location.Y)
				{
					results.Add(new Location { X = x, Y = y });
				}
			}
		}
	}
	
	return results;
}