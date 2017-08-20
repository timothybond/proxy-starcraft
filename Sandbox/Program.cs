using System.Diagnostics;
using System.Linq;

using ProxyStarcraft.Client;
using ProxyStarcraft.Proto;
using System.Collections.Generic;

namespace Sandbox
{
    class Program
    {
        private const string GAME_EXECUTABLE_PATH = "D:/Program Files (x86)/StarCraft II/Support64/SC2Switcher_x64.exe";
        private const string GAME_EXECUTABLE_ARGS = "-sso=1 -launch -uid s2_enus -listen 127.0.0.1 -port 5000";

        private const string MARINE_MICRO_MAP_PATH = "D:/Program Files (x86)/StarCraft II/maps/Example/MarineMicro.SC2Map";
        
        private static bool exit = false;

        static void Main(string[] args)
        {
            using (Process gameProcess = Process.Start(GAME_EXECUTABLE_PATH, GAME_EXECUTABLE_ARGS))
            {
                using (var client = new SynchronousApiClient("ws://127.0.0.1:5000/sc2api"))
                {
                    if (!client.InitiateSinglePlayerGame(MARINE_MICRO_MAP_PATH, Race.Terran))
                    {
                        return;
                    }

                    var gameState = client.GetGameState();

                    var unitTypes = gameState.Observation.RawData.Units.Select(unit => unit.UnitType).Distinct();
                    
                    var unitTypeDataResponse = client.Call(new Request { Data = new RequestData { UnitTypeId = true } });

                    

                    var abilityDataResponse = client.Call(new Request { Data = new RequestData { AbilityId = true } });

                    var buffDataResponse = client.Call(new Request { Data = new RequestData { BuffId = true } });

                    var upgradeDataResponse = client.Call(new Request { Data = new RequestData { UpgradeId = true } });

                    while (true)
                    {
                        if (exit)
                        {
                            client.LeaveGame();
                            break;
                        }
                        else
                        {
                            client.Step();
                            gameState = client.GetGameState();
                        }

                        if (gameState.Observation.RawData.Units.All(unit => unit.Alliance == Alliance.Enemy))
                        {
                            exit = true;
                        }
                    }
                }
            }
        }
    }
}
