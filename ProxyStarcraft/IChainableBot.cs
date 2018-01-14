using System.Collections.Generic;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public abstract class ChainableBot : IBot
    {
        private IBot nextBot;

        protected ChainableBot(IBot nextBot)
        {
            this.nextBot = nextBot;
        }

        public abstract bool IsFinished { get; }

        public abstract Race Race { get; }

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            if (this.IsFinished)
            {
                if (this.nextBot != null)
                {
                    return this.nextBot.Act(gameState);
                }

                return new List<Command>();
            }

            return ActHelper(gameState);
        }

        public void Register(IGameClient client)
        {
            this.RegisterSelf(client);
            this.nextBot.Register(client);
        }

        protected abstract void RegisterSelf(IGameClient client);

        protected abstract IReadOnlyList<Command> ActHelper(GameState gameState);
    }
}
