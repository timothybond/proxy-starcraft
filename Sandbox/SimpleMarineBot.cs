using System.Collections.Generic;
using ProxyStarcraft;
using ProxyStarcraft.Proto;

namespace Sandbox
{
    public class SimpleMarineBot : IBot
    {
        public Race Race => Race.Terran;

        public IReadOnlyList<Command> Act(GameState gameState)
        {
            var commands = new List<Command>();
            var center = GetCenterOfMass(gameState);

            // Simple strategy: fire at most damaged enemy in range, or closest if a tie
            // If there's no target, move toward the center of friendly units.
            foreach (var unit in gameState.Units)
            {
                var target = GetTarget(unit, gameState);

                if (target == null)
                {
                    commands.Add(unit.Move(center.X, center.Y));
                }
                else
                {
                    commands.Add(unit.Attack(target));
                }
            }

            return commands;
        }

        private static Point GetCenterOfMass(GameState gameState)
        {
            var x = 0f;
            var y = 0f;
            var count = 0;

            foreach (var unit in gameState.RawUnits)
            {
                if (unit.Alliance == Alliance.Self)
                {
                    x += unit.Pos.X;
                    y += unit.Pos.Y;
                    count += 1;
                }
            }

            return new Point { X = x / count, Y = y / count };
        }
        
        private static ProxyStarcraft.Unit GetTarget(ProxyStarcraft.Unit unit, GameState gameState)
        {
            // Only valid for units with exactly one weapon
            var range = gameState.UnitTypes[unit.Raw.UnitType].Weapons[0].Range;

            ProxyStarcraft.Unit target = null;

            foreach (var enemyUnit in gameState.EnemyUnits)
            {
                var distance = unit.GetDistance(enemyUnit);
                if (distance < range)
                {
                    if (target == null ||
                        enemyUnit.Raw.Health < target.Raw.Health ||
                        (enemyUnit.Raw.Health == target.Raw.Health && distance < unit.GetDistance(target)))
                    {
                        target = enemyUnit;
                    }
                }
            }

            return target;
        }
    }
}
