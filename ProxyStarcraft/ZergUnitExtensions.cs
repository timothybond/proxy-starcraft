using System.Collections.Generic;

namespace ProxyStarcraft
{
    public static class ZergUnitExtensions
    {
        public static void SpawnLarva(this ZergUnit queen, ZergBuilding hatchery, List<Command> commands)
        {
            if (queen.Raw.Energy >= 25 && !hatchery.IsSpawningLarva() && hatchery.IsBuilt)
            {
                commands.Add(new UseUnitTargetSpecialAbilityCommand(queen, hatchery, SpecialAbilityType.SpawnLarva));
            }
        }
    }
}
