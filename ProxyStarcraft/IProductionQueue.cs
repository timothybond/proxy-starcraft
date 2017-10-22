namespace ProxyStarcraft
{
    public interface IProductionQueue
    {
        bool IsEmpty(GameState gameState);

        BuildingOrUnitType Peek(GameState gameState);

        BuildingOrUnitType Pop(GameState gameState);
    }
}
