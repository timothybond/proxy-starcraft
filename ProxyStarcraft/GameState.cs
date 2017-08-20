using System.Collections.Generic;

using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public class GameState
    {
        public GameState(ResponseGameInfo gameInfo, Observation observation, Dictionary<uint, UnitTypeData> unitTypes)
        {
            this.GameInfo = gameInfo;
            this.Observation = observation;
            this.UnitTypes = unitTypes;
        }

        public ResponseGameInfo GameInfo { get; private set; }

        public Observation Observation { get; private set; }

        public IReadOnlyDictionary<uint, UnitTypeData> UnitTypes { get; private set; }
    }
}
