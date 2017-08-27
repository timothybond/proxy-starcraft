using System.Collections.Generic;

using ProxyStarcraft.Proto;
using Google.Protobuf.Collections;

namespace ProxyStarcraft
{
    /// <summary>
    /// Represents a snapshot of the game, including all information your bot should need to act.
    /// </summary>
    public class GameState
    {
        public GameState(
            ResponseGameInfo gameInfo,
            Observation observation,
            MapData mapData,
            Dictionary<uint, UnitTypeData> unitTypes,
            Dictionary<uint, AbilityData> abilities,
            Translator translator)
        {
            this.GameInfo = gameInfo;
            this.Observation = observation;
            this.MapData = mapData;
            this.UnitTypes = unitTypes;
            this.Abilities = abilities;
            this.Translator = translator;
        }

        /// <summary>
        /// Game information that doesn't change over time, such as which parts of the map are passible.
        /// </summary>
        public ResponseGameInfo GameInfo { get; private set; }

        /// <summary>
        /// Current game state, including all visible units (although those can also be reached via the Units property for convenience).
        /// </summary>
        public Observation Observation { get; private set; }

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

        public RepeatedField<Unit> Units
        {
            get { return this.Observation.RawData.Units; }
        }
    }
}
