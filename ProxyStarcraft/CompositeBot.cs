using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft
{
    /// <summary>
    /// Convenience class that allows you to combine multiple bots together,
    /// in case you want to program simple bots for different functions, e.g.,
    /// resource gathering, unit production, and combat.
    /// </summary>
    public class CompositeBot : IBot
    {
        private IReadOnlyList<IBot> bots;

        public CompositeBot(IEnumerable<IBot> bots)
        {
            this.bots = bots.ToList();
        }

        public IReadOnlyList<ICommand> Act(GameState gameState)
        {
            return bots.SelectMany(bot => bot.Act(gameState)).ToList();
        }
    }
}
