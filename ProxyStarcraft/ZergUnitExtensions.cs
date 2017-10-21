using System.Collections.Generic;

namespace ProxyStarcraft
{
    public static class ZergUnitExtensions
    {
        public static void SpawnLarva(this ZergUnit queen, ZergBuilding hatchery, List<Command> commands)
        {
            if (queen == null || hatchery == null) // TODO: I had a queen die, I guess mid-update, causing a Null Ref.
            {
                return;
            }
            if (queen.Raw.Energy >= 25 && !hatchery.IsSpawningLarva() && hatchery.IsBuilt) // WELP: Spawn Larva puts a "build"-y action on Zerg town halls. The Zerg building is not "building" anything, but shows via Build Progress.
            {
                commands.Add(new UseUnitTargetAbilityCommand(251, queen, hatchery));
            }
        }
    }
}
