using System.Linq;
using ProxyStarcraft.Maps;

namespace ProxyStarcraft.Basic
{
    /// <summary>
    /// Expansion strategy that returns the closest unoccupied deposit to one of the currently-owned deposits. Relies on BasicMapAnalyzer.
    /// </summary>
    public class ClosestExpansionStrategy : IExpansionStrategy
    {
        public Deposit GetNextExpansion(GameState gameState)
        {
            var availableDeposits =
                gameState.GetMapData<BasicMapData>().Deposits
                    .Where(d => !gameState.AllUnits.Any(u => u.IsMainBase && u.GetDistance(d.Center) < 10f))
                    .ToList();

            var ownedBaseLocations = gameState.Units.Where(u => u.IsMainBase).Select(u => new Location { X = (int)u.X, Y = (int)u.Y }).ToList();
            
            if (ownedBaseLocations.Count == 0)
            {
                return null;
            }

            return availableDeposits.OrderBy(d => d.Center.GetClosest(ownedBaseLocations).DistanceTo(d.Center)).FirstOrDefault();
        }
    }
}
