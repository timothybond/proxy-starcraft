using System.Collections.Generic;

namespace ProxyStarcraft
{
    public interface IBot
    {
        IReadOnlyList<IOrder> Act(GameState gameState);
    }
}
