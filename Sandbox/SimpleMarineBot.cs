using System;
using System.Collections.Generic;

using ProxyStarcraft;
using ProxyStarcraft.Proto;

namespace Sandbox
{
    public class SimpleMarineBot : IBot
    {
        public IReadOnlyList<IOrder> Act(GameState gameState)
        {
            var orders = new List<IOrder>();
            var center = GetCenterOfMass(gameState);

            // Simple strategy: fire at most damaged enemy in range, or closest if a tie
            // If there's no target, move toward the center of friendly units.
            foreach (var unit in gameState.Observation.RawData.Units)
            {
                if (unit.Alliance != Alliance.Self)
                {
                    continue;
                }

                var target = GetTarget(unit, gameState);

                if (target == null)
                {
                    orders.Add(new MoveOrder(unit, center.X, center.Y));
                }
                else
                {
                    orders.Add(new AttackOrder(unit, target));
                }
            }

            return orders;
        }

        private static Point GetCenterOfMass(GameState gameState)
        {
            var x = 0f;
            var y = 0f;
            var count = 0;

            foreach (var unit in gameState.Observation.RawData.Units)
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

        private static float DistanceBetween(Unit unit, Unit otherUnit)
        {
            var x = unit.Pos.X - otherUnit.Pos.X;
            var y = unit.Pos.Y - otherUnit.Pos.Y;

            var centerToCenter = Math.Sqrt(x * x + y * y);

            return (float)(centerToCenter - unit.Radius - otherUnit.Radius);
        }

        private static Unit GetTarget(Unit unit, GameState gameState)
        {
            // Only valid for units with exactly one weapon
            var range = gameState.UnitTypes[unit.UnitType].Weapons[0].Range;

            Unit target = null;

            foreach (var otherUnit in gameState.Observation.RawData.Units)
            {
                if (otherUnit.Alliance == Alliance.Enemy)
                {
                    var distance = DistanceBetween(unit, otherUnit);
                    if (distance < range)
                    {
                        if (target == null ||
                            otherUnit.Health < target.Health ||
                            (otherUnit.Health == target.Health && distance < DistanceBetween(unit, target)))
                        {
                            target = otherUnit;
                        }
                    }
                }
            }

            return target;
        }
    }
}
