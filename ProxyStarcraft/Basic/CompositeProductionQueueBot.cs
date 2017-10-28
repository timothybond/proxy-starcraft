using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Commands;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    /// <summary>
    /// A multi-part bot that strictly follows an initial build order, then switches to whatever other production queue is provided.
    /// </summary>
    public class CompositeProductionQueueBot : CompositeBot
    {
        private BuildOrder buildOrder;
        private BasicEconomyBot economyBot;
        private IProductionStrategy productionStrategy;

        private CompositeProductionQueueBot(BuildOrder buildOrder, BasicEconomyBot economyBot)
            : base(new IBot[] { economyBot })
        {
            this.buildOrder = buildOrder;
            this.economyBot = economyBot;
            buildOrder.Next = economyBot;
        }

        public CompositeProductionQueueBot(Race race) : this(race, new BasicProductionStrategy(new ClosestExpansionStrategy()))
        {
        }

        public CompositeProductionQueueBot(Race race, IProductionQueue nextQueue) : this(race, nextQueue, new BasicProductionStrategy(new ClosestExpansionStrategy()))
        {
        }

        public CompositeProductionQueueBot(Race race, IProductionStrategy productionStrategy) : this(new BuildOrder(), new BasicEconomyBot(race, productionStrategy))
        {
        }

        public CompositeProductionQueueBot(Race race, IProductionQueue nextQueue, IProductionStrategy productionStrategy) : this(new BuildOrder(), new BasicEconomyBot(race, productionStrategy))
        {
            this.economyBot.Next = nextQueue;
            this.productionStrategy = productionStrategy;
        }

        public override IReadOnlyList<Command> Act(GameState gameState)
        {
            var commands = new List<Command>(base.Act(gameState));

            if (!this.buildOrder.IsEmpty(gameState))
            {
                var unitToBuild = this.buildOrder.Peek(gameState);
                var cost = gameState.Translator.GetCost(unitToBuild);
                if (cost.IsMet(gameState))
                {
                    commands.Add(this.productionStrategy.Produce(this.buildOrder.Pop(gameState), gameState));
                }
            }

            var workerBuildCommands =
                commands.Where(
                    c => c is BuildCommand &&
                    (c.Unit.Type == TerranUnitType.SCV ||
                     c.Unit.Type == ProtossUnitType.Probe ||
                     c.Unit.Type == ZergUnitType.Drone));

            foreach (var workerBuildCommand in workerBuildCommands)
            {
                this.economyBot.RemoveWorkerFromHarvestAssignments(workerBuildCommand.Unit);
            }

            return commands;
        }

        public void Add(BuildingOrUnitType target)
        {
            buildOrder.Push(target);
        }
    }
}
