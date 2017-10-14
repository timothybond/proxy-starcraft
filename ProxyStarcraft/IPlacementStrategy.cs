namespace ProxyStarcraft
{
    public interface IPlacementStrategy
    {
        IBuildLocation GetPlacement(BuildingType building, GameState gameState);
    }
}
