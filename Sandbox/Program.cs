using System;
using System.Diagnostics;
using System.Linq;

using ProxyStarcraft;
using ProxyStarcraft.Client;
using ProxyStarcraft.Proto;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using ProxyStarcraft.Map;

namespace Sandbox
{
    class Program
    {
        // TODO: Fix hardcoded path
        private const string BASE_GAME_PATH = "D:/Program Files (x86)/StarCraft II";

        private const string GAME_EXECUTABLE_PATH = BASE_GAME_PATH + "/Support64/SC2Switcher_x64.exe";
        private const string GAME_EXECUTABLE_ARGS = "-sso=1 -launch -uid s2_enus -listen 127.0.0.1 -port 5000 -win";

        private const string MARINE_MICRO_MAP_PATH = BASE_GAME_PATH + "/maps/Example/MarineMicro.SC2Map";
        private const string EMPTY_MAP_PATH = BASE_GAME_PATH + "/maps/Test/Empty.SC2Map";

        private const string MINIGAME_BUILD_MARINES_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/BuildMarines.SC2Map";
        private const string MINIGAME_COLLECT_MINERALS_AND_GAS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/CollectMineralsAndGas.SC2Map";
        private const string MINIGAME_COLLECT_MINERAL_SHARDS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/CollectMineralShards.SC2Map";
        private const string MINIGAME_DEFEAT_ROACHES_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/DefeatRoaches.SC2Map";
        private const string MINIGAME_DEFEAT_ZERGLINGS_AND_BANELINGS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/DefeatZerglingsAndBanelings.SC2Map";
        private const string MINIGAME_FIND_AND_DEFEAT_ZERGLINGS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/FindAndDefeatZerglings.SC2Map";
        private const string MINIGAME_MOVE_TO_BEACON_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/MoveToBeacon.SC2Map";

        private const string LADDER_ABYSSAL_REEF_MAP_PATH = BASE_GAME_PATH + "/maps/Ladder/AbyssalReefLE.SC2Map";
        
        private static bool exit = false;

        static void Main(string[] args)
        {
            // TODO: One or more of the following:
            // 1. Get the process that this spawns, so we can terminate it on close
            // 2. Find a better way to spawn the game process directly, so we don't
            //    have to get a second process that this one spawns
            // 3. Check if there's already a waiting client so we can reuse it
            using (Process gameProcess = Process.Start(GAME_EXECUTABLE_PATH, GAME_EXECUTABLE_ARGS))
            {
                using (var client = new SynchronousApiClient("ws://127.0.0.1:5000/sc2api"))
                {
                    //RunMarineMicroGame(client);
                    //RunEmptyMapGame(client);
                    PlayAgainstStandardAI(client);
                }
            }
        }
        
        public static void RunMarineMicroGame(SynchronousApiClient client)
        {
            if (!client.InitiateSinglePlayerGame(MARINE_MICRO_MAP_PATH, Race.Terran))
            {
                return;
            }

            var gameState = client.GetGameState();

            IBot bot = new SimpleMarineBot();

            while (true)
            {
                if (exit)
                {
                    client.LeaveGame();
                    break;
                }
                else
                {
                    var commands = bot.Act(gameState);
                    client.SendCommands(commands);
                    client.Step();
                    gameState = client.GetGameState();
                }

                // Exit once all of our units are dead - probably not going to be what we do in the long run,
                // but it works for the example map we're currently on.
                if (gameState.RawUnits.All(unit => unit.Alliance == Alliance.Enemy))
                {
                    exit = true;
                }
            }
        }

        public static void RunEmptyMapGame(SynchronousApiClient client)
        {
            if (!client.InitiateSinglePlayerGame(EMPTY_MAP_PATH, Race.Terran))
            {
                return;
            }

            var gameState = client.GetGameState();

            var playerId = gameState.GameInfo.PlayerInfo[0].PlayerId;

            var debugRequest = new Request { Debug = new RequestDebug() };

            debugRequest.Debug.Debug.Add(new DebugCommand { GameState = DebugGameState.Minerals });
            debugRequest.Debug.Debug.Add(
                new DebugCommand
                {
                    CreateUnit = new DebugCreateUnit
                    {
                        Owner = (int)playerId,
                        Pos = new Point2D { X = 15f, Y = 15f },
                        Quantity = 1,
                        UnitType = gameState.UnitTypes.Values.Single(unit => string.Equals(unit.Name, "SCV")).UnitId
                    }
                });

            var debugResponse = client.Call(debugRequest);

            for (var i = 0; i < 20; i++)
            {
                client.Step();
            }

            gameState = client.GetGameState();

            // Broke this, probably not much point in fixing it
            //client.SendCommands(new[] { new BuildCommand(gameState.RawUnits[0], TerranBuildingType.SupplyDepot, 15, 15) });

            while (true)
            {
                client.Step();
                gameState = client.GetGameState();
            }
        }

        public static void PlayAgainstStandardAI(SynchronousApiClient client)
        {
            if (!client.InitiateGameAgainstComputer(LADDER_ABYSSAL_REEF_MAP_PATH, Race.Terran, Difficulty.MediumHard))
            {
                return;
            }
            
            var bot = new BenchmarkBot();
            var gameState = client.GetGameState();

            using (var pathingGrid = GetImage(gameState.MapData.PathingGrid))
            {
                pathingGrid.Save("D:/Temp/pathing.bmp");
            }

            using (var placementGrid = GetImage(gameState.MapData.PlacementGrid))
            {
                placementGrid.Save("D:/Temp/placement.bmp");
            }

            using (var terrainHeight = GetImage(gameState.MapData.HeightGrid))
            {
                terrainHeight.Save("D:/Temp/terrain-height.bmp");
            }
            
            using (var areasBitmap = GetImage(gameState.MapData.AreaGrid))
            {
                areasBitmap.Save("D:/Temp/areas.bmp");
            }
            
            while (true)
            {
                var commands = bot.Act(gameState);
                client.SendCommands(commands);
                client.Step();
                gameState = client.GetGameState();
            }
        }

        private static Bitmap GetImage(MapArray<byte> data)
        {
            return GetImage(data.Data, data.Size);
        }

        private static Bitmap GetImage(byte[] data, Size2DI mapSize)
        {
            return new Bitmap(
                mapSize.X,
                mapSize.Y,
                mapSize.X,
                PixelFormat.Format8bppIndexed,
                Marshal.UnsafeAddrOfPinnedArrayElement(data, 0));
        }

        public static ProxyStarcraft.Unit GetBuilder(BuildingOrUnitType buildingOrUnit, GameState gameState, SynchronousApiClient client)
        {
            // TODO: Exclude units/structures already building things
            var buildAction = gameState.Translator.GetBuildAction(buildingOrUnit);

            foreach (var unit in gameState.Units)
            {
                var abilities = client.GetAbilities(unit.Tag);

                if (abilities.Contains(buildAction))
                {
                    return unit;
                }
            }

            return null;
        }
        
        public static void StartHarvesting(SynchronousApiClient client, GameState gameState)
        {
            var harvesters = gameState.Units.Where(unit => unit is TerranUnit terranUnit && terranUnit.TerranUnitType == TerranUnitType.SCV).ToList();
            var minerals = gameState.NeutralUnits.Where(unit => unit.IsMineralDeposit);
            
            var commands = new List<Command>();

            foreach (var harvester in harvesters)
            {
                commands.Add(harvester.Harvest(harvester.GetClosest(minerals)));
            }

            client.SendCommands(commands);
        }
    }
}
