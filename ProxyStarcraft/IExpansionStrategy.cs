using ProxyStarcraft.Maps;

namespace ProxyStarcraft
{
    /// <summary>
    /// Strategic component that determines where to place a new base when expanding. Generally relies on BasicMapAnalyzer to be registered.
    /// </summary>
    public interface IExpansionStrategy
    {
        Deposit GetNextExpansion(GameState gameState);
    }
}
