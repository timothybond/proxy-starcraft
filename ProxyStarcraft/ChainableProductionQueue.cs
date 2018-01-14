using System;

namespace ProxyStarcraft
{
    public abstract class ChainableProductionQueue : IProductionQueue
    {
        public IProductionQueue Next { get; set; }

        protected abstract bool IsSelfEmpty(GameState gameState);

        public bool IsEmpty(GameState gameState)
        {
            if (!this.IsSelfEmpty(gameState))
            {
                return false;
            }

            if (this.Next == null)
            {
                return true;
            }

            return this.Next.IsEmpty(gameState);
        }

        public BuildingOrUnitType Peek(GameState gameState)
        {
            if (this.IsEmpty(gameState))
            {
                throw new InvalidOperationException();
            }

            if (this.IsSelfEmpty(gameState))
            {
                return this.Next.Peek(gameState);
            }

            return PeekSelf(gameState);
        }

        public BuildingOrUnitType Pop(GameState gameState)
        {
            if (this.IsEmpty(gameState))
            {
                throw new InvalidOperationException();
            }

            if (this.IsSelfEmpty(gameState))
            {
                return this.Next.Pop(gameState);
            }

            return PopSelf(gameState);
        }
        
        protected abstract BuildingOrUnitType PeekSelf(GameState gameState);

        protected abstract BuildingOrUnitType PopSelf(GameState gameState);
    }
}
