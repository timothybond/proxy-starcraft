namespace ProxyStarcraft
{
    public static class ZergBuildingExtensions
    {
        public static bool IsSpawningLarva(this ZergBuilding hatchery)
        {
            return hatchery.HasBuff(BuffType.SpawnLarva);
        }
    }
}
