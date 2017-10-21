using System;
using System.Collections.Generic;
using System.Linq;
using ProxyStarcraft.Proto;

namespace ProxyStarcraft
{
    // TODO: Move into the Client library? Probably would require some refactoring,
    // but I think that would be the best place for it in the long run.

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
            { TerranBuildingType.BarracksTechLab, 2 },
            { TerranBuildingType.BarracksReactor, 2 },
            { TerranBuildingType.FactoryTechLab, 2 },
            { TerranBuildingType.FactoryReactor, 2 },
            { TerranBuildingType.StarportTechLab, 2 },
            { TerranBuildingType.StarportReactor, 2 },
            { TerranBuildingType.PlanetaryFortress, 5 },
            { TerranBuildingType.OrbitalCommand, 5 }
        };

        private Dictionary<ProtossBuildingType, int> protossBuildingSizes = new Dictionary<ProtossBuildingType, int>
        {
            { ProtossBuildingType.Nexus, 5 },
            { ProtossBuildingType.Assimilator, 3 },
            { ProtossBuildingType.Pylon, 2 }, // ?
            { ProtossBuildingType.Gateway, 3 }, // ?
            { ProtossBuildingType.Forge, 3 }, // ?
            { ProtossBuildingType.CyberneticsCore, 3 }, // ?
            { ProtossBuildingType.PhotonCannon, 2 }, // ?
            { ProtossBuildingType.RoboticsFacility, 3 }, // ?
            { ProtossBuildingType.WarpGate, 3 }, // ?
            { ProtossBuildingType.Stargate, 3 }, // ?
            { ProtossBuildingType.TwilightCouncil, 3 }, // ?
            { ProtossBuildingType.RoboticsBay, 3 }, // ?
            { ProtossBuildingType.FleetBeacon, 3 }, // ?
            { ProtossBuildingType.TemplarArchive, 3 }, // ?
            { ProtossBuildingType.DarkShrine, 2 } // ?
        };

        private Dictionary<ZergBuildingType, int> zergBuildingSizes = new Dictionary<ZergBuildingType, int>
        {
            { ZergBuildingType.Hatchery, 5 },
            { ZergBuildingType.Extractor, 3 },
            { ZergBuildingType.SpawningPool, 3 }, // ?
            { ZergBuildingType.EvolutionChamber, 3 }, // ?
            { ZergBuildingType.RoachWarren, 3 }, // ?
            { ZergBuildingType.BanelingNest, 3 }, // ?
            { ZergBuildingType.SpineCrawler, 2 }, // ?
            { ZergBuildingType.SporeCrawler, 2 }, // ?
            { ZergBuildingType.Lair, 5 },
            { ZergBuildingType.HydraliskDen, 3 }, // ?
            { ZergBuildingType.LurkerDen, 3 }, // ?
            { ZergBuildingType.InfestationPit, 3 }, // ?
            { ZergBuildingType.Spire, 2 }, // ?
            { ZergBuildingType.NydusNetwork, 2 }, // ?
            { ZergBuildingType.Hive, 5 },
            { ZergBuildingType.GreaterSpire, 3 }, // ?
            { ZergBuildingType.UltraliskCavern, 3 }, // ?
            { ZergBuildingType.CreepTumor, 1 }
        };

        private List<string> buildingUpgradeNames = new List<string>
        {
            "Morph OrbitalCommand",
            "Morph PlanetaryFortress",
            "Morph Lair",
            "Morph Hive",
            "Morph LurkerDen",
            "Morph GreaterSpire",
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
                .Where(ability => ability.FriendlyName.Contains("Build") || ability.FriendlyName.Contains("Train") || buildingUpgradeNames.Contains(ability.FriendlyName));

            scvHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather SCV")).AbilityId;
            muleHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Mule")).AbilityId;
            probeHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Probe")).AbilityId;
            droneHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Drone")).AbilityId;

            mineralFieldTypes = unitTypes.Values.Where(u => u.Name.Contains("MineralField")).Select(u => u.UnitId).ToList();
            vespeneGeyserTypes = unitTypes.Values.Where(u => u.Name.Contains("Vespene")).Select(u => u.UnitId).ToList();

            var abilitiesByName = buildAndTrainAbilities.ToDictionary(a => a.FriendlyName);
            
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
                { TerranBuildingType.BarracksTechLab, abilitiesByName["Build TechLab Barracks"].AbilityId },
                { TerranBuildingType.BarracksReactor, abilitiesByName["Build Reactor Barracks"].AbilityId },
                { TerranBuildingType.FactoryTechLab, abilitiesByName["Build TechLab Factory"].AbilityId },
                { TerranBuildingType.FactoryReactor, abilitiesByName["Build Reactor Factory"].AbilityId },
                { TerranBuildingType.StarportTechLab, abilitiesByName["Build TechLab Starport"].AbilityId },
                { TerranBuildingType.StarportReactor, abilitiesByName["Build Reactor Starport"].AbilityId },
                { TerranBuildingType.PlanetaryFortress, abilitiesByName["Morph PlanetaryFortress"].AbilityId },
                { TerranBuildingType.OrbitalCommand, abilitiesByName["Morph OrbitalCommand"].AbilityId }
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
                { ZergBuildingType.Lair, abilitiesByName["Morph Lair"].AbilityId },
                { ZergBuildingType.HydraliskDen, abilitiesByName["Build HydraliskDen"].AbilityId },
                { ZergBuildingType.LurkerDen, abilitiesByName["Morph LurkerDen"].AbilityId },
                { ZergBuildingType.InfestationPit, abilitiesByName["Build InfestationPit"].AbilityId },
                { ZergBuildingType.Spire, abilitiesByName["Build Spire"].AbilityId },
                { ZergBuildingType.NydusNetwork, abilitiesByName["Build NydusNetwork"].AbilityId },
                { ZergBuildingType.Hive, abilitiesByName["Morph Hive"].AbilityId },
                { ZergBuildingType.GreaterSpire, abilitiesByName["Morph GreaterSpire"].AbilityId },
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
            zergUnitTypesById.Add(unitTypesByName["Egg"].UnitId, ZergUnitType.Cocoon);
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
            zergUnitTypesById.Add(unitTypesByName["Ravager"].UnitId, ZergUnitType.Ravager);
            zergUnitTypesById.Add(unitTypesByName["RavagerBurrowed"].UnitId, ZergUnitType.Ravager);
            zergUnitTypesById.Add(unitTypesByName["Hydralisk"].UnitId, ZergUnitType.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["HydraliskBurrowed"].UnitId, ZergUnitType.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["LurkerMP"].UnitId, ZergUnitType.Lurker);
            zergUnitTypesById.Add(unitTypesByName["LurkerMPBurrowed"].UnitId, ZergUnitType.Lurker);
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
            terranBuildingTypesById.Add(unitTypesByName["BarracksTechLab"].UnitId, TerranBuildingType.BarracksTechLab);
            terranBuildingTypesById.Add(unitTypesByName["BarracksReactor"].UnitId, TerranBuildingType.BarracksReactor);
            terranBuildingTypesById.Add(unitTypesByName["FactoryTechLab"].UnitId, TerranBuildingType.FactoryTechLab);
            terranBuildingTypesById.Add(unitTypesByName["FactoryReactor"].UnitId, TerranBuildingType.FactoryReactor);
            terranBuildingTypesById.Add(unitTypesByName["StarportTechLab"].UnitId, TerranBuildingType.StarportTechLab);
            terranBuildingTypesById.Add(unitTypesByName["StarportReactor"].UnitId, TerranBuildingType.StarportReactor);
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

        public IReadOnlyDictionary<uint, AbilityData> AbilityTypes => this.abilities;

        public IReadOnlyDictionary<uint, UnitTypeData> UnitTypes => this.unitTypes;
        
        public uint GetHarvestAbility(Proto.Unit unit)
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

        public bool IsHarvester(Proto.Unit unit)
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

        public bool IsUnitOfType(Proto.Unit unit, BuildingOrUnitType type)
        {
            switch (type.Value)
            {
                case TerranUnitType terranUnit:
                    return terranUnitTypesById.ContainsKey(unit.UnitType) && terranUnitTypesById[unit.UnitType] == terranUnit;
                case ProtossUnitType protossUnit:
                    return protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == protossUnit;
                case ZergUnitType zergUnit:
                    return zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == zergUnit;
                case TerranBuildingType terranBuilding:
                    return terranBuildingTypesById.ContainsKey(unit.UnitType) && terranBuildingTypesById[unit.UnitType] == terranBuilding;
                case ProtossBuildingType protossBuilding:
                    return protossBuildingTypesById.ContainsKey(unit.UnitType) && protossBuildingTypesById[unit.UnitType] == protossBuilding;
                case ZergBuildingType zergBuilding:
                    return zergBuildingTypesById.ContainsKey(unit.UnitType) && zergBuildingTypesById[unit.UnitType] == zergBuilding;
                default:
                    throw new NotImplementedException();
            }
        }
        
        public uint Move { get; private set; }

        public uint Attack { get; private set; }

        public uint MineralDeposit { get; private set; }

        public uint VespeneGeyser { get; private set; }

        public Size2DI GetBuildingSize(BuildCommand buildCommand)
        {
            return GetBuildingSize(buildCommand.Building);
        }

        public Size2DI GetBuildingSize(BuildingType building)
        {
            int side;

            switch (building.Value)
            {
                case TerranBuildingType terranBuilding:
                    side = terranBuildingSizes[terranBuilding];
                    break;
                case ProtossBuildingType protossBuilding:
                    side = protossBuildingSizes[protossBuilding];
                    break;
                case ZergBuildingType zergBuilding:
                    side = zergBuildingSizes[zergBuilding];
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new Size2DI() { X = side, Y = side };
        }

        // TODO: Collapse these two functions
        public Size2DI GetStructureSize(Unit unit)
        {
            return GetStructureSize(unit.Raw);
        }
        
        public Size2DI GetStructureSize(Proto.Unit unit)
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
            switch (buildingOrUnit.Value)
            {
                case TerranUnitType terranUnit:
                    return createTerranUnitActions[terranUnit];
                case ProtossUnitType protossUnit:
                    return createProtossUnitActions[protossUnit];
                case ZergUnitType zergUnit:
                    return createZergUnitActions[zergUnit];
                case TerranBuildingType terranBuilding:
                    return buildTerranBuildingActions[terranBuilding];
                case ProtossBuildingType protossBuilding:
                    return buildProtossBuildingActions[protossBuilding];
                case ZergBuildingType zergBuilding:
                    return buildZergBuildingActions[zergBuilding];
                default:
                    throw new NotImplementedException();
            }
        }

        public UnitCost GetCost(BuildingOrUnitType buildingOrUnit)
        {
            var unitTypeId = GetUnitTypeId(buildingOrUnit);
            var rawUnitType = this.UnitTypes[unitTypeId];

            var minerals = rawUnitType.MineralCost;
            var vespene = rawUnitType.VespeneCost;

            // Exception: upgrades list the full cost of everything (apparently)
            // TODO: Maybe don't hardcode these numerical values
            if (buildingOrUnit == TerranBuildingType.OrbitalCommand || buildingOrUnit == TerranBuildingType.PlanetaryFortress)
            {
                minerals = minerals - 400;
            }
            else if (buildingOrUnit == ZergBuildingType.Lair)
            {
                minerals = minerals - 400;
            }
            else if (buildingOrUnit == ZergBuildingType.Hive)
            {
                minerals = minerals - 500;
                vespene = vespene - 100;
            }

            var supply = rawUnitType.FoodRequired;
            var prerequisiteId = rawUnitType.TechRequirement; // TODO: handle tech_alias values, Tech Lab requirement
            var prerequisite = prerequisiteId > 0 ? GetBuildingOrUnitType(prerequisiteId) : null;

            return new UnitCost(minerals, vespene, supply, GetBuilder(buildingOrUnit), prerequisite);
        }

        public BuildingOrUnitType GetBuilder(BuildingOrUnitType buildingOrUnit)
        {
            switch (buildingOrUnit.Value)
            {
                case TerranUnitType terranUnit:
                    switch (terranUnit)
                    {
                        case TerranUnitType.SCV:
                            return TerranBuildingType.CommandCenter;

                        case TerranUnitType.MULE:
                            return TerranBuildingType.OrbitalCommand;

                        case TerranUnitType.Marine:
                        case TerranUnitType.Marauder:
                        case TerranUnitType.Reaper:
                        case TerranUnitType.Ghost:
                            return TerranBuildingType.Barracks;

                        case TerranUnitType.Hellion:
                        case TerranUnitType.Hellbat:
                        case TerranUnitType.SiegeTank:
                        case TerranUnitType.Cyclone:
                        case TerranUnitType.Thor:
                        case TerranUnitType.WidowMine:
                            return TerranBuildingType.Factory;

                        case TerranUnitType.Viking:
                        case TerranUnitType.Medivac:
                        case TerranUnitType.Liberator:
                        case TerranUnitType.Raven:
                        case TerranUnitType.Banshee:
                        case TerranUnitType.Battlecruiser:
                            return TerranBuildingType.Starport;

                        case TerranUnitType.AutoTurret:
                            return TerranUnitType.Raven;

                        case TerranUnitType.PointDefenseDrone:
                            return TerranUnitType.Raven;

                        default:
                            throw new NotImplementedException();
                    }
                case TerranBuildingType terranBuilding:
                    switch (terranBuilding)
                    {
                        case TerranBuildingType.CommandCenter:
                        case TerranBuildingType.Refinery:
                        case TerranBuildingType.SupplyDepot:
                        case TerranBuildingType.Barracks:
                        case TerranBuildingType.EngineeringBay:
                        case TerranBuildingType.Bunker:
                        case TerranBuildingType.SensorTower:
                        case TerranBuildingType.MissileTurret:
                        case TerranBuildingType.Factory:
                        case TerranBuildingType.GhostAcademy:
                        case TerranBuildingType.Starport:
                        case TerranBuildingType.Armory:
                        case TerranBuildingType.FusionCore:
                            return TerranUnitType.SCV;

                        case TerranBuildingType.BarracksTechLab:
                        case TerranBuildingType.BarracksReactor:
                            return TerranBuildingType.Barracks;

                        case TerranBuildingType.FactoryTechLab:
                        case TerranBuildingType.FactoryReactor:
                            return TerranBuildingType.Factory;

                        case TerranBuildingType.StarportTechLab:
                        case TerranBuildingType.StarportReactor:
                            return TerranBuildingType.Starport;

                        case TerranBuildingType.PlanetaryFortress:
                        case TerranBuildingType.OrbitalCommand:
                            return TerranBuildingType.CommandCenter;

                        default:
                            throw new NotImplementedException();
                    }
                case ProtossUnitType protossUnit:

                    switch (protossUnit)
                    {
                        case ProtossUnitType.Probe:
                        case ProtossUnitType.MothershipCore:
                            return ProtossBuildingType.Nexus;

                        case ProtossUnitType.Zealot:
                        case ProtossUnitType.Stalker:
                        case ProtossUnitType.Sentry:
                        case ProtossUnitType.Adept:
                        case ProtossUnitType.HighTemplar:
                        case ProtossUnitType.DarkTemplar:
                            return ProtossBuildingType.Gateway;
                        case ProtossUnitType.Immortal:
                        case ProtossUnitType.Colossus:
                        case ProtossUnitType.Disruptor:
                        case ProtossUnitType.Observer:
                        case ProtossUnitType.WarpPrism:
                            return ProtossBuildingType.RoboticsFacility;

                        case ProtossUnitType.Archon:
                            return ProtossUnitType.HighTemplar;

                        case ProtossUnitType.Phoenix:
                        case ProtossUnitType.VoidRay:
                        case ProtossUnitType.Oracle:
                        case ProtossUnitType.Carrier:
                        case ProtossUnitType.Tempest:
                            return ProtossBuildingType.Stargate;

                        case ProtossUnitType.Mothership:
                            return ProtossUnitType.MothershipCore;

                        default:
                            throw new NotImplementedException();
                    }
                case ProtossBuildingType protossBuilding:
                    switch (protossBuilding)
                    {
                        case ProtossBuildingType.Nexus:
                        case ProtossBuildingType.Assimilator:
                        case ProtossBuildingType.Pylon:
                        case ProtossBuildingType.Gateway:
                        case ProtossBuildingType.Forge:
                        case ProtossBuildingType.CyberneticsCore:
                        case ProtossBuildingType.PhotonCannon:
                        case ProtossBuildingType.RoboticsFacility:
                        case ProtossBuildingType.Stargate:
                        case ProtossBuildingType.TwilightCouncil:
                        case ProtossBuildingType.RoboticsBay:
                        case ProtossBuildingType.FleetBeacon:
                        case ProtossBuildingType.TemplarArchive:
                        case ProtossBuildingType.DarkShrine:
                            return ProtossUnitType.Probe;

                        case ProtossBuildingType.WarpGate:
                            return ProtossBuildingType.Gateway;

                        default:
                            throw new NotImplementedException();
                    }
                case ZergUnitType zergUnit:
                    switch (zergUnit)
                    {
                        case ZergUnitType.Larva:
                        case ZergUnitType.Queen:
                            return ZergBuildingType.Hatchery; // TODO: Recognize that Lairs/Hives are basically Hatcheries

                        case ZergUnitType.Drone:
                        case ZergUnitType.Zergling:
                        case ZergUnitType.Roach:
                        case ZergUnitType.Hydralisk:
                        case ZergUnitType.Infestor:
                        case ZergUnitType.SwarmHost:
                        case ZergUnitType.Ultralisk:
                        case ZergUnitType.Overlord:
                        case ZergUnitType.Mutalisk:
                        case ZergUnitType.Corruptor:
                        case ZergUnitType.Viper:
                            return ZergUnitType.Larva;

                        case ZergUnitType.Locust:
                            return ZergUnitType.SwarmHost;

                        case ZergUnitType.Lurker:
                            return ZergUnitType.Hydralisk;

                        case ZergUnitType.Ravager:
                            return ZergUnitType.Roach;

                        case ZergUnitType.InfestedTerran:
                            return ZergUnitType.Infestor;

                        case ZergUnitType.BroodLord:
                            return ZergUnitType.Corruptor;

                        case ZergUnitType.Broodling:
                            return ZergUnitType.BroodLord;

                        case ZergUnitType.Overseer:
                            return ZergUnitType.Overlord;

                        case ZergUnitType.Changeling:
                            return ZergUnitType.Overseer;

                        case ZergUnitType.Baneling:
                            return ZergUnitType.Zergling;

                        case ZergUnitType.NydusWorm:
                            return ZergBuildingType.NydusNetwork;
                        default:
                            throw new NotImplementedException();
                    }
                case ZergBuildingType zergBuilding:
                    switch (zergBuilding)
                    {
                        case ZergBuildingType.Hatchery:
                        case ZergBuildingType.Extractor:
                        case ZergBuildingType.SpawningPool:
                        case ZergBuildingType.EvolutionChamber:
                        case ZergBuildingType.RoachWarren:
                        case ZergBuildingType.BanelingNest:
                        case ZergBuildingType.SpineCrawler:
                        case ZergBuildingType.SporeCrawler:
                        case ZergBuildingType.HydraliskDen:
                        case ZergBuildingType.InfestationPit:
                        case ZergBuildingType.Spire:
                        case ZergBuildingType.NydusNetwork:
                        case ZergBuildingType.UltraliskCavern:
                            return ZergUnitType.Drone;

                        case ZergBuildingType.LurkerDen:
                            return ZergBuildingType.HydraliskDen;

                        case ZergBuildingType.GreaterSpire:
                            return ZergBuildingType.Spire;

                        case ZergBuildingType.Lair:
                            return ZergBuildingType.Hatchery;

                        case ZergBuildingType.Hive:
                            return ZergBuildingType.Lair;

                        case ZergBuildingType.CreepTumor:
                            return ZergUnitType.Queen;

                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether the requested building is actually an upgrade of another building.
        /// </summary>
        public bool IsUpgrade(BuildingType building)
        {
            return building == TerranBuildingType.PlanetaryFortress ||
                   building == TerranBuildingType.OrbitalCommand ||
                   building == ZergBuildingType.Lair ||
                   building == ZergBuildingType.Hive ||
                   building == ZergBuildingType.LurkerDen ||
                   building == ZergBuildingType.GreaterSpire;
        }

        /// <summary>
        /// Gets a unique identifier for the unit type (or one of the unit types if there are multiple, and it's not picky) specified.
        /// </summary>
        private uint GetUnitTypeId(BuildingOrUnitType buildingOrUnit)
        {
            switch (buildingOrUnit.Value)
            {
                case TerranUnitType terranUnit:
                    return terranUnitTypesById.First(pair => pair.Value == terranUnit).Key;
                case ProtossUnitType protossUnit:
                    return protossUnitTypesById.First(pair => pair.Value == protossUnit).Key;
                case ZergUnitType zergUnit:
                    return zergUnitTypesById.First(pair => pair.Value == zergUnit).Key;
                case TerranBuildingType terranBuilding:
                    return terranBuildingTypesById.First(pair => pair.Value == terranBuilding).Key;
                case ProtossBuildingType protossBuilding:
                    return protossBuildingTypesById.First(pair => pair.Value == protossBuilding).Key;
                case ZergBuildingType zergBuilding:
                    return zergBuildingTypesById.First(pair => pair.Value == zergBuilding).Key;
            }

            throw new NotImplementedException();
        }

        public bool IsBuildingSomething(Proto.Unit unit)
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

        public bool IsBuilding(Proto.Unit unit, BuildingOrUnitType target)
        {
            var orders = unit.Orders.FirstOrDefault();

            if (orders == null)
            {
                return false;
            }

            var ability = orders.AbilityId;
            return ability == GetBuildAction(target);
        }

        public uint GetAbilityId(Command command)
        {
            switch (command)
            {
                case MoveCommand moveCommand:
                    return this.Move;
                case AttackCommand attackCommand:
                case AttackMoveCommand attackMoveCommand:
                    return this.Attack;
                case HarvestCommand harvestCommand:
                    return GetHarvestAbility(command.Unit.Raw);
                case BuildCommand buildCommand:
                    return GetBuildAction(buildCommand.Building);
                case TrainCommand trainCommand:
                    return GetBuildAction(trainCommand.Target);
                case RallyTargetCommand rallyTargetCommand:
                    return GetRallyAbility(rallyTargetCommand.Unit.Raw);
                case RallyLocationCommand rallyLocationCommand:
                    return GetRallyAbility(rallyLocationCommand.Unit.Raw);
                case RallyWorkersLocationCommand rallyWorkersLocationCommand:
                    return GetRallyWorkersAbility(rallyWorkersLocationCommand.Unit.Raw);
                case RallyWorkersTargetCommand rallyWorkersTargetCommand:
                    return GetRallyWorkersAbility(rallyWorkersTargetCommand.Unit.Raw);
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the appropriate Rally ability for the given structure.
        /// 
        /// Note that this primarily exists to differentiate between the Hatchery and everything else,
        /// because the Hatchery has its own Rally command (because it produces both units and workers
        /// and therefore needs two, I guess). This will not ensure that the unit is a building.
        /// </summary>
        public uint GetRallyAbility(Proto.Unit unit)
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
        public uint GetRallyWorkersAbility(Proto.Unit unit)
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

        public Unit ConvertUnit(Proto.Unit unit)
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
