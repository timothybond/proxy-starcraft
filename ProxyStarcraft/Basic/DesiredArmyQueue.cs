using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft.Basic
{
    public class DesiredArmyQueue : IProductionQueue
    {
        private Dictionary<TerranUnitType, int> desiredUnits = new Dictionary<TerranUnitType, int>();
        
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

        public bool IsEmpty(GameState gameState)
        {
            var unitsByType = gameState.Units.OfType<TerranUnit>().GroupBy(t => t.TerranUnitType).ToDictionary(group => group.Key, g => g.Count());

            return desiredUnits.All(pair => unitsByType.ContainsKey(pair.Key) && unitsByType[pair.Key] >= pair.Value);
        }

        public BuildingOrUnitType Peek(GameState gameState)
        {
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
                if (cost.IsMet(gameState) || !cost.HasResources(gameState))
                {
                    return nextUnitType;
                }
                else
                {
                    // We couldn't build this unit, but not for lack of resources, so we should expand production.
                    BuildingOrUnitType producerOrPrerequisite;

                    if (!cost.HasPrerequisite(gameState))
                    {
                        producerOrPrerequisite = UnitOrPrerequisite(cost.Prerequisite, gameState);
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

                            producerOrPrerequisite = UnitOrPrerequisite(techLabType, gameState);
                        }
                        else
                        {
                            producerOrPrerequisite = UnitOrPrerequisite(cost.Builder, gameState);
                        }
                    }

                    return producerOrPrerequisite;
                }
            }

            throw new InvalidOperationException();
        }

        public BuildingOrUnitType Pop(GameState gameState)
        {
            return Peek(gameState);
        }
        
        private BuildingOrUnitType UnitOrPrerequisite(BuildingOrUnitType buildingOrUnit, GameState gameState)
        {
            var cost = gameState.Translator.GetCost(buildingOrUnit);

            if (!cost.HasPrerequisite(gameState))
            {
                return UnitOrPrerequisite(cost.Prerequisite, gameState);
            }

            return buildingOrUnit;
        }
    }
}
