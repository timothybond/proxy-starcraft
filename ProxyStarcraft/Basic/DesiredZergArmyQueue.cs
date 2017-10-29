using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft.Basic
{
    public class DesiredZergArmyQueue : DesiredArmyQueue<ZergUnitType>
    {
        protected override Dictionary<ZergUnitType, int> GetUnitsByType(GameState gameState) => gameState.Units.OfType<ZergUnit>().GroupBy(t => t.ZergUnitType).ToDictionary(group => group.Key, g => g.Count());
    }
}
