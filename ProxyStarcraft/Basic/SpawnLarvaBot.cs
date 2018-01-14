using System.Collections.Generic;
using ProxyStarcraft.Proto;
using System.Linq;

namespace ProxyStarcraft.Basic
{
    /// <summary>
    /// A bot to handle Queens' <see cref="SpecialAbilityType.SpawnLarva"/> micro.
    /// </summary>
    public class SpawnLarvaBot : IBot
    {
        private float withinRadius;
        public SpawnLarvaBot(float excludeOutsideOfRadius = 99999f)
        {
            this.withinRadius = excludeOutsideOfRadius;
        }

        public Race Race => Race.Zerg;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            var queens = new List<ZergUnit>();
            var hatcheries = new List<ZergBuilding>();
            foreach (var unit in gameState.Units)
            {
                if (unit is ZergBuilding zergBuilding && (zergBuilding.ZergBuildingType == ZergBuildingType.Hatchery ||
                            zergBuilding.ZergBuildingType == ZergBuildingType.Lair ||
                            zergBuilding.ZergBuildingType == ZergBuildingType.Hive))
                {
                    hatcheries.Add(zergBuilding);
                }
                
                else if (unit is ZergUnit zergUnit && zergUnit.ZergUnitType == ZergUnitType.Queen)
                {
                    queens.Add(zergUnit);   
                }
            }
            var queensByHatchery = this.ClosestQueenByHatchery(hatcheries, queens);
            var commands = new List<Command>();
            foreach (var hatcheryQueenPair in queensByHatchery)
            {
                hatcheryQueenPair.Value.SpawnLarva(hatcheryQueenPair.Key, commands);
            }
            return commands;
        }

        public void Register(IGameClient client)
        {
        }

        private Dictionary<ZergBuilding, ZergUnit> ClosestQueenByHatchery(List<ZergBuilding> hatcheries, List<ZergUnit> queens)
        {
            var results = new Dictionary<ZergBuilding, ZergUnit>();
            if (!queens.Any())
            {
                return results;
            }
            foreach (var item in hatcheries)
            {
                var closestQueen = (ZergUnit)item.GetClosest(queens);
                if (closestQueen.GetDistance(item) > withinRadius)
                {
                    continue;
                }
                queens.Remove(closestQueen);
                results.Add(item, closestQueen);
                if (!queens.Any())
                {
                    return results;
                }
            }
            return results;
        }
    }
}
