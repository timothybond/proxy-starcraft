using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents a snapshot of the game from the perspective of a single player.
    /// </summary>
    public class GameState
    {
        public GameState(
            ResponseGameInfo gameInfo,
            ResponseObservation observation,
            MapData mapData,
            Dictionary<uint, UnitTypeData> unitTypes,
            Dictionary<uint, AbilityData> abilities,
            Translator translator)
        {
            this.GameInfo = gameInfo;
            this.Response = observation;
            this.MapData = mapData;
            this.UnitTypes = unitTypes;
            this.Abilities = abilities;
            this.Translator = translator;

            var unitsByAlliance = this.Response.Observation.RawData.Units.GroupBy(u => u.Alliance);
            
            foreach (var grouping in unitsByAlliance)
            {
                if (grouping.Key == Alliance.Self)
                {
                    this.Units = grouping.Select(translator.ConvertUnit).ToList();
                }
                else if (grouping.Key == Alliance.Ally)
                {
                    this.AlliedUnits = grouping.Select(translator.ConvertUnit).ToList();
                }
                else if (grouping.Key == Alliance.Enemy)
                {
                    this.EnemyUnits = grouping.Select(translator.ConvertUnit).ToList();
                }
                else if (grouping.Key == Alliance.Neutral)
                {
                    this.NeutralUnits = grouping.Select(translator.ConvertUnit).ToList();
                }
            }

            this.Units = this.Units ?? new List<Unit>();
            this.AlliedUnits = this.AlliedUnits ?? new List<Unit>();
            this.EnemyUnits = this.EnemyUnits ?? new List<Unit>();
            this.NeutralUnits = this.NeutralUnits ?? new List<Unit>();

            this.AllUnits = this.Units.Concat(this.AlliedUnits).Concat(this.EnemyUnits).Concat(this.NeutralUnits).ToList();
        }

        /// <summary>
        /// Game information that doesn't change over time, such as which parts of the map are passible.
        /// </summary>
        public ResponseGameInfo GameInfo { get; private set; }
        
        /// <summary>
        /// Raw response. You probably don't need this most of the time.
        /// </summary>
        public ResponseObservation Response { get; private set; }

        /// <summary>
        /// Current game state, including all visible units (although those can also be reached via the Units property for convenience).
        /// </summary>
        public Observation Observation
        {
            get
            {
                return this.Response.Observation;
            }
        }

        /// <summary>
        /// Static and dynamic information about the map, including known structures.
        /// </summary>
        public MapData MapData { get; private set; }

        /// <summary>
        /// All of the defined unit types. This includes a lot of things you don't care about.
        /// Most useful information is better reached via the Translator.
        /// </summary>
        public IReadOnlyDictionary<uint, UnitTypeData> UnitTypes { get; private set; }

        /// <summary>
        /// All of the defined abilities (which is everything from "Move" to "Psionic Storm").
        /// This includes a lot of things you don't care about.
        /// Most useful information is better reached via the Translator.
        /// </summary>
        public IReadOnlyDictionary<uint, AbilityData> Abilities { get; private set; }

        /// <summary>
        /// Helper object that handles converting between a lot of the protobufs/unique ids and friendlier enumerations.
        /// </summary>
        public Translator Translator { get; private set; }

        /// <summary>
        /// Units controlled by this player.
        /// </summary>
        public IReadOnlyList<Unit> Units { get; private set; }

        /// <summary>
        /// Units controlled by any player, or none.
        /// </summary>
        public IReadOnlyList<Unit> AllUnits { get; private set; }

        /// <summary>
        /// Uncontrolled units, such as mineral deposits and critters.
        /// </summary>
        public IReadOnlyList<Unit> NeutralUnits { get; set; }

        /// <summary>
        /// Units controlled by hostile players.
        /// </summary>
        public IReadOnlyList<Unit> EnemyUnits { get; private set; }

        /// <summary>
        /// Units controlled by other, friendly players.
        /// </summary>
        public IReadOnlyList<Unit> AlliedUnits { get; private set; }

        public RepeatedField<Proto.Unit> RawUnits
        {
            get { return this.Observation.RawData.Units; }
        }
    }
}
