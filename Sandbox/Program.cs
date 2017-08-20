using System.Diagnostics;
using System.Linq;

using ProxyStarcraft.Client;
using SC2APIProtocol;

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

                    var observation = client.GetRawObservation();
                    var gameInfo = client.GetGameInfo();

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
                            observation = client.GetRawObservation();
                            gameInfo = client.GetGameInfo();
                        }

                        if (observation.RawData.Units.All(unit => unit.Alliance == Alliance.Enemy))
                        {
                            exit = true;
                        }
                    }
                }
            }
        }
    }
}
