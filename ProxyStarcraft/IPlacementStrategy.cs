namespace ProxyStarcraft
{
    public interface IPlacementStrategy
    {
        Location GetPlacement(BuildingType building, GameState gameState);
    }
}
