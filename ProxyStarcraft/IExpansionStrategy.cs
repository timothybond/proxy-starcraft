using ProxyStarcraft.Map;

namespace ProxyStarcraft
{
    public interface IExpansionStrategy
    {
        Deposit GetNextExpansion(GameState gameState);
    }
}
