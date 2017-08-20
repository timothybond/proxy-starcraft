using System.Collections.Generic;

namespace ProxyStarcraft
{
    public interface IBot
    {
        IReadOnlyList<ICommand> Act(GameState gameState);
    }
}
