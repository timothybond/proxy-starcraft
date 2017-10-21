using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public abstract class Building : Unit
    {
        public Building(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
        }

        public abstract BuildingType BuildingType { get; }

        public Size2DI Size => translator.GetBuildingSize(this.BuildingType);
        
        public RallyLocationCommand Rally(float x, float y)
        {
            return new RallyLocationCommand(this, x, y);
        }

        public RallyTargetCommand Rally(Unit unit)
        {
            return new RallyTargetCommand(this, unit);
        }

        public RallyWorkersLocationCommand RallyWorkers(float x, float y)
        {
            return new RallyWorkersLocationCommand(this, x, y);
        }

        public RallyWorkersTargetCommand RallyWorkers(Unit unit)
        {
            return new RallyWorkersTargetCommand(this, unit);
        }
    }
}
