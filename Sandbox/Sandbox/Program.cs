using System.Linq;
using System.Threading;

using SC2APIProtocol;

namespace Sandbox
{
    class Program
    {
        private const string MARINE_MICRO_MAP_PATH = "D:/Program Files (x86)/StarCraft II/maps/Example/MarineMicro.SC2Map";
        
        private static bool exit = false;

        static void Main(string[] args)
        {
            using (var client = new SynchronousApiClient("ws://127.0.0.1:5000/sc2api"))
            {
                var pingResponse = client.Call(new Request { Ping = new RequestPing() });

                var availableMapsResponse = client.Call(new Request { AvailableMaps = new RequestAvailableMaps() });

                var createGameRequest = new Request
                {
                    CreateGame = new RequestCreateGame
                    {
                        LocalMap = new LocalMap { MapPath = MARINE_MICRO_MAP_PATH }
                    }
                };

                createGameRequest.CreateGame.PlayerSetup.Add(new PlayerSetup
                {
                    Type = PlayerType.Participant,
                    Race = Race.Terran
                });
                
                var createGameResponse = client.Call(createGameRequest);

                var joinGameResponse = client.Call(
                    new Request
                    {
                        JoinGame = new RequestJoinGame
                        {
                            Race = Race.Terran,
                            Options = new InterfaceOptions {  Raw = true }
                        }
                    });

                var observationResponse = client.Call(new Request { Observation = new RequestObservation() });

                while (observationResponse.Status != Status.Ended)
                {
                    if (exit)
                    {
                        client.Call(new Request { LeaveGame = new RequestLeaveGame() });
                        break;
                    }
                    else
                    {
                        client.Call(new Request { Step = new RequestStep { Count = 1 } });
                        observationResponse = client.Call(new Request { Observation = new RequestObservation() });
                    }

                    if (observationResponse.Observation.Observation.RawData.Units.All(unit => unit.Alliance == Alliance.Enemy))
                    {
                        exit = true;
                    }
                }

                while (!exit)
                {
                    Thread.Sleep(200);
                }
            }
        }
    }
}
