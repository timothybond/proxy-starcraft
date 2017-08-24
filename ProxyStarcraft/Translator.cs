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
    /// </summary>
    public class Translator
    {
        private Dictionary<TerranUnit, uint> createTerranUnitActions;
        private Dictionary<ProtossUnit, uint> createProtossUnitActions;
        private Dictionary<ZergUnit, uint> createZergUnitActions;

        private Dictionary<TerranBuilding, uint> buildTerranBuildingActions;
        private Dictionary<ProtossBuilding, uint> buildProtossBuildingActions;
        private Dictionary<ZergBuilding, uint> buildZergBuildingActions;

        private Dictionary<uint, TerranUnit> terranUnitTypesById;
        private Dictionary<uint, ProtossUnit> protossUnitTypesById;
        private Dictionary<uint, ZergUnit> zergUnitTypesById;

        private Dictionary<uint, TerranBuilding> terranBuildingTypesById;
        private Dictionary<uint, ProtossBuilding> protossBuildingTypesById;
        private Dictionary<uint, ZergBuilding> zergBuildingTypesById;

        private uint scvHarvest;
        private uint muleHarvest;
        private uint probeHarvest;
        private uint droneHarvest;

        private IReadOnlyList<uint> mineralFieldTypes;

        private Dictionary<TerranBuilding, int> terranBuildingSizes = new Dictionary<TerranBuilding, int>
        {
            { TerranBuilding.CommandCenter, 5 },
            { TerranBuilding.Refinery, 3 },
            { TerranBuilding.SupplyDepot, 2 },
            { TerranBuilding.Barracks, 3 },
            { TerranBuilding.EngineeringBay, 3 },
            { TerranBuilding.Bunker, 3 },
            { TerranBuilding.SensorTower, 1 },
            { TerranBuilding.MissileTurret, 2 },
            { TerranBuilding.Factory, 3 },
            { TerranBuilding.GhostAcademy, 3 },
            { TerranBuilding.Starport, 3 },
            { TerranBuilding.Armory, 3 },
            { TerranBuilding.FusionCore, 3 }, // ?
            { TerranBuilding.TechLab, 2 },
            { TerranBuilding.Reactor, 2 },
            { TerranBuilding.PlanetaryFortress, 5 },
            { TerranBuilding.OrbitalCommand, 5 }
        };

        private Dictionary<ProtossBuilding, int> protossBuildingSizes = new Dictionary<ProtossBuilding, int>
        {
            { ProtossBuilding.Nexus, 5 },
            { ProtossBuilding.Assimilator, 3 },
            { ProtossBuilding.Pylon, 2 }, // ?
            { ProtossBuilding.Gateway, 0 },
            { ProtossBuilding.Forge, 0 },
            { ProtossBuilding.CyberneticsCore, 0 },
            { ProtossBuilding.PhotonCannon, 0 },
            { ProtossBuilding.RoboticsFacility, 0 },
            { ProtossBuilding.WarpGate, 0 },
            { ProtossBuilding.Stargate, 0 },
            { ProtossBuilding.TwilightCouncil, 0 },
            { ProtossBuilding.RoboticsBay, 0 },
            { ProtossBuilding.FleetBeacon, 0 },
            { ProtossBuilding.TemplarArchive, 0 },
            { ProtossBuilding.DarkShrine, 0 }
        };

        private Dictionary<ZergBuilding, int> zergBuildingSizes = new Dictionary<ZergBuilding, int>
        {
            { ZergBuilding.Hatchery, 5 },
            { ZergBuilding.Extractor, 3 },
            { ZergBuilding.SpawningPool, 0 },
            { ZergBuilding.EvolutionChamber, 0 },
            { ZergBuilding.RoachWarren, 0 },
            { ZergBuilding.BanelingNest, 0 },
            { ZergBuilding.SpineCrawler, 0 },
            { ZergBuilding.SporeCrawler, 0 },
            { ZergBuilding.Lair, 5 },
            { ZergBuilding.HydraliskDen, 0 },
            { ZergBuilding.LurkerDen, 0 },
            { ZergBuilding.InfestationPit, 0 },
            { ZergBuilding.Spire, 0 },
            { ZergBuilding.NydusNetwork, 0 },
            { ZergBuilding.Hive, 5 },
            { ZergBuilding.GreaterSpire, 0 },
            { ZergBuilding.UltraliskCavern, 0 },
            { ZergBuilding.CreepTumor, 1 }
        };

        public Translator(Dictionary<uint, AbilityData> abilities, Dictionary<uint, UnitTypeData> unitTypes)
        {
            // Somewhat-amusing trick: although there are tons of non-used abilities,
            // you can quickly narrow it down to ones that actually appear in-game
            // by only using the ones with hotkeys. Every button on the screen has a hotkey.
            var hotkeyedAbilities = abilities.Values.Where(ability => !string.IsNullOrEmpty(ability.Hotkey)).ToList();

            Move = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Move")).AbilityId;
            Attack = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Attack Attack")).AbilityId;

            var buildAndTrainAbilities = hotkeyedAbilities
                .Where(ability => ability.FriendlyName.Contains("Build") || ability.FriendlyName.Contains("Train"));

            scvHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather SCV")).AbilityId;
            muleHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Mule")).AbilityId;
            probeHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Probe")).AbilityId;
            droneHarvest = hotkeyedAbilities.Single(ability => string.Equals(ability.FriendlyName, "Harvest Gather Drone")).AbilityId;

            mineralFieldTypes = unitTypes.Values.Where(u => u.Name.Contains("MineralField")).Select(u => u.UnitId).ToList();

            var abilitiesByName = new Dictionary<string, AbilityData>();
            
            foreach (var ability in buildAndTrainAbilities)
            {
                abilitiesByName.Add(ability.FriendlyName, ability);
            }

            createTerranUnitActions = new Dictionary<TerranUnit, uint>
            {
                { TerranUnit.SCV, abilitiesByName["Train SCV"].AbilityId },
                { TerranUnit.Marine, abilitiesByName["Train Marine"].AbilityId },
                { TerranUnit.Marauder, abilitiesByName["Train Marauder"].AbilityId },
                { TerranUnit.Reaper, abilitiesByName["Train Reaper"].AbilityId },
                { TerranUnit.Ghost, abilitiesByName["Train Ghost"].AbilityId },
                { TerranUnit.Hellion, abilitiesByName["Train Hellion"].AbilityId },
                { TerranUnit.Hellbat, abilitiesByName["Train Hellbat"].AbilityId },
                { TerranUnit.SiegeTank, abilitiesByName["Train SiegeTank"].AbilityId },
                { TerranUnit.Cyclone, abilitiesByName["Train Cyclone"].AbilityId },
                { TerranUnit.Thor, abilitiesByName["Train Thor"].AbilityId },
                { TerranUnit.Viking, abilitiesByName["Train VikingFighter"].AbilityId },
                { TerranUnit.Medivac, abilitiesByName["Train Medivac"].AbilityId },
                { TerranUnit.Liberator, abilitiesByName["Train Liberator"].AbilityId },
                { TerranUnit.Raven, abilitiesByName["Train Raven"].AbilityId },
                { TerranUnit.Banshee, abilitiesByName["Train Banshee"].AbilityId },
                { TerranUnit.Battlecruiser, abilitiesByName["Train Battlecruiser"].AbilityId },
                { TerranUnit.WidowMine, abilitiesByName["Train WidowMine"].AbilityId },
            };

            createProtossUnitActions = new Dictionary<ProtossUnit, uint>
            {
                { ProtossUnit.Probe, abilitiesByName["Train Probe"].AbilityId },
                { ProtossUnit.Zealot, abilitiesByName["Train Zealot"].AbilityId },
                { ProtossUnit.Stalker, abilitiesByName["Train Stalker"].AbilityId },
                { ProtossUnit.Sentry, abilitiesByName["Train Sentry"].AbilityId },
                { ProtossUnit.Adept, abilitiesByName["Train Adept"].AbilityId },
                { ProtossUnit.HighTemplar, abilitiesByName["Train HighTemplar"].AbilityId },
                { ProtossUnit.DarkTemplar, abilitiesByName["Train DarkTemplar"].AbilityId },
                { ProtossUnit.Immortal, abilitiesByName["Train Immortal"].AbilityId },
                { ProtossUnit.Colossus, abilitiesByName["Train Colossus"].AbilityId },
                { ProtossUnit.Disruptor, abilitiesByName["Train Disruptor"].AbilityId },
                { ProtossUnit.Observer, abilitiesByName["Train Observer"].AbilityId },
                { ProtossUnit.WarpPrism, abilitiesByName["Train WarpPrism"].AbilityId },
                { ProtossUnit.Phoenix, abilitiesByName["Train Phoenix"].AbilityId },
                { ProtossUnit.VoidRay, abilitiesByName["Train VoidRay"].AbilityId },
                { ProtossUnit.Oracle, abilitiesByName["Train Oracle"].AbilityId },
                { ProtossUnit.Carrier, abilitiesByName["Train Carrier"].AbilityId },
                { ProtossUnit.Tempest, abilitiesByName["Train Tempest"].AbilityId },
            };

            createZergUnitActions = new Dictionary<ZergUnit, uint>
            {
                { ZergUnit.Drone, abilitiesByName["Train Drone"].AbilityId },
                { ZergUnit.Queen, abilitiesByName["Train Queen"].AbilityId },
                { ZergUnit.Zergling, abilitiesByName["Train Zergling"].AbilityId },
                { ZergUnit.Baneling, abilitiesByName["Train Baneling"].AbilityId },
                { ZergUnit.Roach, abilitiesByName["Train Roach"].AbilityId },
                { ZergUnit.Hydralisk, abilitiesByName["Train Hydralisk"].AbilityId },
                { ZergUnit.Infestor, abilitiesByName["Train Infestor"].AbilityId },
                { ZergUnit.SwarmHost, abilitiesByName["Train SwarmHost"].AbilityId },
                { ZergUnit.Ultralisk, abilitiesByName["Train Ultralisk"].AbilityId },
                { ZergUnit.Overlord, abilitiesByName["Train Overlord"].AbilityId },
                { ZergUnit.Mutalisk, abilitiesByName["Train Mutalisk"].AbilityId },
                { ZergUnit.Corruptor, abilitiesByName["Train Corruptor"].AbilityId },
                { ZergUnit.Viper, abilitiesByName["Train Viper"].AbilityId },
            };

            buildTerranBuildingActions = new Dictionary<TerranBuilding, uint>
            {
                { TerranBuilding.CommandCenter, abilitiesByName["Build CommandCenter"].AbilityId },
                { TerranBuilding.Refinery, abilitiesByName["Build Refinery"].AbilityId },
                { TerranBuilding.SupplyDepot, abilitiesByName["Build SupplyDepot"].AbilityId },
                { TerranBuilding.Barracks, abilitiesByName["Build Barracks"].AbilityId },
                { TerranBuilding.EngineeringBay, abilitiesByName["Build EngineeringBay"].AbilityId },
                { TerranBuilding.Bunker, abilitiesByName["Build Bunker"].AbilityId },
                { TerranBuilding.SensorTower, abilitiesByName["Build SensorTower"].AbilityId },
                { TerranBuilding.MissileTurret, abilitiesByName["Build MissileTurret"].AbilityId },
                { TerranBuilding.Factory, abilitiesByName["Build Factory"].AbilityId },
                { TerranBuilding.GhostAcademy, abilitiesByName["Build GhostAcademy"].AbilityId },
                { TerranBuilding.Starport, abilitiesByName["Build Starport"].AbilityId },
                { TerranBuilding.Armory, abilitiesByName["Build Armory"].AbilityId },
                { TerranBuilding.FusionCore, abilitiesByName["Build FusionCore"].AbilityId },
            };

            buildProtossBuildingActions = new Dictionary<ProtossBuilding, uint>
            {
                { ProtossBuilding.Nexus, abilitiesByName["Build Nexus"].AbilityId },
                { ProtossBuilding.Assimilator, abilitiesByName["Build Assimilator"].AbilityId },
                { ProtossBuilding.Pylon, abilitiesByName["Build Pylon"].AbilityId },
                { ProtossBuilding.Gateway, abilitiesByName["Build Gateway"].AbilityId },
                { ProtossBuilding.Forge, abilitiesByName["Build Forge"].AbilityId },
                { ProtossBuilding.CyberneticsCore, abilitiesByName["Build CyberneticsCore"].AbilityId },
                { ProtossBuilding.PhotonCannon, abilitiesByName["Build PhotonCannon"].AbilityId },
                { ProtossBuilding.RoboticsFacility, abilitiesByName["Build RoboticsFacility"].AbilityId },
                { ProtossBuilding.Stargate, abilitiesByName["Build Stargate"].AbilityId },
                { ProtossBuilding.TwilightCouncil, abilitiesByName["Build TwilightCouncil"].AbilityId },
                { ProtossBuilding.RoboticsBay, abilitiesByName["Build RoboticsBay"].AbilityId },
                { ProtossBuilding.FleetBeacon, abilitiesByName["Build FleetBeacon"].AbilityId },
                { ProtossBuilding.TemplarArchive, abilitiesByName["Build TemplarArchive"].AbilityId },
                { ProtossBuilding.DarkShrine, abilitiesByName["Build DarkShrine"].AbilityId }
            };

            buildZergBuildingActions = new Dictionary<ZergBuilding, uint>
            {
                { ZergBuilding.Hatchery, abilitiesByName["Build Hatchery"].AbilityId },
                { ZergBuilding.Extractor, abilitiesByName["Build Extractor"].AbilityId },
                { ZergBuilding.SpawningPool, abilitiesByName["Build SpawningPool"].AbilityId },
                { ZergBuilding.EvolutionChamber, abilitiesByName["Build EvolutionChamber"].AbilityId },
                { ZergBuilding.RoachWarren, abilitiesByName["Build RoachWarren"].AbilityId },
                { ZergBuilding.BanelingNest, abilitiesByName["Build BanelingNest"].AbilityId },
                { ZergBuilding.SpineCrawler, abilitiesByName["Build SpineCrawler"].AbilityId },
                { ZergBuilding.SporeCrawler, abilitiesByName["Build SporeCrawler"].AbilityId },
                { ZergBuilding.HydraliskDen, abilitiesByName["Build HydraliskDen"].AbilityId },
                { ZergBuilding.InfestationPit, abilitiesByName["Build InfestationPit"].AbilityId },
                { ZergBuilding.Spire, abilitiesByName["Build Spire"].AbilityId },
                { ZergBuilding.NydusNetwork, abilitiesByName["Build NydusNetwork"].AbilityId },
                { ZergBuilding.UltraliskCavern, abilitiesByName["Build UltraliskCavern"].AbilityId }
            };

            var unitTypesByName = unitTypes.Values.Where(unitType => !string.IsNullOrEmpty(unitType.Name)).ToDictionary(unitType => unitType.Name);

            terranUnitTypesById = new Dictionary<uint, TerranUnit>();

            terranUnitTypesById.Add(unitTypesByName["SCV"].UnitId, TerranUnit.SCV);
            terranUnitTypesById.Add(unitTypesByName["MULE"].UnitId, TerranUnit.MULE);
            terranUnitTypesById.Add(unitTypesByName["Marine"].UnitId, TerranUnit.Marine);
            terranUnitTypesById.Add(unitTypesByName["Marauder"].UnitId, TerranUnit.Marauder);
            terranUnitTypesById.Add(unitTypesByName["Reaper"].UnitId, TerranUnit.Reaper);
            terranUnitTypesById.Add(unitTypesByName["Ghost"].UnitId, TerranUnit.Ghost);
            terranUnitTypesById.Add(unitTypesByName["HellionTank"].UnitId, TerranUnit.Hellion);
            terranUnitTypesById.Add(unitTypesByName["Hellion"].UnitId, TerranUnit.Hellion);
            terranUnitTypesById.Add(unitTypesByName["SiegeTank"].UnitId, TerranUnit.SiegeTank);
            terranUnitTypesById.Add(unitTypesByName["Cyclone"].UnitId, TerranUnit.Cyclone);
            terranUnitTypesById.Add(unitTypesByName["Thor"].UnitId, TerranUnit.Thor);
            terranUnitTypesById.Add(unitTypesByName["VikingFighter"].UnitId, TerranUnit.Viking);
            terranUnitTypesById.Add(unitTypesByName["VikingAssault"].UnitId, TerranUnit.Viking);
            terranUnitTypesById.Add(unitTypesByName["Medivac"].UnitId, TerranUnit.Medivac);
            terranUnitTypesById.Add(unitTypesByName["Liberator"].UnitId, TerranUnit.Liberator);
            terranUnitTypesById.Add(unitTypesByName["Raven"].UnitId, TerranUnit.Raven);
            terranUnitTypesById.Add(unitTypesByName["Banshee"].UnitId, TerranUnit.Banshee);
            terranUnitTypesById.Add(unitTypesByName["Battlecruiser"].UnitId, TerranUnit.Battlecruiser);
            terranUnitTypesById.Add(unitTypesByName["WidowMine"].UnitId, TerranUnit.WidowMine);
            terranUnitTypesById.Add(unitTypesByName["AutoTurret"].UnitId, TerranUnit.AutoTurret);
            terranUnitTypesById.Add(unitTypesByName["PointDefenseDrone"].UnitId, TerranUnit.PointDefenseDrone);

            zergUnitTypesById = new Dictionary<uint, ZergUnit>();

            zergUnitTypesById.Add(unitTypesByName["Larva"].UnitId, ZergUnit.Larva);
            zergUnitTypesById.Add(unitTypesByName["Drone"].UnitId, ZergUnit.Drone);
            zergUnitTypesById.Add(unitTypesByName["DroneBurrowed"].UnitId, ZergUnit.Drone);
            zergUnitTypesById.Add(unitTypesByName["Queen"].UnitId, ZergUnit.Queen);
            zergUnitTypesById.Add(unitTypesByName["QueenBurrowed"].UnitId, ZergUnit.Queen);
            zergUnitTypesById.Add(unitTypesByName["Zergling"].UnitId, ZergUnit.Zergling);
            zergUnitTypesById.Add(unitTypesByName["ZerglingBurrowed"].UnitId, ZergUnit.Zergling);
            zergUnitTypesById.Add(unitTypesByName["Baneling"].UnitId, ZergUnit.Baneling);
            zergUnitTypesById.Add(unitTypesByName["BanelingBurrowed"].UnitId, ZergUnit.Baneling);
            zergUnitTypesById.Add(unitTypesByName["Roach"].UnitId, ZergUnit.Roach);
            zergUnitTypesById.Add(unitTypesByName["RoachBurrowed"].UnitId, ZergUnit.Roach);
            zergUnitTypesById.Add(unitTypesByName["Hydralisk"].UnitId, ZergUnit.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["HydraliskBurrowed"].UnitId, ZergUnit.Hydralisk);
            zergUnitTypesById.Add(unitTypesByName["Infestor"].UnitId, ZergUnit.Infestor);
            zergUnitTypesById.Add(unitTypesByName["InfestorBurrowed"].UnitId, ZergUnit.Infestor);
            zergUnitTypesById.Add(unitTypesByName["InfestedTerran"].UnitId, ZergUnit.InfestedTerran);
            zergUnitTypesById.Add(unitTypesByName["InfestorTerranBurrowed"].UnitId, ZergUnit.InfestedTerran);
            zergUnitTypesById.Add(unitTypesByName["SwarmHostMP"].UnitId, ZergUnit.SwarmHost);
            zergUnitTypesById.Add(unitTypesByName["SwarmHostBurrowedMP"].UnitId, ZergUnit.SwarmHost);
            zergUnitTypesById.Add(unitTypesByName["LocustMP"].UnitId, ZergUnit.Locust);
            zergUnitTypesById.Add(unitTypesByName["LocustMPFlying"].UnitId, ZergUnit.Locust);
            zergUnitTypesById.Add(unitTypesByName["Ultralisk"].UnitId, ZergUnit.Ultralisk);
            zergUnitTypesById.Add(unitTypesByName["UltraliskBurrowed"].UnitId, ZergUnit.Ultralisk);
            zergUnitTypesById.Add(unitTypesByName["Broodling"].UnitId, ZergUnit.Broodling);
            zergUnitTypesById.Add(unitTypesByName["Overlord"].UnitId, ZergUnit.Overlord);
            zergUnitTypesById.Add(unitTypesByName["Overseer"].UnitId, ZergUnit.Overseer);
            zergUnitTypesById.Add(unitTypesByName["Changeling"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingMarine"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingMarineShield"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZealot"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZergling"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["ChangelingZerglingWings"].UnitId, ZergUnit.Changeling);
            zergUnitTypesById.Add(unitTypesByName["Mutalisk"].UnitId, ZergUnit.Mutalisk);
            zergUnitTypesById.Add(unitTypesByName["Corruptor"].UnitId, ZergUnit.Corruptor);
            zergUnitTypesById.Add(unitTypesByName["BroodLord"].UnitId, ZergUnit.BroodLord);
            zergUnitTypesById.Add(unitTypesByName["Viper"].UnitId, ZergUnit.Viper);
            zergUnitTypesById.Add(unitTypesByName["NydusCanal"].UnitId, ZergUnit.NydusWorm);

            protossUnitTypesById = new Dictionary<uint, ProtossUnit>();

            protossUnitTypesById.Add(unitTypesByName["Probe"].UnitId, ProtossUnit.Probe);
            protossUnitTypesById.Add(unitTypesByName["Zealot"].UnitId, ProtossUnit.Zealot);
            protossUnitTypesById.Add(unitTypesByName["Stalker"].UnitId, ProtossUnit.Stalker);
            protossUnitTypesById.Add(unitTypesByName["Sentry"].UnitId, ProtossUnit.Sentry);
            protossUnitTypesById.Add(unitTypesByName["Adept"].UnitId, ProtossUnit.Adept);
            protossUnitTypesById.Add(unitTypesByName["HighTemplar"].UnitId, ProtossUnit.HighTemplar);
            protossUnitTypesById.Add(unitTypesByName["DarkTemplar"].UnitId, ProtossUnit.DarkTemplar);
            protossUnitTypesById.Add(unitTypesByName["Immortal"].UnitId, ProtossUnit.Immortal);
            protossUnitTypesById.Add(unitTypesByName["Colossus"].UnitId, ProtossUnit.Colossus);
            protossUnitTypesById.Add(unitTypesByName["Disruptor"].UnitId, ProtossUnit.Disruptor);
            protossUnitTypesById.Add(unitTypesByName["Archon"].UnitId, ProtossUnit.Archon);
            protossUnitTypesById.Add(unitTypesByName["Observer"].UnitId, ProtossUnit.Observer);
            protossUnitTypesById.Add(unitTypesByName["WarpPrism"].UnitId, ProtossUnit.WarpPrism);
            protossUnitTypesById.Add(unitTypesByName["Phoenix"].UnitId, ProtossUnit.Phoenix);
            protossUnitTypesById.Add(unitTypesByName["VoidRay"].UnitId, ProtossUnit.VoidRay);
            protossUnitTypesById.Add(unitTypesByName["Oracle"].UnitId, ProtossUnit.Oracle);
            protossUnitTypesById.Add(unitTypesByName["Carrier"].UnitId, ProtossUnit.Carrier);
            protossUnitTypesById.Add(unitTypesByName["Tempest"].UnitId, ProtossUnit.Tempest);
            protossUnitTypesById.Add(unitTypesByName["MothershipCore"].UnitId, ProtossUnit.MothershipCore);
            protossUnitTypesById.Add(unitTypesByName["Mothership"].UnitId, ProtossUnit.Mothership);

            terranBuildingTypesById = new Dictionary<uint, TerranBuilding>();

            terranBuildingTypesById.Add(unitTypesByName["CommandCenter"].UnitId, TerranBuilding.CommandCenter);
            terranBuildingTypesById.Add(unitTypesByName["Refinery"].UnitId, TerranBuilding.Refinery);
            terranBuildingTypesById.Add(unitTypesByName["SupplyDepot"].UnitId, TerranBuilding.SupplyDepot);
            terranBuildingTypesById.Add(unitTypesByName["Barracks"].UnitId, TerranBuilding.Barracks);
            terranBuildingTypesById.Add(unitTypesByName["EngineeringBay"].UnitId, TerranBuilding.EngineeringBay);
            terranBuildingTypesById.Add(unitTypesByName["Bunker"].UnitId, TerranBuilding.Bunker);
            terranBuildingTypesById.Add(unitTypesByName["SensorTower"].UnitId, TerranBuilding.SensorTower);
            terranBuildingTypesById.Add(unitTypesByName["MissileTurret"].UnitId, TerranBuilding.MissileTurret);
            terranBuildingTypesById.Add(unitTypesByName["Factory"].UnitId, TerranBuilding.Factory);
            terranBuildingTypesById.Add(unitTypesByName["GhostAcademy"].UnitId, TerranBuilding.GhostAcademy);
            terranBuildingTypesById.Add(unitTypesByName["Starport"].UnitId, TerranBuilding.Starport);
            terranBuildingTypesById.Add(unitTypesByName["Armory"].UnitId, TerranBuilding.Armory);
            terranBuildingTypesById.Add(unitTypesByName["FusionCore"].UnitId, TerranBuilding.FusionCore);
            terranBuildingTypesById.Add(unitTypesByName["TechLab"].UnitId, TerranBuilding.TechLab);
            terranBuildingTypesById.Add(unitTypesByName["Reactor"].UnitId, TerranBuilding.Reactor);
            terranBuildingTypesById.Add(unitTypesByName["PlanetaryFortress"].UnitId, TerranBuilding.PlanetaryFortress);
            terranBuildingTypesById.Add(unitTypesByName["OrbitalCommand"].UnitId, TerranBuilding.OrbitalCommand);

            protossBuildingTypesById = new Dictionary<uint, ProtossBuilding>();

            protossBuildingTypesById.Add(unitTypesByName["Nexus"].UnitId, ProtossBuilding.Nexus);
            protossBuildingTypesById.Add(unitTypesByName["Assimilator"].UnitId, ProtossBuilding.Assimilator);
            protossBuildingTypesById.Add(unitTypesByName["Pylon"].UnitId, ProtossBuilding.Pylon);
            protossBuildingTypesById.Add(unitTypesByName["Gateway"].UnitId, ProtossBuilding.Gateway);
            protossBuildingTypesById.Add(unitTypesByName["Forge"].UnitId, ProtossBuilding.Forge);
            protossBuildingTypesById.Add(unitTypesByName["CyberneticsCore"].UnitId, ProtossBuilding.CyberneticsCore);
            protossBuildingTypesById.Add(unitTypesByName["PhotonCannon"].UnitId, ProtossBuilding.PhotonCannon);
            protossBuildingTypesById.Add(unitTypesByName["RoboticsFacility"].UnitId, ProtossBuilding.RoboticsFacility);
            protossBuildingTypesById.Add(unitTypesByName["WarpGate"].UnitId, ProtossBuilding.WarpGate);
            protossBuildingTypesById.Add(unitTypesByName["Stargate"].UnitId, ProtossBuilding.Stargate);
            protossBuildingTypesById.Add(unitTypesByName["TwilightCouncil"].UnitId, ProtossBuilding.TwilightCouncil);
            protossBuildingTypesById.Add(unitTypesByName["RoboticsBay"].UnitId, ProtossBuilding.RoboticsBay);
            protossBuildingTypesById.Add(unitTypesByName["FleetBeacon"].UnitId, ProtossBuilding.FleetBeacon);
            protossBuildingTypesById.Add(unitTypesByName["TemplarArchive"].UnitId, ProtossBuilding.TemplarArchive);
            protossBuildingTypesById.Add(unitTypesByName["DarkShrine"].UnitId, ProtossBuilding.DarkShrine);

            zergBuildingTypesById = new Dictionary<uint, ZergBuilding>();

            zergBuildingTypesById.Add(unitTypesByName["Hatchery"].UnitId, ZergBuilding.Hatchery);
            zergBuildingTypesById.Add(unitTypesByName["Extractor"].UnitId, ZergBuilding.Extractor);
            zergBuildingTypesById.Add(unitTypesByName["SpawningPool"].UnitId, ZergBuilding.SpawningPool);
            zergBuildingTypesById.Add(unitTypesByName["EvolutionChamber"].UnitId, ZergBuilding.EvolutionChamber);
            zergBuildingTypesById.Add(unitTypesByName["RoachWarren"].UnitId, ZergBuilding.RoachWarren);
            zergBuildingTypesById.Add(unitTypesByName["BanelingNest"].UnitId, ZergBuilding.BanelingNest);
            zergBuildingTypesById.Add(unitTypesByName["SpineCrawler"].UnitId, ZergBuilding.SpineCrawler);
            zergBuildingTypesById.Add(unitTypesByName["SporeCrawler"].UnitId, ZergBuilding.SporeCrawler);
            zergBuildingTypesById.Add(unitTypesByName["Lair"].UnitId, ZergBuilding.Lair);
            zergBuildingTypesById.Add(unitTypesByName["HydraliskDen"].UnitId, ZergBuilding.HydraliskDen);
            zergBuildingTypesById.Add(unitTypesByName["LurkerDen"].UnitId, ZergBuilding.LurkerDen);
            zergBuildingTypesById.Add(unitTypesByName["InfestationPit"].UnitId, ZergBuilding.InfestationPit);
            zergBuildingTypesById.Add(unitTypesByName["Spire"].UnitId, ZergBuilding.Spire);
            zergBuildingTypesById.Add(unitTypesByName["NydusNetwork"].UnitId, ZergBuilding.NydusNetwork);
            zergBuildingTypesById.Add(unitTypesByName["Hive"].UnitId, ZergBuilding.Hive);
            zergBuildingTypesById.Add(unitTypesByName["GreaterSpire"].UnitId, ZergBuilding.GreaterSpire);
            zergBuildingTypesById.Add(unitTypesByName["UltraliskCavern"].UnitId, ZergBuilding.UltraliskCavern);
            zergBuildingTypesById.Add(unitTypesByName["CreepTumor"].UnitId, ZergBuilding.CreepTumor);
        }

        public uint GetAbilityId(TrainCommand trainCommand)
        {
            if (trainCommand.TerranUnit != TerranUnit.Unspecified)
            {
                return createTerranUnitActions[trainCommand.TerranUnit];
            }

            if (trainCommand.ProtossUnit != ProtossUnit.Unspecified)
            {
                return createProtossUnitActions[trainCommand.ProtossUnit];
            }

            if (trainCommand.ZergUnit != ZergUnit.Unspecified)
            {
                return createZergUnitActions[trainCommand.ZergUnit];
            }

            throw new ArgumentException("Received a training command with no unit specified.");
        }

        public uint GetAbilityId(BuildCommand buildCommand)
        {
            if (buildCommand.TerranBuilding != TerranBuilding.Unspecified)
            {
                return buildTerranBuildingActions[buildCommand.TerranBuilding];
            }

            if (buildCommand.ProtossBuilding != ProtossBuilding.Unspecified)
            {
                return buildProtossBuildingActions[buildCommand.ProtossBuilding];
            }

            if (buildCommand.ZergBuilding != ZergBuilding.Unspecified)
            {
                return buildZergBuildingActions[buildCommand.ZergBuilding];
            }

            throw new ArgumentException("Received a build command with no building specified.");
        }

        public uint GetHarvestAbility(Unit unit)
        {
            if (terranUnitTypesById.ContainsKey(unit.UnitType))
            {
                if (terranUnitTypesById[unit.UnitType] == TerranUnit.SCV)
                {
                    return scvHarvest;
                }

                if (terranUnitTypesById[unit.UnitType] == TerranUnit.MULE)
                {
                    return scvHarvest;
                }
            }

            if (zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == ZergUnit.Drone)
            {
                return droneHarvest;
            }

            if (protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == ProtossUnit.Probe)
            {
                return probeHarvest;
            }

            throw new ArgumentException("Attempted to get harvest ability for unit other than SCV, MULE, Probe, or Drone.", "unit");
        }

        public bool IsHarvester(Unit unit)
        {
            if (terranUnitTypesById.ContainsKey(unit.UnitType))
            {
                if (terranUnitTypesById[unit.UnitType] == TerranUnit.SCV)
                {
                    return true;
                }

                if (terranUnitTypesById[unit.UnitType] == TerranUnit.MULE)
                {
                    return true;
                }
            }

            if (zergUnitTypesById.ContainsKey(unit.UnitType) && zergUnitTypesById[unit.UnitType] == ZergUnit.Drone)
            {
                return true;
            }

            if (protossUnitTypesById.ContainsKey(unit.UnitType) && protossUnitTypesById[unit.UnitType] == ProtossUnit.Probe)
            {
                return true;
            }

            return false;
        }

        public uint Move { get; private set; }

        public uint Attack { get; private set; }

        public uint MineralDeposit { get; private set; }

        public uint VespeneGeyser { get; private set; }

        public Size2DI GetBuildingSize(BuildCommand buildCommand)
        {
            if (buildCommand.TerranBuilding != TerranBuilding.Unspecified)
            {
                return GetBuildingSize(buildCommand.TerranBuilding);
            }

            if (buildCommand.ProtossBuilding != ProtossBuilding.Unspecified)
            {
                return GetBuildingSize(buildCommand.ProtossBuilding);
            }

            if (buildCommand.ZergBuilding != ZergBuilding.Unspecified)
            {
                return GetBuildingSize(buildCommand.ZergBuilding);
            }

            throw new InvalidOperationException();
        }

        public Size2DI GetBuildingSize(TerranBuilding building)
        {
            var side = terranBuildingSizes[building];
            return new Size2DI() { X = side, Y = side };
        }

        public Size2DI GetBuildingSize(ProtossBuilding building)
        {
            var side = protossBuildingSizes[building];
            return new Size2DI() { X = side, Y = side };
        }

        public Size2DI GetBuildingSize(ZergBuilding building)
        {
            var side = zergBuildingSizes[building];
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

            // TODO: Determine if all mineral fiels are the same size
            if (mineralFieldTypes.Contains(unit.UnitType))
            {
                return new Size2DI { X = 2, Y = 1 };
            }

            throw new ArgumentException($"Unit type '{unit.UnitType}' not recognized as a structure.");
        }
    }
}
