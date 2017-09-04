using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public abstract class Building : Unit2
    {
        public Building(Proto.Unit unit, Translator translator) : base(unit, translator)
        {
        }

        public abstract BuildingType BuildingType { get; }

        public Size2DI Size => translator.GetBuildingSize(this.BuildingType);

        public TrainCommand Train(TerranUnitType unitType)
        {
            var ability = translator.GetBuildAction(unitType);
            return new TrainCommand(this.Raw, unitType, ability);
        }

        public TrainCommand Train(ProtossUnitType unitType)
        {
            var ability = translator.GetBuildAction(unitType);
            return new TrainCommand(this.Raw, unitType, ability);
        }

        public TrainCommand Train(ZergUnitType unitType)
        {
            var ability = translator.GetBuildAction(unitType);
            return new TrainCommand(this.Raw, unitType, ability);
        }

        public RallyLocationCommand Rally(float x, float y)
        {
            return new RallyLocationCommand(translator.GetRallyAbility(this.Raw), this.Raw, x, y);
        }

        public RallyTargetCommand Rally(Unit2 unit)
        {
            return new RallyTargetCommand(translator.GetRallyAbility(this.Raw), this.Raw, unit.Raw);
        }

        public RallyWorkersLocationCommand RallyWorkers(float x, float y)
        {
            return new RallyWorkersLocationCommand(translator.GetRallyWorkersAbility(this.Raw), this.Raw, x, y);
        }

        public RallyWorkersTargetCommand RallyWorkers(Unit2 unit)
        {
            return new RallyWorkersTargetCommand(translator.GetRallyWorkersAbility(this.Raw), this.Raw, unit.Raw);
        }
    }
}
