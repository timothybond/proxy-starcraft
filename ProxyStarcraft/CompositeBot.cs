using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft
{
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
