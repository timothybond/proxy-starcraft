namespace ProxyStarcraft
{
    public interface IProductionStrategy
    {
        /// <summary>
        /// Gets the location at which to place a particular building.
        /// </summary>
        IBuildLocation GetPlacement(BuildingType building, GameState gameState);

        /// <summary>
        /// Gets the command to create a particular unit. Assumes both that the cost IS definitely met and that the production WILL occur.
        /// </summary>
        Command Produce(BuildingOrUnitType buildingOrUnit, GameState gameState);
    }
}
