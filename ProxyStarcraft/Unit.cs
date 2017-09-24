namespace ProxyStarcraft
{
    public abstract class Unit
    {
        protected readonly Translator translator;

        public Unit(Proto.Unit unit, Translator translator)
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

        public ulong Tag => this.Raw.Tag;

        public float X => this.Raw.Pos.X;

        public float Y => this.Raw.Pos.Y;

        public bool IsFinishedBuilding => this.Raw.BuildProgress == 1.0f;

        public bool IsBuilding(BuildingOrUnitType buildingOrUnitType)
        {
            return translator.IsBuilding(this.Raw, buildingOrUnitType);
        }

        public MoveCommand Move(float x, float y)
        {
            return new MoveCommand(translator.Move, this, x, y);
        }

        public AttackCommand Attack(Unit target)
        {
            return new AttackCommand(translator.Attack, this, target);
        }

        public AttackMoveCommand AttackMove(float x, float y)
        {
            return new AttackMoveCommand(translator.Attack, this, x, y);
        }

        public HarvestCommand Harvest(Unit target)
        {
            return new HarvestCommand(translator.GetHarvestAbility(this.Raw), this, target);
        }

        public BuildCommand Build(BuildingType buildingType, int x, int y)
        {
            return new BuildCommand(this, buildingType, x, y, translator.GetBuildAction(buildingType));
        }

        public TrainCommand Train(UnitType unitType)
        {
            var ability = translator.GetBuildAction(unitType);
            return new TrainCommand(this, unitType, ability);
        }
    }
}
