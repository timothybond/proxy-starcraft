namespace ProxyStarcraft
{
    public abstract class Unit2
    {
        protected readonly Translator translator;

        public Unit2(Proto.Unit unit, Translator translator)
        {
            this.translator = translator;
            this.Raw = unit;
            this.RawType = translator.UnitTypes[unit.UnitType];
        }

        public Proto.Unit Raw { get; private set; }

        public Proto.UnitTypeData RawType { get; private set; }

        public abstract BuildingOrUnitType Type { get; }

        /// <summary>
        /// Determines if the unit is a mineral deposit.
        /// </summary>
        public bool IsMineralDeposit =>
            this.Raw.Alliance == Proto.Alliance.Neutral && this.Raw.MineralContents > 0; // TODO: Figure out if there is a valid case where this fails

        public bool IsBuildingSomething => translator.IsBuildingSomething(this.Raw);

        public bool IsBuilding(BuildingOrUnitType buildingOrUnitType)
        {
            return translator.IsBuilding(this.Raw, buildingOrUnitType);
        }

        public MoveCommand Move(float x, float y)
        {
            return new MoveCommand(translator.Move, this.Raw, x, y);
        }

        public AttackCommand Attack(Unit2 target)
        {
            return new AttackCommand(translator.Attack, this.Raw, target.Raw);
        }

        public AttackMoveCommand AttackMove(float x, float y)
        {
            return new AttackMoveCommand(translator.Attack, this.Raw, x, y);
        }

        public HarvestCommand Harvest(Unit2 target)
        {
            return new HarvestCommand(translator.GetHarvestAbility(this.Raw), this.Raw, target.Raw);
        }

        protected BuildCommand Build(BuildingType buildingType, int x, int y)
        {
            return new BuildCommand(this.Raw, buildingType, x, y, translator.GetBuildAction(buildingType));
        }
    }
}
