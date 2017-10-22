using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class DesiredArmyBot : IBot
    {
        private Dictionary<TerranUnitType, int> desiredUnits = new Dictionary<TerranUnitType, int>();

        public DesiredArmyBot(IProductionStrategy productionStrategy)
        {
            this.ProductionStrategy = productionStrategy;
        }

        public Race Race => Race.Terran;

        public IProductionStrategy ProductionStrategy { get; private set; }

        public void Set(TerranUnitType unitType, int desired)
        {
            if (this.desiredUnits.ContainsKey(unitType))
            {
                this.desiredUnits[unitType] = desired;
            }
            else
            {
                this.desiredUnits.Add(unitType, desired);
            }
        }

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            // Whenever possible, build a new unit of an unsatisfied type, starting with the type furthest from its goal (as a ratio).
            // TODO: Count units being trained currently.
            var unitsByType = gameState.Units.OfType<TerranUnit>().GroupBy(t => t.TerranUnitType).ToDictionary(group => group.Key, g => g.Count());

            var nextUnitPriorities = this.desiredUnits.OrderBy(
                pair =>
                {
                    var current = unitsByType.ContainsKey(pair.Key) ? unitsByType[pair.Key] : 0;
                    var desired = pair.Value;
                    return (double)current / desired;
                }).Select(pair => pair.Key).ToList();

            if (nextUnitPriorities.Count > 0)
            {
                var nextUnitType = nextUnitPriorities[0];

                var cost = gameState.Translator.GetCost(nextUnitType);
                if (cost.IsMet(gameState))
                {
                    return new List<Command> { ProductionStrategy.Produce(nextUnitType, gameState) };
                }
                else if (cost.HasResources(gameState))
                {
                    // We couldn't build this unit, but not for lack of resources, so we should expand production.
                    Command buildProducerOrPrerequisite;

                    if (!cost.HasPrerequisite(gameState))
                    {
                        buildProducerOrPrerequisite = BuildOrBuildPrerequisite(cost.Prerequisite, gameState);
                        
                    }
                    else 
                    {
                        // At the moment the only checks are resources, prerequisite, and builder, so the builder must be missing.
                        // There is a special case where a Tech Lab prerequisite is used to enforce 'builder must have tech lab',
                        // so we might be in the position of either building an add-on or building a new instance of the building.
                        if (cost.Prerequisite == TerranBuildingType.TechLab ||
                            cost.Prerequisite == TerranBuildingType.BarracksTechLab ||
                            cost.Prerequisite == TerranBuildingType.FactoryTechLab ||
                            cost.Prerequisite == TerranBuildingType.StarportTechLab)
                        {
                            var techLabType = cost.Prerequisite;
                            
                            if (techLabType == TerranBuildingType.TechLab)
                            {
                                if (cost.Builder == TerranBuildingType.Barracks)
                                {
                                    techLabType = TerranBuildingType.BarracksTechLab;
                                }
                                else if (cost.Builder == TerranBuildingType.Factory)
                                {
                                    techLabType = TerranBuildingType.FactoryTechLab;
                                }
                                else if (cost.Builder == TerranBuildingType.Starport)
                                {
                                    techLabType = TerranBuildingType.StarportTechLab;
                                }
                                else
                                {
                                    throw new NotImplementedException();
                                }
                            }

                            buildProducerOrPrerequisite = BuildOrBuildPrerequisite(techLabType, gameState);
                        }
                        else
                        {
                            buildProducerOrPrerequisite = BuildOrBuildPrerequisite(cost.Builder, gameState);
                        }
                    }

                    return buildProducerOrPrerequisite != null ? new List<Command> { buildProducerOrPrerequisite } : new List<Command>();
                }
            }
            
            return new List<Command>();
        }

        private Command BuildOrBuildPrerequisite(BuildingOrUnitType buildingOrUnit, GameState gameState)
        {
            var cost = gameState.Translator.GetCost(buildingOrUnit);

            if (!cost.HasPrerequisite(gameState))
            {
                return BuildOrBuildPrerequisite(cost.Prerequisite, gameState);
            }

            if (cost.IsMet(gameState))
            {
                return this.ProductionStrategy.Produce(buildingOrUnit, gameState);
            }

            return null;
        }
    }
}
