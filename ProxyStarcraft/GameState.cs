using System.Collections.Generic;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class GameState
    {
        public GameState(ResponseGameInfo gameInfo, Observation observation, Dictionary<uint, UnitTypeData> unitTypes, Dictionary<uint, AbilityData> abilities)
        {
            this.GameInfo = gameInfo;
            this.Observation = observation;
            this.UnitTypes = unitTypes;
            this.Abilities = abilities;
        }

        public ResponseGameInfo GameInfo { get; private set; }

        public Observation Observation { get; private set; }

        public IReadOnlyDictionary<uint, UnitTypeData> UnitTypes { get; private set; }

        public IReadOnlyDictionary<uint, AbilityData> Abilities { get; private set; }
    }
}
