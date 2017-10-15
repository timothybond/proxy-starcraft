using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using ProxyStarcraft;
using ProxyStarcraft.Client;
using ProxyStarcraft.Proto;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Sandbox
{
    class Program
    {
        private static readonly string BASE_DRIVE = "D:/";
        private static readonly string BASE_GAME_PATH = BASE_DRIVE + "Program Files (x86)/StarCraft II";
        private static readonly string GAME_EXECUTABLE_PATH = BASE_GAME_PATH + "/Versions/Base58400/SC2_x64.exe"; //"/Support64/SC2Switcher_x64.exe";
        private const string GAME_EXECUTABLE_ARGS_BASE = "-sso=1 -launch -uid s2_enus -listen 127.0.0.1 -displayMode 0";
        private const string GAME_EXECUTABLE_ARGS_PLAYER1 = GAME_EXECUTABLE_ARGS_BASE + " -port 5000";
        private const string GAME_EXECUTABLE_ARGS_PLAYER2 = GAME_EXECUTABLE_ARGS_BASE + " -port 5001";

        private static string MARINE_MICRO_MAP_PATH => BASE_GAME_PATH + "/maps/Example/MarineMicro.SC2Map";
        private static string EMPTY_MAP_PATH => BASE_GAME_PATH + "/maps/Test/Empty.SC2Map";

        private static string MINIGAME_BUILD_MARINES_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/BuildMarines.SC2Map";
        private static string MINIGAME_COLLECT_MINERALS_AND_GAS_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/CollectMineralsAndGas.SC2Map";
        private static string MINIGAME_COLLECT_MINERAL_SHARDS_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/CollectMineralShards.SC2Map";
        private static string MINIGAME_DEFEAT_ROACHES_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/DefeatRoaches.SC2Map";
        private static string MINIGAME_DEFEAT_ZERGLINGS_AND_BANELINGS_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/DefeatZerglingsAndBanelings.SC2Map";
        private static string MINIGAME_FIND_AND_DEFEAT_ZERGLINGS_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/FindAndDefeatZerglings.SC2Map";
        private static string MINIGAME_MOVE_TO_BEACON_MAP_PATH => BASE_GAME_PATH + "/maps/Minigames/MoveToBeacon.SC2Map";

        private static string LADDER_ABYSSAL_REEF_MAP_PATH => BASE_GAME_PATH + "/maps/Ladder/AbyssalReefLE.SC2Map";
        
        private static bool exit = false;

        static Program()
        {
            var executeInfoFile = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents", "StarCraft II", "ExecuteInfo.txt");

            if (!File.Exists(executeInfoFile))
            {
                Console.WriteLine("Could not find ExecuteInfo.txt. Using old values.");
                return;
            }

            var lines = File.ReadAllLines(executeInfoFile);
            foreach (var line in lines)
            {
                var match = Regex.Match(line, "^executable = (.+)$");
                if (match.Success)
                {
                    GAME_EXECUTABLE_PATH = match.Groups[1].Value;
                    break;
                }
            }

            BASE_GAME_PATH = Path.GetFullPath(Path.Combine(GAME_EXECUTABLE_PATH, @"..\..\..\"));
            BASE_DRIVE = Path.GetPathRoot(GAME_EXECUTABLE_PATH);
        }

        static void Main(string[] args)
        {
            //var bot = new BenchmarkBot();

            //PlayAgainstStandardAI(bot);

            var bot1 = new BenchmarkBot();
            var bot2 = new ZergRushBot();

            PlayOneOnOne(bot1, bot2);
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

        public static void PlayOneOnOne(IBot bot1, IBot bot2)
        {
            // TODO: One or more of the following:
            // 1. Get the process that this spawns, so we can terminate it on close
            // 2. Find a better way to spawn the game process directly, so we don't
            //    have to get a second process that this one spawns
            // 3. Check if there's already a waiting client so we can reuse it

            // TODO: Reduce duplication
            var info1 = new ProcessStartInfo(GAME_EXECUTABLE_PATH);
            info1.WorkingDirectory = BASE_GAME_PATH + "/Support64";
            info1.Arguments = GAME_EXECUTABLE_ARGS_PLAYER1;

            var info2 = new ProcessStartInfo(GAME_EXECUTABLE_PATH);
            info2.WorkingDirectory = BASE_GAME_PATH + "/Support64";
            info2.Arguments = GAME_EXECUTABLE_ARGS_PLAYER2;

            using (Process gameProcess1 = Process.Start(info1))
            {
                using (Process gameProcess2 = Process.Start(info2))
                {
                    using (var client1 = new SynchronousApiClient("ws://127.0.0.1:5000/sc2api"))
                    {
                        using (var client2 = new SynchronousApiClient("ws://127.0.0.1:5001/sc2api"))
                        {
                            var initiateGameSuccess = client1.InitiateGameAgainstBot(LADDER_ABYSSAL_REEF_MAP_PATH, bot1.Race, bot2.Race);

                            if (!client2.JoinGameAgainstBot(bot2.Race))
                            {
                                return;
                            }

                            if (!initiateGameSuccess.Result)
                            {
                                return;
                            }

                            var gameState1 = client1.GetGameState();
                            var gameState2 = client2.GetGameState();

                            //SaveMapData(gameState1);

                            while (true)
                            {
                                var commands1 = bot1.Act(gameState1);
                                client1.SendCommands(commands1);
                                client1.Step();
                                gameState1 = client1.GetGameState();

                                var commands2 = bot2.Act(gameState2);
                                client2.SendCommands(commands2);
                                client2.Step();
                                gameState2 = client2.GetGameState();
                            }
                        }
                    }
                }
            }
        }

        public static void PlayAgainstStandardAI(IBot bot)
        {
            // TODO: One or more of the following:
            // 1. Get the process that this spawns, so we can terminate it on close
            // 2. Find a better way to spawn the game process directly, so we don't
            //    have to get a second process that this one spawns
            // 3. Check if there's already a waiting client so we can reuse it
            var info = new ProcessStartInfo(GAME_EXECUTABLE_PATH);
            info.WorkingDirectory = BASE_GAME_PATH + "/Support64";
            info.Arguments = GAME_EXECUTABLE_ARGS_PLAYER1;

            using (Process gameProcess = Process.Start(info))
            {
                using (var client = new SynchronousApiClient("ws://127.0.0.1:5000/sc2api"))
                {
                    if (!client.InitiateGameAgainstComputer(LADDER_ABYSSAL_REEF_MAP_PATH, Race.Terran, Difficulty.MediumHard))
                    {
                        return;
                    }

                    var gameState = client.GetGameState();

                    //SaveMapData(gameState);

                    while (true)
                    {
                        var commands = bot.Act(gameState);
                        client.SendCommands(commands);
                        client.Step();
                        gameState = client.GetGameState();
                    }
                }
            }
        }

        private static void SaveMapData(GameState gameState)
        {
            using (var pathingGrid = GetImage(gameState.MapData.PathingGrid))
            {
                pathingGrid.Save(BASE_DRIVE + "Temp/pathing.bmp");
            }

            using (var placementGrid = GetImage(gameState.MapData.PlacementGrid))
            {
                placementGrid.Save(BASE_DRIVE + "Temp/placement.bmp");
            }

            using (var terrainHeight = GetImage(gameState.MapData.HeightGrid))
            {
                terrainHeight.Save(BASE_DRIVE + "Temp/terrain-height.bmp");
            }

            using (var areasBitmap = GetImage(gameState.MapData.AreaGrid))
            {
                areasBitmap.Save(BASE_DRIVE + "Temp/areas.bmp");
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
