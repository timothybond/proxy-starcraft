using System;
using System.Collections.Generic;

namespace ProxyStarcraft.Basic
{
    public class BasicPlacementStrategy : IPlacementStrategy
    {
        public Location GetPlacement(BuildingType building, GameState gameState)
        {
            // We're going to make some dumb assumptions here:
            // 1. There's only one Command Center (or equivalent)
            // 1. We'd like to build this building very near where the Command Center currently is
            // 2. As long as we don't block anything, it doesn't matter where it goes
            var mainBaseLocation = GetMainBaseLocation(gameState);

            var size = gameState.Translator.GetBuildingSize(building);

            var locations = new HashSet<Location> { mainBaseLocation };
            var pastLocations = new HashSet<Location>();
            var nextLocations = new HashSet<Location>();

            // This is essentially a breadth-first search of map locations
            while (locations.Count > 0)
            {
                foreach (var location in locations)
                {
                    if (gameState.MapData.CanBuild(size, location.X, location.Y))
                    {
                        return location;
                    }

                    var adjacentLocations = location.AdjacentLocations(gameState.MapData.Size);

                    foreach (var adjacentLocation in adjacentLocations)
                    {
                        if (!pastLocations.Contains(adjacentLocation) && !locations.Contains(adjacentLocation) && gameState.MapData.CanTraverse(adjacentLocation))
                        {
                            nextLocations.Add(adjacentLocation);
                        }
                    }

                    pastLocations.Add(location);
                }

                locations = nextLocations;
                nextLocations = new HashSet<Location>();
            }

            throw new InvalidOperationException("Cannot find placement location anywhere on map.");
        }

        private Location GetMainBaseLocation(GameState gameState)
        {
            foreach (var unit in gameState.Units)
            {
                if (unit.Type == TerranBuildingType.CommandCenter ||
                    unit.Type == TerranBuildingType.OrbitalCommand ||
                    unit.Type == TerranBuildingType.PlanetaryFortress ||
                    unit.Type == ProtossBuildingType.Nexus ||
                    unit.Type == ZergBuildingType.Hatchery ||
                    unit.Type == ZergBuildingType.Lair ||
                    unit.Type == ZergBuildingType.Hive)
                {
                    // They're all 5x5 so the center is 2 spaces up and to the right of (X, Y) as we're marking it
                    return new Location { X = (int)unit.X + 2, Y = (int)unit.Y + 2 };
                }
            }

            throw new InvalidOperationException("No Command Center / Hatchery / Nexus or equivalent found.");
        }
    }
}
