using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft.Basic
{
    public class DesiredTerranArmyQueue : DesiredArmyQueue<TerranUnitType>
    {
        protected override Dictionary<TerranUnitType, int> GetUnitsByType(GameState gameState) => gameState.Units.OfType<TerranUnit>().GroupBy(t => t.TerranUnitType).ToDictionary(group => group.Key, g => g.Count());
    }
}
