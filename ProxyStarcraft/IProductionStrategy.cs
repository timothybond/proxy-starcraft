namespace ProxyStarcraft
{
    public interface IProductionStrategy
    {
        IBuildLocation GetPlacement(BuildingType building, GameState gameState);

        Command Produce(BuildingOrUnitType buildingOrUnit, GameState gameState);
    }
}
