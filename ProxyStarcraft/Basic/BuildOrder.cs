using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class BuildOrder : ChainableProductionQueue
    {
        private Queue<BuildingOrUnitType> buildOrder = new Queue<BuildingOrUnitType>();
        
        public IProductionStrategy ProductionStrategy { get; private set; }

        protected override bool IsSelfEmpty(GameState gameState)
        {
            return buildOrder.Count == 0;
        }

        public void Push(BuildingOrUnitType target)
        {
            buildOrder.Enqueue(target);
        }

        protected override BuildingOrUnitType PeekSelf(GameState gameState)
        {
            return buildOrder.Peek();
        }

        protected override BuildingOrUnitType PopSelf(GameState gameState)
        {
            return buildOrder.Dequeue();
        }
    }
}
