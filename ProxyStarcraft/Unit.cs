using ProxyStarcraft.Proto;

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

        public UnitTypeData RawType { get; private set; }

        public abstract BuildingOrUnitType Type { get; }

        /// <summary>
        /// Determines if the unit is a mineral deposit.
        /// </summary>
        public bool IsMineralDeposit =>
            this.Raw.Alliance == Proto.Alliance.Neutral && this.RawType.HasMinerals; // TODO: Figure out if there is a valid case where this fails

        /// <summary>
        /// Determines if the unit is a vespene geyser.
        /// </summary>
        public bool IsVespeneGeyser =>
            this.Raw.Alliance == Proto.Alliance.Neutral && this.RawType.HasVespene; // TODO: Figure out if there is a valid case where this fails

        public bool IsBuildingSomething => translator.IsBuildingSomething(this.Raw);

        public ulong Tag => this.Raw.Tag;

        public float X => this.Raw.Pos.X;

        public float Y => this.Raw.Pos.Y;

        public bool IsBuilt => this.Raw.BuildProgress == 1.0f;

        public bool IsMainBase =>
            this.Type == TerranBuildingType.CommandCenter ||
            this.Type == TerranBuildingType.PlanetaryFortress ||
            this.Type == TerranBuildingType.OrbitalCommand ||
            this.Type == ProtossBuildingType.Nexus ||
            this.Type == ZergBuildingType.Hatchery ||
            this.Type == ZergBuildingType.Lair ||
            this.Type == ZergBuildingType.Hive;

        public bool IsVespeneBuilding =>
            this.Type == TerranBuildingType.Refinery||
            this.Type == ProtossBuildingType.Assimilator ||
            this.Type == ZergBuildingType.Extractor;

        public bool IsWorker =>
            this.Type == TerranUnitType.SCV ||
            this.Type == ProtossUnitType.Probe ||
            this.Type == ZergUnitType.Drone;

        public bool IsBuilding(BuildingOrUnitType buildingOrUnitType)
        {
            return translator.IsBuilding(this.Raw, buildingOrUnitType);
        }

        public MoveCommand Move(float x, float y)
        {
            return new MoveCommand(this, x, y);
        }

        public MoveCommand Move(Location location)
        {
            var point = (Point2D)location;

            return new MoveCommand(this, point.X, point.Y);
        }

        public AttackCommand Attack(Unit target)
        {
            return new AttackCommand(this, target);
        }

        public AttackMoveCommand AttackMove(float x, float y)
        {
            return new AttackMoveCommand(this, x, y);
        }

        public AttackMoveCommand AttackMove(Location location)
        {
            var point = (Point2D)location;

            return new AttackMoveCommand(this, point.X, point.Y);
        }

        public HarvestCommand Harvest(Unit target)
        {
            return new HarvestCommand(this, target);
        }

        public BuildCommand Build(BuildingType buildingType, IBuildLocation location)
        {
            return new BuildCommand(this, buildingType, location);
        }
        
        public TrainCommand Train(UnitType unitType)
        {
            var ability = translator.GetBuildAction(unitType);
            return new TrainCommand(this, unitType);
        }
        
        public override string ToString() => this.Type.ToString();
    }
}
