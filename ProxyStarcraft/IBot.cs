using System.Collections.Generic;

namespace ProxyStarcraft
{
    public interface IBot
    {
        Proto.Race Race { get; }

        IReadOnlyList<Command> Act(GameState gameState);

        /// <summary>
        /// Clumsy method to tie MapAnalyzers from bots into the client. Must be called on the client before starting the game.
        /// </summary>
        /// <param name="client">Client used for this bot.</param>
        void Register(IGameClient client);
    }
}
