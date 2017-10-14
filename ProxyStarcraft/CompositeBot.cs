using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

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
            if (bots == null)
            {
                throw new ArgumentNullException("bots");
            }
            
            this.bots = bots.ToList();

            if (this.bots.Count == 0)
            {
                throw new ArgumentException("Must specify at least one bot.");
            }
        }

        public Race Race => this.bots.Select(b => b.Race).First(r => r != Race.NoRace && r != Race.Random);

        public virtual IReadOnlyList<Command> Act(GameState gameState)
        {
            return bots.SelectMany(bot => bot.Act(gameState)).ToList();
        }
    }
}
