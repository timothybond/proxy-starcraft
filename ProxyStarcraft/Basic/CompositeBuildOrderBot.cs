using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Commands;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class CompositeBuildOrderBot : CompositeBot
    {
        private BuildOrderBot buildOrderBot;
        private BasicEconomyBot economyBot;

        private CompositeBuildOrderBot(BuildOrderBot buildOrderBot, BasicEconomyBot economyBot)
            : base(new IBot[] { buildOrderBot, economyBot })
        {
            this.buildOrderBot = buildOrderBot;
            this.economyBot = economyBot;
        }

        public CompositeBuildOrderBot(Race race) : this(race, new BasicProductionStrategy())
        {
        }

        public CompositeBuildOrderBot(Race race, IBot nextBot) : this(race, nextBot, new BasicProductionStrategy())
        {
        }

        public CompositeBuildOrderBot(Race race, IProductionStrategy productionStrategy) : this(new BuildOrderBot(productionStrategy, race), new BasicEconomyBot(race, productionStrategy))
        {
        }

        public CompositeBuildOrderBot(Race race, IBot nextBot, IProductionStrategy productionStrategy) : this(new BuildOrderBot(productionStrategy, nextBot, race), new BasicEconomyBot(race, productionStrategy))
        {
        }

        public override IReadOnlyList<Command> Act(GameState gameState)
        {
            if (!this.economyBot.AutoBuildWorkers)
            {
                if (this.buildOrderBot.IsFinished)
                {
                    this.economyBot.AutoBuildWorkers = true;
                    this.economyBot.AutoBuildSupply = true;
                }
            }

            var commands = base.Act(gameState);

            var workerBuildCommands = commands.Where(c => c is BuildCommand && c.Unit.Type == TerranUnitType.SCV);

            foreach (var workerBuildCommand in workerBuildCommands)
            {
                this.economyBot.RemoveWorkerFromHarvestAssignments(workerBuildCommand.Unit);
            }

            return commands;
        }

        public void Add(BuildingOrUnitType target)
        {
            buildOrderBot.Add(target);
        }
    }
}
