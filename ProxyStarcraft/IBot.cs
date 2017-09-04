using System.Collections.Generic;

namespace ProxyStarcraft
{
    public interface IBot
    {
        IReadOnlyList<Command> Act(GameState gameState);
    }
}
