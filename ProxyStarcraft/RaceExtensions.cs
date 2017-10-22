using System;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    public static class RaceExtensions
    {
        public static UnitType GetWorkerType(this Race race)
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

        public static BuildingOrUnitType GetSupplyType(this Race race)
        {
            switch (race)
            {
                case Race.Terran:
                    return TerranBuildingType.SupplyDepot;
                case Race.Protoss:
                    return ProtossBuildingType.Pylon;
                case Race.Zerg:
                    return ZergUnitType.Overlord;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
