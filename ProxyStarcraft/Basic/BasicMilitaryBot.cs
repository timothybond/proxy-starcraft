using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft.Basic
{
    public class BasicMilitaryBot : IBot
    {
        private readonly IReadOnlyList<int> waveSizes;

        private int currentWave = 0;

        public BasicMilitaryBot(int waveSize)
        {
            this.waveSizes = new List<int> { waveSize };
        }

        public BasicMilitaryBot(IEnumerable<int> waveSizes)
        {
            this.waveSizes = waveSizes.ToList();

            if (this.waveSizes.Count == 0)
            {
                throw new ArgumentException("Must specify at least one wave size.");
            }
        }

        public Race Race => Race.NoRace;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            var soldiers = new List<Unit>();
            Building mainBase = null;

            // Basically just grab all of the combat-capable units and attack in waves.
            foreach (var unit in gameState.Units)
            {
                if (unit is TerranBuilding terranBuilding)
                {
                    if (terranBuilding.TerranBuildingType == TerranBuildingType.CommandCenter ||
                        terranBuilding.TerranBuildingType == TerranBuildingType.OrbitalCommand ||
                        terranBuilding.TerranBuildingType == TerranBuildingType.PlanetaryFortress)
                    {
                        mainBase = terranBuilding;
                    }
                }
                else if (unit is ProtossBuilding protossBuilding && protossBuilding.ProtossBuildingType == ProtossBuildingType.Nexus)
                {
                    mainBase = protossBuilding;
                }
                else if (unit is ZergBuilding zergBuilding)
                {
                    if (zergBuilding.ZergBuildingType == ZergBuildingType.Hatchery ||
                        zergBuilding.ZergBuildingType == ZergBuildingType.Lair ||
                        zergBuilding.ZergBuildingType == ZergBuildingType.Hive)
                    {
                        mainBase = zergBuilding;
                    }
                }
                else if (unit is TerranUnit terranUnit)
                {
                    if (terranUnit.TerranUnitType != TerranUnitType.SCV &&
                        terranUnit.TerranUnitType != TerranUnitType.MULE &&
                        terranUnit.TerranUnitType != TerranUnitType.WidowMine &&
                        terranUnit.TerranUnitType != TerranUnitType.PointDefenseDrone &&
                        terranUnit.TerranUnitType != TerranUnitType.AutoTurret)
                    {
                        soldiers.Add(terranUnit);
                    }
                }
                else if (unit is ProtossUnit protossUnit)
                {
                    if (protossUnit.ProtossUnitType != ProtossUnitType.Probe &&
                        protossUnit.ProtossUnitType != ProtossUnitType.Observer &&
                        protossUnit.ProtossUnitType != ProtossUnitType.WarpPrism)
                    {
                        soldiers.Add(protossUnit);
                    }
                }
                else if (unit is ZergUnit zergUnit)
                {
                    if (zergUnit.ZergUnitType != ZergUnitType.Larva &&
                        zergUnit.ZergUnitType != ZergUnitType.Cocoon &&
                        zergUnit.ZergUnitType != ZergUnitType.Drone &&
                        zergUnit.ZergUnitType != ZergUnitType.Queen &&
                        zergUnit.ZergUnitType != ZergUnitType.Overlord &&
                        zergUnit.ZergUnitType != ZergUnitType.Overseer &&
                        zergUnit.ZergUnitType != ZergUnitType.Changeling &&
                        zergUnit.ZergUnitType != ZergUnitType.NydusWorm)
                    {
                        soldiers.Add(zergUnit);
                    }
                }
            }

            if (mainBase == null)
            {
                return new List<Command>();
            }
            
            return Attack(gameState, mainBase, soldiers);
        }

        private List<Command> Attack(GameState gameState, Building mainBase, List<Unit> soldiers)
        {
            // Initially, await X idle soldiers and send them toward the enemy's starting location.
            // Once they're there and have no further orders, send them to attack any sighted enemy unit/structure.
            // Once we run out of those, send them to scout every resource deposit until we find more.
            var commands = new List<Command>();

            var waveSize = this.waveSizes[this.currentWave];
            var enemyStartLocation = gameState.MapData.Raw.StartLocations.OrderByDescending(point => mainBase.GetDistance(point)).First();

            var idleSoldiers = soldiers.Where(s => s.Raw.Orders.Count == 0).ToList();

            if (!soldiers.Any(s => s.GetDistance(enemyStartLocation) < 5f) ||
                gameState.EnemyUnits.Any(e => e.GetDistance(enemyStartLocation) < 10f))
            {
                if (idleSoldiers.Count >= waveSize)
                {
                    foreach (var soldier in idleSoldiers)
                    {
                        commands.Add(soldier.AttackMove(enemyStartLocation.X, enemyStartLocation.Y));
                    }

                    if (this.currentWave < this.waveSizes.Count - 1)
                    {
                        this.currentWave += 1;
                    }
                }

                return commands;
            }

            if (gameState.EnemyUnits.Count > 0)
            {
                foreach (var soldier in idleSoldiers)
                {
                    commands.Add(soldier.AttackMove(gameState.EnemyUnits[0].X, gameState.EnemyUnits[0].Y));
                }

                return commands;
            }

            var unscoutedLocations = gameState.MapData.Deposits.Select(d => d.Center).ToList();

            foreach (var location in unscoutedLocations)
            {
                if (soldiers.Any(s => s.GetDistance(location) < 5f ||
                                 s.Raw.Orders.Any(o => o.TargetWorldSpacePos.GetDistance(location) < 5f)))
                {
                    continue;
                }

                if (idleSoldiers.Count == 0)
                {
                    break;
                }

                commands.Add(idleSoldiers[0].AttackMove(location));
                idleSoldiers.RemoveAt(0);
            }

            return commands;
        }
    }
}
