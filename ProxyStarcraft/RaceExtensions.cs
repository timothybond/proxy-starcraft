using System;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public static class RaceExtensions
    {
        public static UnitType GetWorker(this Race race)
        {
            switch (race)
            {
                case Race.Terran:
                    return TerranUnitType.SCV;
                case Race.Protoss:
                    return ProtossUnitType.Probe;
                case Race.Zerg:
                    return ZergUnitType.Drone;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
