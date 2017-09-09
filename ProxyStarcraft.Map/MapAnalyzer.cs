using System;
using System.Collections.Generic;
using System.Linq;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Map
{
    public class MapAnalyzer
    {
        public static MapArray<byte> GetAreas(MapData mapData)
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
            MapArray<byte> areas = new MapArray<byte>(mapData.Size);

            // Area id - will be incremented as we move to other areas
            // (and before assigning the first area as well)
            byte currentId = 0;

            var locations = new HashSet<Location>();
            var otherAreaLocations = new HashSet<Location>();

            var startPosition = mapData.Raw.StartLocations[0];
            var otherStartPosition = mapData.Raw.StartLocations[1];
            
            otherAreaLocations.Add(new Location { X = (int)startPosition.X, Y = (int)startPosition.Y });
            
            while (otherAreaLocations.Count > 0)
            {
                locations = new HashSet<Location>();

                var lastLocation = locations.First();
                locations.Remove(lastLocation);
                currentId++;
                areas[lastLocation] = currentId;

                AddAdjacentLocations(lastLocation, locations, mapData, areas);

                while (locations.Count > 0)
                {
                    var location = locations.First();
                    locations.Remove(location);
                    otherAreaLocations.Remove(location);

                    if (mapData.CanBuild(lastLocation) == mapData.CanBuild(location))
                    {
                        areas[location] = currentId;
                        AddAdjacentLocations(location, locations, mapData, areas);
                    }
                    else
                    {
                        otherAreaLocations.Add(location);
                    }
                }
            }

            return areas;
        }
        
        private static void AddAdjacentLocations(Location location, HashSet<Location> locations, MapData mapData, MapArray<byte> areas)
        {
            foreach (var adjacentLocation in AdjacentLocations(location, mapData.Size))
            {
                if (areas[adjacentLocation] == 0 &&
                    (mapData.CanBuild(adjacentLocation) || mapData.CanTraverse(adjacentLocation)))
                {
                    locations.Add(adjacentLocation);
                }
            }
        }

        private static IEnumerable<Location> AdjacentLocations(Location location, Size2DI size)
        {
            var results = new List<Location>();

            var xVals = new List<int> { location.X - 1, location.X, location.X + 1 };
            xVals.Remove(-1);
            xVals.Remove(size.X);

            var yVals = new List<int> { location.Y - 1, location.Y, location.Y + 1 };
            yVals.Remove(-1);
            yVals.Remove(size.Y);

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
    }
}
