using ProxyStarcraft.Proto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProxyStarcraft
{
    /// <summary>
    /// Convenience class for mapping between various IDs from the service
    /// and friendlier ways of referring to units/structures/abilities.
    /// 
    /// Note that as far as I can tell, the API is not guaranteed to
    /// continue using the exact same identifiers for everything.
    /// 
    /// This needs rework... it mostly serves its purpose but it tends
    /// to just be the extra thing you need to pass around everywhere.
    /// </summary>
    public class Translator
    {
        private Dictionary<uint, AbilityData> abilities;
        private Dictionary<uint, UnitTypeData> unitTypes;

        private Dictionary<TerranUnitType, uint> createTerranUnitActions;
        private Dictionary<ProtossUnitType, uint> createProtossUnitActions;
        private Dictionary<ZergUnitType, uint> createZergUnitActions;

        private Dictionary<TerranBuildingType, uint> buildTerranBuildingActions;
        private Dictionary<ProtossBuildingType, uint> buildProtossBuildingActions;
        private Dictionary<ZergBuildingType, uint> buildZergBuildingActions;

        private Dictionary<uint, TerranUnitType> terranUnitTypesById;
        private Dictionary<uint, ProtossUnitType> protossUnitTypesById;
        private Dictionary<uint, ZergUnitType> zergUnitTypesById;
        
        private Dictionary<uint, TerranBuildingType> terranBuildingTypesById;
        private Dictionary<uint, ProtossBuildingType> protossBuildingTypesById;
        private Dictionary<uint, ZergBuildingType> zergBuildingTypesById;

        private uint scvHarvest;
        private uint muleHarvest;
        private uint probeHarvest;
        private uint droneHarvest;

        private uint rally;
        private uint rallyUnitsHatchery;

        private uint rallyWorkersCommandCenter;
        private uint rallyWorkersHatchery;
        private uint rallyWorkersNexus;
        
        private IReadOnlyList<uint> mineralFieldTypes;

        private IReadOnlyList<uint> vespeneGeyserTypes;

        private Dictionary<TerranBuildingType, int> terranBuildingSizes = new Dictionary<TerranBuildingType, int>
        {
            { TerranBuildingType.CommandCenter, 5 },
            { TerranBuildingType.Refinery, 3 },
            { TerranBuildingType.SupplyDepot, 2 },
            { TerranBuildingType.Barracks, 3 },
            { TerranBuildingType.EngineeringBay, 3 },
            { TerranBuildingType.Bunker, 3 },
            { TerranBuildingType.SensorTower, 1 },
            { TerranBuildingType.MissileTurret, 2 },
            { TerranBuildingType.Factory, 3 },
            { TerranBuildingType.GhostAcademy, 3 },
            { TerranBuildingType.Starport, 3 },
            { TerranBuildingType.Armory, 3 },
            { TerranBuildingType.FusionCore, 3 }, // ?
            { TerranBuildingType.TechLab, 2 },
            { TerranBuildingType.Reactor, 2 },
            { TerranBuildingType.PlanetaryFortress, 5 },
            { TerranBuildingType.OrbitalCommand, 5 }
        };

        private Dictionary<ProtossBuildingType, int> protossBuildingSizes = new Dictionary<ProtossBuildingType, int>
        {
            { ProtossBuildingType.Nexus, 5 },
            { ProtossBuildingType.Assimilator, 3 },
            { ProtossBuildingType.Pylon, 2 }, // ?
            { ProtossBuildingType.Gateway, 0 },
            { ProtossBuildingType.Forge, 0 },
            { ProtossBuildingType.CyberneticsCore, 0 },
            { ProtossBuildingType.PhotonCannon, 0 },
            { ProtossBuildingType.RoboticsFacility, 0 },
            { ProtossBuildingType.WarpGate, 0 },
            { ProtossBuildingType.Stargate, 0 },
            { ProtossBuildingType.TwilightCouncil, 0 },
            { ProtossBuildingType.RoboticsBay, 0 },
            { ProtossBuildingType.FleetBeacon, 0 },
            { ProtossBuildingType.TemplarArchive, 0 },
            { ProtossBuildingType.DarkShrine, 0 }
        };

        private Dictionary<ZergBuildingType, int> zergBuildingSizes = new Dictionary<ZergBuildingType, int>
        {
            { ZergBuildingType.Hatchery, 5 },
            { ZergBuildingType.Extractor, 3 },
            { ZergBuildingType.SpawningPool, 0 },
            { ZergBuildingType.EvolutionChamber, 0 },
            { ZergBuildingType.RoachWarren, 0 },
            { ZergBuildingType.BanelingNest, 0 },
            { ZergBuildingType.SpineCrawler, 0 },
            { ZergBuildingType.SporeCrawler, 0 },
            { ZergBuildingType.Lair, 5 },
            { ZergBuildingType.HydraliskDen, 0 },
            { ZergBuildingType.LurkerDen, 0 },
            { ZergBuildingType.InfestationPit, 0 },
            { ZergBuildingType.Spire, 0 },
            { ZergBuildingType.NydusNetwork, 0 },
            { ZergBuildingType.Hive, 5 },
            { ZergBuildingType.GreaterSpire, 0 },
            { ZergBuildingType.UltraliskCavern, 0 },
            { ZergBuildingType.CreepTumor, 1 }
        };

        public Translator(Dictionary<uint, AbilityData> abilities, Dictionary<uint, UnitTypeData> unitTypes)
        {
            this.abilities = abilities;
            this.unitTypes = unitTypes;
            
            // Somewhat-amusing trick: although there are tons of non-used abilities,
            // you can quickly narrow it down to ones that actually appear in-game
            // by only using the ones with hotkeys. Every button on the screen has a hotkey.
            var hotkeyedAbilities = abilities.Values.Where(ability => !string.IsNullOrEmpty(ability.Hotkey)).ToList();

            Move = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Move")).AbilityId;
            Attack = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Attack Attack")).AbilityId;

            rally = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Rally Building")).AbilityId;
            rallyWorkersCommandCenter = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Rally CommandCenter")).AbilityId;
            rallyWorkersNexus = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Rally Nexus")).AbilityId;
            rallyWorkersHatchery = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Rally Hatchery Workers")).AbilityId;
            rallyUnitsHatchery = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Rally Hatchery Units")).AbilityId;

            var buildAndTrainAbilities = hotkeyedAbilities
                .Where(ability => ability.FriendlyName.Contains("Build") || ability.FriendlyName.Contains("Train"));

            scvHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather SCV")).AbilityId;
            muleHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Mule")).AbilityId;
            probeHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Probe")).AbilityId;
            droneHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Drone")).AbilityId;

            mineralFieldTypes = unitTypes.Values.Where(u => u.Name.Contains("MineralField")).Select(u => u.UnitId).ToList();
            vespeneGeyserTypes = unitTypes.Values.Where(u => u.Name.Contains("Vespene")).Select(u => u.UnitId).ToList();

            var abilitiesByName = new Dictionary<string, AbilityData>();
            
            foreach (var ability in buildAndTrainAbilities)
            {
                abilitiesByName.Add(ability.FriendlyName, ability);
            }

            createTerranUnitActions = new Dictionary<TerranUnitType, uint>
            {
                { TerranUnitType.SCV, abilitiesByName["Train SCV"].AbilityId },
                { TerranUnitType.Marine, abilitiesByName["Train Marine"].AbilityId },
                { TerranUnitType.Marauder, abilitiesByName["Train Marauder"].AbilityId },
                { TerranUnitType.Reaper, abilitiesByName["Train Reaper"].AbilityId },
                { TerranUnitType.Ghost, abilitiesByName["Train Ghost"].AbilityId },
                { TerranUnitType.Hellion, abilitiesByName["Train Hellion"].AbilityId },
                { TerranUnitType.Hellbat, abilitiesByName["Train Hellbat"].AbilityId },
                { TerranUnitType.SiegeTank, abilitiesByName["Train SiegeTank"].AbilityId },
                { TerranUnitType.Cyclone, abilitiesByName["Train Cyclone"].AbilityId },
                { TerranUnitType.Thor, abilitiesByName["Train Thor"].AbilityId },
                { TerranUnitType.Viking, abilitiesByName["Train VikingFighter"].AbilityId },
                { TerranUnitType.Medivac, abilitiesByName["Train Medivac"].AbilityId },
                { TerranUnitType.Liberator, abilitiesByName["Train Liberator"].AbilityId },
                { TerranUnitType.Raven, abilitiesByName["Train Raven"].AbilityId },
                { TerranUnitType.Banshee, abilitiesByName["Train Banshee"].AbilityId },
                { TerranUnitType.Battlecruiser, abilitiesByName["Train Battlecruiser"].AbilityId },
                { TerranUnitType.WidowMine, abilitiesByName["Train WidowMine"].AbilityId },
            };

            createProtossUnitActions = new Dictionary<ProtossUnitType, uint>
            {
                { ProtossUnitType.Probe, abilitiesByName["Train Probe"].AbilityId },
                { ProtossUnitType.Zealot, abilitiesByName["Train Zealot"].AbilityId },
                { ProtossUnitType.Stalker, abilitiesByName["Train Stalker"].AbilityId },
                { ProtossUnitType.Sentry, abilitiesByName["Train Sentry"].AbilityId },
                { ProtossUnitType.Adept, abilitiesByName["Train Adept"].AbilityId },
                { ProtossUnitType.HighTemplar, abilitiesByName["Train HighTemplar"].AbilityId },
                { ProtossUnitType.DarkTemplar, abilitiesByName["Train DarkTemplar"].AbilityId },
                { ProtossUnitType.Immortal, abilitiesByName["Train Immortal"].AbilityId },
                { ProtossUnitType.Colossus, abilitiesByName["Train Colossus"].AbilityId },
                { ProtossUnitType.Disruptor, abilitiesByName["Train Disruptor"].AbilityId },
                { ProtossUnitType.Observer, abilitiesByName["Train Observer"].AbilityId },
                { ProtossUnitType.WarpPrism, abilitiesByName["Train WarpPrism"].AbilityId },
                { ProtossUnitType.Phoenix, abilitiesByName["Train Phoenix"].AbilityId },
                { ProtossUnitType.VoidRay, abilitiesByName["Train VoidRay"].AbilityId },
                { ProtossUnitType.Oracle, abilitiesByName["Train Oracle"].AbilityId },
                { ProtossUnitType.Carrier, abilitiesByName["Train Carrier"].AbilityId },
                { ProtossUnitType.Tempest, abilitiesByName["Train Tempest"].AbilityId },
            };

            createZergUnitActions = new Dictionary<ZergUnitType, uint>
            {
                { ZergUnitType.Drone, abilitiesByName["Train Drone"].AbilityId },
                { ZergUnitType.Queen, abilitiesByName["Train Queen"].AbilityId },
                { ZergUnitType.Zergling, abilitiesByName["Train Zergling"].AbilityId },
                { ZergUnitType.Baneling, abilitiesByName["Train Baneling"].AbilityId },
                { ZergUnitType.Roach, abilitiesByName["Train Roach"].AbilityId },
                { ZergUnitType.Hydralisk, abilitiesByName["Train Hydralisk"].AbilityId },
                { ZergUnitType.Infestor, abilitiesByName["Train Infestor"].AbilityId },
                { ZergUnitType.SwarmHost, abilitiesByName["Train SwarmHost"].AbilityId },
                { ZergUnitType.Ultralisk, abilitiesByName["Train Ultralisk"].AbilityId },
                { ZergUnitType.Overlord, abilitiesByName["Train Overlord"].AbilityId },
                { ZergUnitType.Mutalisk, abilitiesByName["Train Mutalisk"].AbilityId },
                { ZergUnitType.Corruptor, abilitiesByName["Train Corruptor"].AbilityId },
                { ZergUnitType.Viper, abilitiesByName["Train Viper"].AbilityId },
            };

            buildTerranBuildingActions = new Dictionary<TerranBuildingType, uint>
            {
                { TerranBuildingType.CommandCenter, abilitiesByName["Build CommandCenter"].AbilityId },
                { TerranBuildingType.Refinery, abilitiesByName["Build Refinery"].AbilityId },
                { TerranBuildingType.SupplyDepot, abilitiesByName["Build SupplyDepot"].AbilityId },
                { TerranBuildingType.Barracks, abilitiesByName["Build Barracks"].AbilityId },
                { TerranBuildingType.EngineeringBay, abilitiesByName["Build EngineeringBay"].AbilityId },
                { TerranBuildingType.Bunker, abilitiesByName["Build Bunker"].AbilityId },
                { TerranBuildingType.SensorTower, abilitiesByName["Build SensorTower"].AbilityId },
                { TerranBuildingType.MissileTurret, abilitiesByName["Build MissileTurret"].AbilityId },
                { TerranBuildingType.Factory, abilitiesByName["Build Factory"].AbilityId },
                { TerranBuildingType.GhostAcademy, abilitiesByName["Build GhostAcademy"].AbilityId },
                { TerranBuildingType.Starport, abilitiesByName["Build Starport"].AbilityId },
                { TerranBuildingType.Armory, abilitiesByName["Build Armory"].AbilityId },
                { TerranBuildingType.FusionCore, abilitiesByName["Build FusionCore"].AbilityId },
            };

            buildProtossBuildingActions = new Dictionary<ProtossBuildingType, uint>
            {
                { ProtossBuildingType.Nexus, abilitiesByName["Build Nexus"].AbilityId },
                { ProtossBuildingType.Assimilator, abilitiesByName["Build Assimilator"].AbilityId },
                { ProtossBuildingType.Pylon, abilitiesByName["Build Pylon"].AbilityId },
                { ProtossBuildingType.Gateway, abilitiesByName["Build Gateway"].AbilityId },
                { ProtossBuildingType.Forge, abilitiesByName["Build Forge"].AbilityId },
                { ProtossBuildingType.CyberneticsCore, abilitiesByName["Build CyberneticsCore"].AbilityId },
                { ProtossBuildingType.PhotonCannon, abilitiesByName["Build PhotonCannon"].AbilityId },
                { ProtossBuildingType.RoboticsFacility, abilitiesByName["Build RoboticsFacility"].AbilityId },
                { ProtossBuildingType.Stargate, abilitiesByName["Build Stargate"].AbilityId },
                { ProtossBuildingType.TwilightCouncil, abilitiesByName["Build TwilightCouncil"].AbilityId },
                { ProtossBuildingType.RoboticsBay, abilitiesByName["Build RoboticsBay"].AbilityId },
                { ProtossBuildingType.FleetBeacon, abilitiesByName["Build FleetBeacon"].AbilityId },
                { ProtossBuildingType.TemplarArchive, abilitiesByName["Build TemplarArchive"].AbilityId },
                { ProtossBuildingType.DarkShrine, abilitiesByName["Build DarkShrine"].AbilityId }
            };

            buildZergBuildingActions = new Dictionary<ZergBuildingType, uint>
            {
                { ZergBuildingType.Hatchery, abilitiesByName["Build Hatchery"].AbilityId },
                { ZergBuildingType.Extractor, abilitiesByName["Build Extractor"].AbilityId },
                { ZergBuildingType.SpawningPool, abilitiesByName["Build SpawningPool"].AbilityId },
                { ZergBuildingType.EvolutionChamber, abilitiesByName["Build EvolutionChamber"].AbilityId },
                { ZergBuildingType.RoachWarren, abilitiesByName["Build RoachWarren"].AbilityId },
                { ZergBuildingType.BanelingNest, abilitiesByName["Build BanelingNest"].AbilityId },
                { ZergBuildingType.SpineCrawler, abilitiesByName["Build SpineCrawler"].AbilityId },
                { ZergBuildingType.SporeCrawler, abilitiesByName["Build SporeCrawler"].AbilityId },
                { ZergBuildingType.HydraliskDen, abilitiesByName["Build HydraliskDen"].AbilityId },
                { ZergBuildingType.InfestationPit, abilitiesByName["Build InfestationPit"].AbilityId },
                { ZergBuildingType.Spire, abilitiesByName["Build Spire"].AbilityId },
                { ZergBuildingType.NydusNetwork, abilitiesByName["Build NydusNetwork"].AbilityId },
                { ZergBuildingType.UltraliskCavern, abilitiesByName["Build UltraliskCavern"].AbilityId }
            };

            var unitTypesByName = unitTypes.Values.Where(unitType => !string.IsNullOrEmpty(unitType.Name)).ToDictionary(unitType => unitType.Name);

            terranUnitTypesById = new Dictionary<uint, TerranUnitType>();

            terranUnitTypesById.Add(unitTypesByName["SCV"].UnitId, TerranUnitType.SCV);
            terranUnitTypesById.Add(unitTypesByName["MULE"].UnitId, TerranUnitType.MULE);
            terranUnitTypesById.Add(unitTypesByName["Marine"].UnitId, TerranUnitType.Marine);
            terranUnitTypesById.Add(unitTypesByName["Marauder"].UnitId, TerranUnitType.Marauder);
            terranUnitTypesById.Add(unitTypesByName["Reaper"].UnitId, TerranUnitType.Reaper);
            terranUnitTypesById.Add(unitTypesByName["Ghost"].UnitId, TerranUnitType.Ghost);
            terranUnitTypesById.Add(unitTypesByName["HellionTank"].UnitId, TerranUnitType.Hellion);
            terranUnitTypesById.Add(unitTypesByName["Hellion"].UnitId, TerranUnitType.Hellion);
            terranUnitTypesById.Add(unitTypesByName["SiegeTank"].UnitId, TerranUnitType.SiegeTank);
            terranUnitTypesById.Add(unitTypesByName["Cyclone"].UnitId, TerranUnitType.Cyclone);
            terranUnitTypesById.Add(unitTypesByName["Thor"].UnitId, TerranUnitType.Thor);
            terranUnitTypesById.Add(unitTypesByName["VikingFighter"].UnitId, TerranUnitType.Viking);
            terranUnitTypesById.Add(unitTypesByName["VikingAssault"].UnitId, TerranUnitType.Viking);
            terranUnitTypesById.Add(unitTypesByName["Medivac"].UnitId, TerranUnitType.Medivac);
            terranUnitTypesById.Add(unitTypesByName["Liberator"].UnitId, TerranUnitType.Liberator);
            terranUnitTypesById.Add(unitTypesByName["Raven"].UnitId, TerranUnitType.Raven);
            terranUnitTypesById.Add(unitTypesByName["Banshee"].UnitId, TerranUnitType.Banshee);
            terranUnitTypesById.Add(unitTypesByName["Battlecruiser"].UnitId, TerranUnitType.Battlecruiser);
            terranUnitTypesById.Add(unitTypesByName["WidowMine"].UnitId, TerranUnitType.WidowMine);
            terranUnitTypesById.Add(unitTypesByName["AutoTurret"].UnitId, TerranUnitType.AutoTurret);
            terranUnitTypesById.Add(unitTypesByName["PointDefenseDrone"].UnitId, TerranUnitType.PointDefenseDrone);

            zergUnitTypesById = new Dictionary<uint, ZergUnitType>();

            zergUnitTypesById.Add(unitTypesByName["Larva"].UnitId, ZergUnitType.Larva);
            zergUnitTypesById.Add(unitTypesByName["Drone"].UnitId, ZergUnitType.Drone);
            zergUnitTypesById.Add(unitTypesByName["DroneBurrowed"].UnitId, ZergUnitType.Drone);
            zergUnitTypesById.Add(unitTypesByName["Queen"].UnitId, ZergUnitType.Queen);
            zergUnitTypesById.Add(unitTypesByName["QueenBurrowed"].UnitId, ZergUnitType.Queen);
            zergUnitTypesById.Add(unitTypesByName["Zergling"].UnitId, ZergUnitType.Zergling);
            zergUnitTypesById.Add(unitTypesByName["ZerglingBurrowed"].UnitId, ZergUnitType.Zergling);
            zergUnitTypesById.Add(unitTypesByName["Baneling"].UnitId, ZergUnitType.Baneling);
            zergUnitTypesById.Add(unitTypesByName["BanelingBurrowed"].UnitId, ZergUnitType.Baneling);
            zergUnitTypesById.Add(unitTypesByName["Roach"].UnitId, ZergUnitType.Roach);
            zergUnitTypesById.Add(unitTypesByName["RoachBurrowed"].UnitId, ZergUnitType.Roach);
            zergUnitTypesById.Add(unitTypesByName["Hydralisk"].UnitId, ZergUnitType.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["HydraliskBurrowed"].UnitId, ZergUnitType.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["Infestor"].UnitId, ZergUnitType.Infestor);
            zergUnitTypesById.Add(unitTypesByName["InfestorBurrowed"].UnitId, ZergUnitType.Infestor);
            zergUnitTypesById.Add(unitTypesByName["InfestedTerran"].UnitId, ZergUnitType.InfestedTerran);
            zergUnitTypesById.Add(unitTypesByName["InfestorTerranBurrowed"].UnitId, ZergUnitType.InfestedTerran);
            zergUnitTypesById.Add(unitTypesByName["SwarmHostMP"].UnitId, ZergUnitType.SwarmHost);
            zergUnitTypesById.Add(unitTypesByName["SwarmHostBurrowedMP"].UnitId, ZergUnitType.SwarmHost);
            zergUnitTypesById.Add(unitTypesByName["LocustMP"].UnitId, ZergUnitType.Locust);
            zergUnitTypesById.Add(unitTypesByName["LocustMPFlying"].UnitId, ZergUnitType.Locust);
            zergUnitTypesById.Add(unitTypesByName["Ultralisk"].UnitId, ZergUnitType.Ultralisk);
            zergUnitTypesById.Add(unitTypesByName["UltraliskBurrowed"].UnitId, ZergUnitType.Ultralisk);
            zergUnitTypesById.Add(unitTypesByName["Broodling"].UnitId, ZergUnitType.Broodling);
            zergUnitTypesById.Add(unitTypesByName["Overlord"].UnitId, ZergUnitType.Overlord);
            zergUnitTypesById.Add(unitTypesByName["Overseer"].UnitId, ZergUnitType.Overseer);
            zergUnitTypesById.Add(unitTypesByName["Changeling"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingMarine"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingMarineShield"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZealot"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZergling"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZerglingWings"].UnitId, ZergUnitType.Changeling);
            zergUnitTypesById.Add(unitTypesByName["Mutalisk"].UnitId, ZergUnitType.Mutalisk);
            zergUnitTypesById.Add(unitTypesByName["Corruptor"].UnitId, ZergUnitType.Corruptor);
            zergUnitTypesById.Add(unitTypesByName["BroodLord"].UnitId, ZergUnitType.BroodLord);
            zergUnitTypesById.Add(unitTypesByName["Viper"].UnitId, ZergUnitType.Viper);
            zergUnitTypesById.Add(unitTypesByName["NydusCanal"].UnitId, ZergUnitType.NydusWorm);

            protossUnitTypesById = new Dictionary<uint, ProtossUnitType>();

            protossUnitTypesById.Add(unitTypesByName["Probe"].UnitId, ProtossUnitType.Probe);
            protossUnitTypesById.Add(unitTypesByName["Zealot"].UnitId, ProtossUnitType.Zealot);
            protossUnitTypesById.Add(unitTypesByName["Stalker"].UnitId, ProtossUnitType.Stalker);
            protossUnitTypesById.Add(unitTypesByName["Sentry"].UnitId, ProtossUnitType.Sentry);
            protossUnitTypesById.Add(unitTypesByName["Adept"].UnitId, ProtossUnitType.Adept);
            protossUnitTypesById.Add(unitTypesByName["HighTemplar"].UnitId, ProtossUnitType.HighTemplar);
            protossUnitTypesById.Add(unitTypesByName["DarkTemplar"].UnitId, ProtossUnitType.DarkTemplar);
            protossUnitTypesById.Add(unitTypesByName["Immortal"].UnitId, ProtossUnitType.Immortal);
            protossUnitTypesById.Add(unitTypesByName["Colossus"].UnitId, ProtossUnitType.Colossus);
            protossUnitTypesById.Add(unitTypesByName["Disruptor"].UnitId, ProtossUnitType.Disruptor);
            protossUnitTypesById.Add(unitTypesByName["Archon"].UnitId, ProtossUnitType.Archon);
            protossUnitTypesById.Add(unitTypesByName["Observer"].UnitId, ProtossUnitType.Observer);
            protossUnitTypesById.Add(unitTypesByName["WarpPrism"].UnitId, ProtossUnitType.WarpPrism);
            protossUnitTypesById.Add(unitTypesByName["Phoenix"].UnitId, ProtossUnitType.Phoenix);
            protossUnitTypesById.Add(unitTypesByName["VoidRay"].UnitId, ProtossUnitType.VoidRay);
            protossUnitTypesById.Add(unitTypesByName["Oracle"].UnitId, ProtossUnitType.Oracle);
            protossUnitTypesById.Add(unitTypesByName["Carrier"].UnitId, ProtossUnitType.Carrier);
            protossUnitTypesById.Add(unitTypesByName["Tempest"].UnitId, ProtossUnitType.Tempest);
            protossUnitTypesById.Add(unitTypesByName["MothershipCore"].UnitId, ProtossUnitType.MothershipCore);
            protossUnitTypesById.Add(unitTypesByName["Mothership"].UnitId, ProtossUnitType.Mothership);

            terranBuildingTypesById = new Dictionary<uint, TerranBuildingType>();

            terranBuildingTypesById.Add(unitTypesByName["CommandCenter"].UnitId, TerranBuildingType.CommandCenter);
            terranBuildingTypesById.Add(unitTypesByName["Refinery"].UnitId, TerranBuildingType.Refinery);
            terranBuildingTypesById.Add(unitTypesByName["SupplyDepot"].UnitId, TerranBuildingType.SupplyDepot);
            terranBuildingTypesById.Add(unitTypesByName["Barracks"].UnitId, TerranBuildingType.Barracks);
            terranBuildingTypesById.Add(unitTypesByName["EngineeringBay"].UnitId, TerranBuildingType.EngineeringBay);
            terranBuildingTypesById.Add(unitTypesByName["Bunker"].UnitId, TerranBuildingType.Bunker);
            terranBuildingTypesById.Add(unitTypesByName["SensorTower"].UnitId, TerranBuildingType.SensorTower);
            terranBuildingTypesById.Add(unitTypesByName["MissileTurret"].UnitId, TerranBuildingType.MissileTurret);
            terranBuildingTypesById.Add(unitTypesByName["Factory"].UnitId, TerranBuildingType.Factory);
            terranBuildingTypesById.Add(unitTypesByName["GhostAcademy"].UnitId, TerranBuildingType.GhostAcademy);
            terranBuildingTypesById.Add(unitTypesByName["Starport"].UnitId, TerranBuildingType.Starport);
            terranBuildingTypesById.Add(unitTypesByName["Armory"].UnitId, TerranBuildingType.Armory);
            terranBuildingTypesById.Add(unitTypesByName["FusionCore"].UnitId, TerranBuildingType.FusionCore);
            terranBuildingTypesById.Add(unitTypesByName["TechLab"].UnitId, TerranBuildingType.TechLab);
            terranBuildingTypesById.Add(unitTypesByName["Reactor"].UnitId, TerranBuildingType.Reactor);
            terranBuildingTypesById.Add(unitTypesByName["PlanetaryFortress"].UnitId, TerranBuildingType.PlanetaryFortress);
            terranBuildingTypesById.Add(unitTypesByName["OrbitalCommand"].UnitId, TerranBuildingType.OrbitalCommand);

            protossBuildingTypesById = new Dictionary<uint, ProtossBuildingType>();

            protossBuildingTypesById.Add(unitTypesByName["Nexus"].UnitId, ProtossBuildingType.Nexus);
            protossBuildingTypesById.Add(unitTypesByName["Assimilator"].UnitId, ProtossBuildingType.Assimilator);
            protossBuildingTypesById.Add(unitTypesByName["Pylon"].UnitId, ProtossBuildingType.Pylon);
            protossBuildingTypesById.Add(unitTypesByName["Gateway"].UnitId, ProtossBuildingType.Gateway);
            protossBuildingTypesById.Add(unitTypesByName["Forge"].UnitId, ProtossBuildingType.Forge);
            protossBuildingTypesById.Add(unitTypesByName["CyberneticsCore"].UnitId, ProtossBuildingType.CyberneticsCore);
            protossBuildingTypesById.Add(unitTypesByName["PhotonCannon"].UnitId, ProtossBuildingType.PhotonCannon);
            protossBuildingTypesById.Add(unitTypesByName["RoboticsFacility"].UnitId, ProtossBuildingType.RoboticsFacility);
            protossBuildingTypesById.Add(unitTypesByName["WarpGate"].UnitId, ProtossBuildingType.WarpGate);
            protossBuildingTypesById.Add(unitTypesByName["Stargate"].UnitId, ProtossBuildingType.Stargate);
            protossBuildingTypesById.Add(unitTypesByName["TwilightCouncil"].UnitId, ProtossBuildingType.TwilightCouncil);
            protossBuildingTypesById.Add(unitTypesByName["RoboticsBay"].UnitId, ProtossBuildingType.RoboticsBay);
            protossBuildingTypesById.Add(unitTypesByName["FleetBeacon"].UnitId, ProtossBuildingType.FleetBeacon);
            protossBuildingTypesById.Add(unitTypesByName["TemplarArchive"].UnitId, ProtossBuildingType.TemplarArchive);
            protossBuildingTypesById.Add(unitTypesByName["DarkShrine"].UnitId, ProtossBuildingType.DarkShrine);

            zergBuildingTypesById = new Dictionary<uint, ZergBuildingType>();

            zergBuildingTypesById.Add(unitTypesByName["Hatchery"].UnitId, ZergBuildingType.Hatchery);
            zergBuildingTypesById.Add(unitTypesByName["Extractor"].UnitId, ZergBuildingType.Extractor);
            zergBuildingTypesById.Add(unitTypesByName["SpawningPool"].UnitId, ZergBuildingType.SpawningPool);
            zergBuildingTypesById.Add(unitTypesByName["EvolutionChamber"].UnitId, ZergBuildingType.EvolutionChamber);
            zergBuildingTypesById.Add(unitTypesByName["RoachWarren"].UnitId, ZergBuildingType.RoachWarren);
            zergBuildingTypesById.Add(unitTypesByName["BanelingNest"].UnitId, ZergBuildingType.BanelingNest);
            zergBuildingTypesById.Add(unitTypesByName["SpineCrawler"].UnitId, ZergBuildingType.SpineCrawler);
            zergBuildingTypesById.Add(unitTypesByName["SporeCrawler"].UnitId, ZergBuildingType.SporeCrawler);
            zergBuildingTypesById.Add(unitTypesByName["Lair"].UnitId, ZergBuildingType.Lair);
            zergBuildingTypesById.Add(unitTypesByName["HydraliskDen"].UnitId, ZergBuildingType.HydraliskDen);
            zergBuildingTypesById.Add(unitTypesByName["LurkerDen"].UnitId, ZergBuildingType.LurkerDen);
            zergBuildingTypesById.Add(unitTypesByName["InfestationPit"].UnitId, ZergBuildingType.InfestationPit);
            zergBuildingTypesById.Add(unitTypesByName["Spire"].UnitId, ZergBuildingType.Spire);
            zergBuildingTypesById.Add(unitTypesByName["NydusNetwork"].UnitId, ZergBuildingType.NydusNetwork);
            zergBuildingTypesById.Add(unitTypesByName["Hive"].UnitId, ZergBuildingType.Hive);
            zergBuildingTypesById.Add(unitTypesByName["GreaterSpire"].UnitId, ZergBuildingType.GreaterSpire);
            zergBuildingTypesById.Add(unitTypesByName["UltraliskCavern"].UnitId, ZergBuildingType.UltraliskCavern);
            zergBuildingTypesById.Add(unitTypesByName["CreepTumor"].UnitId, ZergBuildingType.CreepTumor);
        }

        public uint GetAbilityId(TrainCommand trainCommand)
        {
            if (trainCommand.TerranUnit != TerranUnitType.Unspecified)
            {
                return createTerranUnitActions[trainCommand.TerranUnit];
            }

            if (trainCommand.ProtossUnit != ProtossUnitType.Unspecified)
            {
                return createProtossUnitActions[trainCommand.ProtossUnit];
            }

            if (trainCommand.ZergUnit != ZergUnitType.Unspecified)
            {
                return createZergUnitActions[trainCommand.ZergUnit];
            }

            throw new ArgumentException("Received a training command with no unit specified.");
        }

        public uint GetAbilityId(BuildCommand buildCommand)
        {
            if (buildCommand.Building.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return buildTerranBuildingActions[buildCommand.Building.TerranBuilding];
            }
            else if (buildCommand.Building.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return buildProtossBuildingActions[buildCommand.Building.ProtossBuilding];
            }
            else if (buildCommand.Building.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return buildZergBuildingActions[buildCommand.Building.ZergBuilding];
            }

            throw new ArgumentException("Received a build command with no building specified.");
        }

        public uint GetHarvestAbility(Unit unit)
        {
            if (terranUnitTypesById.ContainsKey(unit.UnitType))
            {
                if (terranUnitTypesById[unit.UnitType] == TerranUnitType.SCV)
                {
                    return scvHarvest;
                }

                if (terranUnitTypesById[unit.UnitType] == TerranUnitType.MULE)
                {
                    return scvHarvest;
                }
            }

            if (zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == ZergUnitType.Drone)
            {
                return droneHarvest;
            }

            if (protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == ProtossUnitType.Probe)
            {
                return probeHarvest;
            }

            throw new ArgumentException("Attempted to get harvest ability for unit other than SCV, MULE, Probe, or Drone.", "unit");
        }

        public bool IsHarvester(Unit unit)
        {
            if (terranUnitTypesById.ContainsKey(unit.UnitType))
            {
                if (terranUnitTypesById[unit.UnitType] == TerranUnitType.SCV)
                {
                    return true;
                }

                if (terranUnitTypesById[unit.UnitType] == TerranUnitType.MULE)
                {
                    return true;
                }
            }

            if (zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == ZergUnitType.Drone)
            {
                return true;
            }

            if (protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == ProtossUnitType.Probe)
            {
                return true;
            }

            return false;
        }

        public bool IsUnitOfType(Unit unit, BuildingOrUnitType unitType)
        {
            if (unitType.TerranUnit != TerranUnitType.Unspecified)
            {
                return terranUnitTypesById.ContainsKey(unit.UnitType) && terranUnitTypesById[unit.UnitType] == unitType.TerranUnit;
            }
            else if (unitType.ProtossUnit != ProtossUnitType.Unspecified)
            {
                return protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == unitType.ProtossUnit;
            }
            else if (unitType.ZergUnit != ZergUnitType.Unspecified)
            {
                return zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == unitType.ZergUnit;
            }
            else if (unitType.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return terranBuildingTypesById.ContainsKey(unit.UnitType) && terranBuildingTypesById[unit.UnitType] == unitType.TerranBuilding;
            }
            else if (unitType.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return protossBuildingTypesById.ContainsKey(unit.UnitType) && protossBuildingTypesById[unit.UnitType] == unitType.ProtossBuilding;
            }
            else if (unitType.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return zergBuildingTypesById.ContainsKey(unit.UnitType) && zergBuildingTypesById[unit.UnitType] == unitType.ZergBuilding;
            }

            throw new NotImplementedException();
        }
        
        public uint Move { get; private set; }

        public uint Attack { get; private set; }

        public uint MineralDeposit { get; private set; }

        public uint VespeneGeyser { get; private set; }

        public Size2DI GetBuildingSize(BuildCommand buildCommand)
        {
            if (buildCommand.Building.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return GetBuildingSize(buildCommand.Building.TerranBuilding);
            }

            if (buildCommand.Building.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return GetBuildingSize(buildCommand.Building.ProtossBuilding);
            }

            if (buildCommand.Building.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return GetBuildingSize(buildCommand.Building.ZergBuilding);
            }

            throw new NotImplementedException();
        }

        public Size2DI GetBuildingSize(BuildingType building)
        {
            int side;

            if (building.TerranBuilding != TerranBuildingType.Unspecified)
            {
                side = terranBuildingSizes[building.TerranBuilding];
            }
            else if (building.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                side = protossBuildingSizes[building.ProtossBuilding];
            }
            else if (building.ZergBuilding != ZergBuildingType.Unspecified)
            {
                side = zergBuildingSizes[building.ZergBuilding];
            }
            else
            {
                throw new NotImplementedException();
            }

            return new Size2DI() { X = side, Y = side };
        }
        
        public Size2DI GetStructureSize(Unit unit)
        {
            if (terranBuildingTypesById.ContainsKey(unit.UnitType))
            {
                return GetBuildingSize(terranBuildingTypesById[unit.UnitType]);
            }

            if (protossBuildingTypesById.ContainsKey(unit.UnitType))
            {
                return GetBuildingSize(protossBuildingTypesById[unit.UnitType]);
            }

            if (zergBuildingTypesById.ContainsKey(unit.UnitType))
            {
                return GetBuildingSize(zergBuildingTypesById[unit.UnitType]);
            }

            // TODO: Determine if all mineral fields are the same size
            if (mineralFieldTypes.Contains(unit.UnitType))
            {
                return new Size2DI { X = 2, Y = 1 };
            }

            if (vespeneGeyserTypes.Contains(unit.UnitType))
            {
                return new Size2DI { X = 3, Y = 3 };
            }

            // Various other non-building structures are a bit of a puzzler for the moment.
            return new Size2DI { X = 0, Y = 0 };
        }

        public uint GetBuildAction(BuildingOrUnitType buildingOrUnit)
        {
            if (buildingOrUnit.TerranBuilding != TerranBuildingType.Unspecified)
            {
                return buildTerranBuildingActions[buildingOrUnit.TerranBuilding];
            }
            else if (buildingOrUnit.ProtossBuilding != ProtossBuildingType.Unspecified)
            {
                return buildProtossBuildingActions[buildingOrUnit.ProtossBuilding];
            }
            else if (buildingOrUnit.ZergBuilding != ZergBuildingType.Unspecified)
            {
                return buildZergBuildingActions[buildingOrUnit.ZergBuilding];
            }
            else if (buildingOrUnit.TerranUnit != TerranUnitType.Unspecified)
            {
                return createTerranUnitActions[buildingOrUnit.TerranUnit];
            }
            else if (buildingOrUnit.ProtossUnit != ProtossUnitType.Unspecified)
            {
                return createProtossUnitActions[buildingOrUnit.ProtossUnit];
            }
            else if (buildingOrUnit.ZergUnit != ZergUnitType.Unspecified)
            {
                return createZergUnitActions[buildingOrUnit.ZergUnit];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool IsBuildingSomething(Unit unit)
        {
            var orders = unit.Orders.FirstOrDefault();

            if (orders == null)
            {
                return false;
            }

            var ability = orders.AbilityId;

            return buildTerranBuildingActions.ContainsValue(ability) ||
                   buildProtossBuildingActions.ContainsValue(ability) ||
                   buildZergBuildingActions.ContainsValue(ability) ||
                   createTerranUnitActions.ContainsValue(ability) ||
                   createProtossUnitActions.ContainsValue(ability) ||
                   createZergUnitActions.ContainsValue(ability);
        }

        public bool IsBuilding(Unit unit, BuildingOrUnitType target)
        {
            var orders = unit.Orders.FirstOrDefault();

            if (orders == null)
            {
                return false;
            }

            var ability = orders.AbilityId;
            return ability == GetBuildAction(target);
        }

        /// <summary>
        /// Gets the appropriate Rally ability for the given structure.
        /// 
        /// Note that this primarily exists to differentiate between the Hatchery and everything else,
        /// because the Hatchery has its own Rally command (because it produces both units and workers
        /// and therefore needs two, I guess). This will not ensure that the unit is a building.
        /// </summary>
        public uint GetRallyAbility(Unit unit)
        {
            if (IsUnitOfType(unit, ZergBuildingType.Hatchery) ||
                IsUnitOfType(unit, ZergBuildingType.Lair) ||
                IsUnitOfType(unit, ZergBuildingType.Hive))
            {
                return rallyUnitsHatchery;
            }

            return rally;
        }

        /// <summary>
        /// Gets the appropriate Rally Workers ability for the given structure.
        /// 
        /// Throws ArgumentException if it's not a main base structure.
        /// </summary>
        public uint GetRallyWorkersAbility(Unit unit)
        {
            if (IsUnitOfType(unit, TerranBuildingType.CommandCenter) ||
                IsUnitOfType(unit, TerranBuildingType.OrbitalCommand) ||
                IsUnitOfType(unit, TerranBuildingType.PlanetaryFortress))
            {
                return rallyWorkersCommandCenter;
            }

            if (IsUnitOfType(unit, ProtossBuildingType.Nexus))
            {
                return rallyWorkersNexus;
            }
            
            if (IsUnitOfType(unit, ZergBuildingType.Hatchery) ||
                IsUnitOfType(unit, ZergBuildingType.Lair) ||
                IsUnitOfType(unit, ZergBuildingType.Hive))
            {
                return rallyWorkersHatchery;
            }

            throw new ArgumentException("Unit was not a CommandCenter/Nexus/Hatchery or equivalent.");
        }

        public BuildingOrUnitType GetBuildingOrUnitType(uint unitTypeId)
        {
            if (terranUnitTypesById.ContainsKey(unitTypeId))
            {
                return terranUnitTypesById[unitTypeId];
            }
            else if (protossUnitTypesById.ContainsKey(unitTypeId))
            {
                return protossUnitTypesById[unitTypeId];
            }
            else if (zergUnitTypesById.ContainsKey(unitTypeId))
            {
                return zergUnitTypesById[unitTypeId];
            }
            else if (terranBuildingTypesById.ContainsKey(unitTypeId))
            {
                return terranBuildingTypesById[unitTypeId];
            }
            else if (protossBuildingTypesById.ContainsKey(unitTypeId))
            {
                return protossBuildingTypesById[unitTypeId];
            }
            else if (zergBuildingTypesById.ContainsKey(unitTypeId))
            {
                return zergBuildingTypesById[unitTypeId];
            }

            throw new NotImplementedException();
        }

        public Unit2 ConvertUnit(Proto.Unit unit)
        {
            var unitTypeId = unit.UnitType;

            if (terranUnitTypesById.ContainsKey(unitTypeId))
            {
                return new TerranUnit(unit, this);
            }
            else if (protossUnitTypesById.ContainsKey(unitTypeId))
            {
                return new ProtossUnit(unit, this);
            }
            else if (zergUnitTypesById.ContainsKey(unitTypeId))
            {
                return new ZergUnit(unit, this);
            }
            else if (terranBuildingTypesById.ContainsKey(unitTypeId))
            {
                return new TerranBuilding(unit, this);
            }
            else if (protossBuildingTypesById.ContainsKey(unitTypeId))
            {
                return new ProtossBuilding(unit, this);
            }
            else if (zergBuildingTypesById.ContainsKey(unitTypeId))
            {
                return new ZergBuilding(unit, this);
            }
            else
            {
                return new UnspecifiedUnit(unit, this);
            }
        }
    }
}
