using System.Collections.Generic;

namespace ProxyStarcraft
{
    public interface IBot
    {
        Proto.Race Race { get; }

        IReadOnlyList<Command> Act(GameState gameState);
    }
}
