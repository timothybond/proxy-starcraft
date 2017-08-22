using System;
using System.Diagnostics;
using System.Linq;

using ProxyStarcraft;
using ProxyStarcraft.Client;
using ProxyStarcraft.Proto;
using System.Collections.Generic;

namespace Sandbox
{
    class Program
    {
        // TODO: Fix hardcoded path
        private const string BASE_GAME_PATH = "D:/Program Files (x86)/StarCraft II";

        private const string GAME_EXECUTABLE_PATH = BASE_GAME_PATH + "/Support64/SC2Switcher_x64.exe";
        private const string GAME_EXECUTABLE_ARGS = "-sso=1 -launch -uid s2_enus -listen 127.0.0.1 -port 5000";

        private const string MARINE_MICRO_MAP_PATH = BASE_GAME_PATH + "/maps/Example/MarineMicro.SC2Map";
        private const string EMPTY_MAP_PATH = BASE_GAME_PATH + "/maps/Test/Empty.SC2Map";

        private const string MINIGAME_BUILD_MARINES_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/BuildMarines.SC2Map";
        private const string MINIGAME_COLLECT_MINERALS_AND_GAS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/CollectMineralsAndGas.SC2Map";
        private const string MINIGAME_COLLECT_MINERAL_SHARDS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/CollectMineralShards.SC2Map";
        private const string MINIGAME_DEFEAT_ROACHES_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/DefeatRoaches.SC2Map";
        private const string MINIGAME_DEFEAT_ZERGLINGS_AND_BANELINGS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/DefeatZerglingsAndBanelings.SC2Map";
        private const string MINIGAME_FIND_AND_DEFEAT_ZERGLINGS_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/FindAndDefeatZerglings.SC2Map";
        private const string MINIGAME_MOVE_TO_BEACON_MAP_PATH = BASE_GAME_PATH + "/maps/Minigames/MoveToBeacon.SC2Map";
        
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
                    RunGatherMinigame(client);
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
                if (gameState.Units.All(unit => unit.Alliance == Alliance.Enemy))
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

            client.SendCommands(new[] { new BuildCommand(gameState.Units[0], TerranBuilding.SupplyDepot, 15, 15) });

            while (true)
            {
                client.Step();
                gameState = client.GetGameState();
            }
        }

        public static void RunGatherMinigame(SynchronousApiClient client)
        {
            if (!client.InitiateSinglePlayerGame(MINIGAME_COLLECT_MINERALS_AND_GAS_MAP_PATH, Race.Terran))
            {
                return;
            }

            var gameState = client.GetGameState();

            StartHarvesting(client, gameState);

            while (true)
            {
                client.Step();
                gameState = client.GetGameState();
            }
        }

        public static void StartHarvesting(SynchronousApiClient client, GameState gameState)
        {
            var harvesters = new List<Unit>();
            var minerals = new List<Unit>();

            foreach (var unit in gameState.Units)
            {
                if (unit.Alliance == Alliance.Neutral && unit.MineralContents > 0)
                {
                    minerals.Add(unit);
                }

                if (gameState.Translator.IsHarvester(unit))
                {
                    harvesters.Add(unit);
                }
            }

            var commands = new List<ICommand>();

            foreach (var harvester in harvesters)
            {
                commands.Add(new HarvestCommand(harvester, harvester.GetClosest(minerals)));
            }

            client.SendCommands(commands);
        }
    }
}
