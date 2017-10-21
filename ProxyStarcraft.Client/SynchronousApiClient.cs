using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProxyStarcraft.Proto;
using WebSocket4Net;

namespace ProxyStarcraft.Client
{
    /// <summary>
    /// Code to simplify dealing with the API by making it appear synchronous.
    /// 
    /// Probably basically harmless to use this when running the game in Single-Step mode,
    /// but maybe less so in Real-Time mode. Naturally this is not thread-safe.
    /// </summary>
    public class SynchronousApiClient : IDisposable
    {
        // TODO: Stop hardcoding port numbers
        private const int SHARED_PORT = 5500;
        private const int CLIENT_BASE_PORT = 5501;
        private const int CLIENT_GAME_PORT = 5502;
        private const int SERVER_BASE_PORT = 5503;
        private const int SERVER_GAME_PORT = 5504;

        private const int JOIN_GAME_TIMEOUT_MS = 120000;

        // Unique action IDs. May change. Should get from Abilities dictionary instead.
        private const int Move = 16;
        private const int Attack = 23;

        private object socketLock = new object();

        private WebSocket webSocket;

        private bool connected = false;
        private int connectionRetries = 4;

        private AutoResetEvent receivedEvent = new AutoResetEvent(false);

        private Response socketResponse;

        // Note: these only potentially changes at the beginning of a game, so we will avoid calling for them repeatedly.
        private ResponseGameInfo gameInfo;
        private Dictionary<uint, UnitTypeData> unitTypes;
        private Dictionary<uint, AbilityData> abilities;
        private MapData mapData;

        private Translator translator;

        private Request lastRequest;

        public SynchronousApiClient(String address)
        {
            webSocket = new WebSocket(address);

            webSocket.DataReceived += OnReceivedData;
            webSocket.MessageReceived += OnReceivedMessage;
            webSocket.Opened += OnSocketOpened;
            webSocket.Error += OnSocketError;
        }

        public GameState GetGameState()
        {
            gameInfo = gameInfo ?? Call(new Request { GameInfo = new RequestGameInfo() }).Result.GameInfo;
            unitTypes = unitTypes ?? Call(new Request { Data = new RequestData { UnitTypeId = true } }).Result.Data.Units.ToDictionary(unitType => unitType.UnitId);
            abilities = abilities ?? Call(new Request { Data = new RequestData { AbilityId = true } }).Result.Data.Abilities.ToDictionary(ability => ability.AbilityId);

            translator = translator ?? new Translator(abilities, unitTypes);

            mapData = mapData ?? new MapData(gameInfo.StartRaw);
            
            var response = Call(new Request { Observation = new RequestObservation() });

            // Sometimes this isn't giving me an Observation back (and it gives the error "Request missing command"). No idea why.
            if (response.Result.Observation == null)
            {
                int retries = 3;

                while (retries > 0 && response.Result.Observation == null)
                {
                    response = Call(new Request { Observation = new RequestObservation() });
                    retries--;
                }
            }

            var observation = response.Result.Observation;
            
            var units = observation.Observation.RawData.Units.Select(u => translator.ConvertUnit(u)).ToList();

            mapData = new MapData(mapData, units, translator, unitTypes, observation.Observation.RawData.MapState.Creep);
            
            return new GameState(gameInfo, response.Result.Observation, mapData, unitTypes, abilities, translator);
        }

        public List<uint> GetAbilities(ulong unitTag)
        {
            var queryRequest = new Request { Query = new RequestQuery { } };

            queryRequest.Query.Abilities.Add(new RequestQueryAvailableAbilities { UnitTag = unitTag });

            var response = Call(queryRequest);

            return response.Result.Query.Abilities[0].Abilities.Select(a => (uint)a.AbilityId).ToList();
        }

        public void SendCommands(IEnumerable<Command> commands)
        {
            var actionRequest = new Request { Action = new RequestAction() };

            foreach (var command in commands)
            {
                actionRequest.Action.Actions.Add(BuildAction(command));
            }

            // TODO: Check response for errors
            var actionResponse = Call(actionRequest).Result;

            if (actionResponse.Action != null && actionResponse.Action.Result.Any(result => result != ActionResult.Success))
            {
                Debugger.Break();
            }
        }

        private Proto.Action BuildAction(Command command)
        {
            ActionRawUnitCommand unitCommand;

            var abilityId = translator.GetAbilityId(command);

            switch (command)
            {
                case BuildCommand buildCommand:
                    switch (buildCommand.BuildLocation)
                    {
                        case StandardBuildLocation standardLocation:
                            var buildingSize = translator.GetBuildingSize(buildCommand);
                            var x = standardLocation.Location.X + buildingSize.X * 0.5f;
                            var y = standardLocation.Location.Y + buildingSize.Y * 0.5f;
                            unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId, TargetWorldSpacePos = new Point2D { X = x, Y = y } };
                            break;
                        case VespeneBuildLocation vespeneLocation:
                            unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId, TargetUnitTag = vespeneLocation.VespeneGeyser.Tag };
                            break;
                        case UpgradeBuildLocation upgradeLocation:
                            unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId };
                            break;
                        case AddOnBuildLocation addOnLocation:
                            unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId };
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    break;
                case LocationTargetCommand locationTargetCommand:
                    unitCommand = new ActionRawUnitCommand
                    {
                        AbilityId = (int)abilityId,
                        TargetWorldSpacePos = new Point2D { X = locationTargetCommand.X, Y = locationTargetCommand.Y }
                    };
                    break;
                case UnitTargetCommand unitTargetCommand:
                    unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId, TargetUnitTag = unitTargetCommand.Target.Tag };
                    break;
                case NoTargetCommand noTargetCommand:
                    unitCommand = new ActionRawUnitCommand { AbilityId = (int)abilityId };
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            unitCommand.UnitTags.Add(command.Unit.Tag);
            return new Proto.Action { ActionRaw = new ActionRaw { UnitCommand = unitCommand } };
        }
        
        public void Step()
        {
            Step(1);
        }

        public void Step(uint stepCount)
        {
            Call(new Request { Step = new RequestStep { Count = stepCount } }).Wait();
        }

        public void LeaveGame()
        {
            Call(new Request { LeaveGame = new RequestLeaveGame() }).Wait();
            gameInfo = null;
            unitTypes = null;
            abilities = null;
            translator = null;
        }

        public bool InitiateSinglePlayerGame(string map, Race race)
        {
            var createGameRequest = new Request
            {
                CreateGame = new RequestCreateGame
                {
                    LocalMap = new LocalMap { MapPath = map }
                }
            };

            createGameRequest.CreateGame.PlayerSetup.Add(
                new PlayerSetup
                {
                    Type = PlayerType.Participant,
                    Race = race
                });

            var createGameResponse = Call(createGameRequest);

            if (createGameResponse.Result.Status != Status.InitGame)
            {
                return false;
            }

            var joinGameResponse = Call(
                new Request
                {
                    JoinGame = new RequestJoinGame
                    {
                        Race = race,
                        Options = new InterfaceOptions { Raw = true }
                    }
                },
                JOIN_GAME_TIMEOUT_MS);

            return joinGameResponse.Result.Status == Status.InGame;
        }

        public bool InitiateGameAgainstComputer(string map, Race race, Difficulty opponentLevel)
        {
            var createGameRequest = new Request
            {
                CreateGame = new RequestCreateGame
                {
                    LocalMap = new LocalMap { MapPath = map }
                }
            };

            createGameRequest.CreateGame.PlayerSetup.Add(
                new PlayerSetup
                {
                    Type = PlayerType.Participant,
                    Race = race
                });
            createGameRequest.CreateGame.PlayerSetup.Add(
                new PlayerSetup
                {
                    Type = PlayerType.Computer,
                    Difficulty = opponentLevel,
                    Race = Race.Random
                });

            var createGameResponse = Call(createGameRequest);

            if (createGameResponse.Result.Status != Status.InitGame)
            {
                return false;
            }

            var joinGameResponse = Call(
                new Request
                {
                    JoinGame = new RequestJoinGame
                    {
                        Race = race,
                        Options = new InterfaceOptions { Raw = true }
                    }
                },
                JOIN_GAME_TIMEOUT_MS);

            return joinGameResponse.Result.Status == Status.InGame;
        }

        public async Task<bool> InitiateGameAgainstBot(string map, Race race1, Race race2)
        {
            var createGameRequest = new Request
            {
                CreateGame = new RequestCreateGame
                {
                    LocalMap = new LocalMap { MapPath = map }
                }
            };

            createGameRequest.CreateGame.PlayerSetup.Add(
                new PlayerSetup
                {
                    Type = PlayerType.Participant,
                    Race = race1
                });
            createGameRequest.CreateGame.PlayerSetup.Add(
                new PlayerSetup
                {
                    Type = PlayerType.Participant,
                    Race = race2
                });

            var createGameResponse = Call(createGameRequest);

            if (createGameResponse.Result.Status != Status.InitGame)
            {
                return false;
            }

            var joinGame = new RequestJoinGame
            {
                Race = race1,
                Options = new InterfaceOptions { Raw = true },
                SharedPort = SHARED_PORT,
                ServerPorts = new PortSet { BasePort = SERVER_BASE_PORT, GamePort = SERVER_GAME_PORT }
            };

            joinGame.ClientPorts.Add(new PortSet { BasePort = CLIENT_BASE_PORT, GamePort = CLIENT_GAME_PORT });

            var joinGameResponse = await Call(
                new Request
                {
                    JoinGame = joinGame
                },
                JOIN_GAME_TIMEOUT_MS);
            
            return joinGameResponse.Status == Status.InGame;
        }

        public bool JoinGameAgainstBot(Race race)
        {
            var joinGame = new RequestJoinGame
            {
                Race = race,
                Options = new InterfaceOptions { Raw = true },
                SharedPort = SHARED_PORT,
                ServerPorts = new PortSet { BasePort = SERVER_BASE_PORT, GamePort = SERVER_GAME_PORT }
            };

            joinGame.ClientPorts.Add(new PortSet { BasePort = CLIENT_BASE_PORT, GamePort = CLIENT_GAME_PORT });

            var joinGameResponse = Call(
                new Request
                {
                    JoinGame = joinGame
                },
                JOIN_GAME_TIMEOUT_MS);

            return joinGameResponse.Result.Status == Status.InGame;
        }

        public async Task<Response> Call(Request request, int timeoutMs = 5000)
        {
            Response retval = null;

            // TODO: Find a less dumb way of doing this. I feel like I'm mixing threading and tasks and they don't quite go together.
            await Task.Run(() =>
            {
                Connect();

                SendRequest(request);

                // TODO: Use a proper synchronization primitive for this

                int retries = 3;
                
                while (!receivedEvent.WaitOne(TimeSpan.FromMilliseconds(timeoutMs)))
                {
                    if (retries == 0)
                    {
                        throw new TimeoutException("Socket response timed out. Retries exhausted.");
                    }

                    retries -= 1;

                    // Most of our requests are actually idempotent, so on the off chance
                    // that we never get a response, it makes sense to just resend them.
                    // This might be something that should change if I can diagnose the root cause.
                    SendRequest(request);
                }

                lock (socketLock)
                {
                    retval = socketResponse;
                    socketResponse = null;
                }
            });

            return retval;
        }

        private void SendRequest(Request request)
        {
            //Console.WriteLine(request.ToString());

            using (var mem = new MemoryStream())
            {
                using (var stream = new Google.Protobuf.CodedOutputStream(mem))
                {
                    request.WriteTo(stream);
                }

                var data = mem.ToArray();

                lock (socketLock)
                {
                    lastRequest = request;
                    webSocket.Send(data, 0, data.Length);
                }
            }
        }

        private void Connect()
        {
            if (connected)
            {
                return;
            }

            webSocket.Open();

            while (!connected)
            {
                Thread.Sleep(20);
            }
        }

        private void OnReceivedData(object sender, WebSocket4Net.DataReceivedEventArgs e)
        {
            lock (socketLock)
            {
                socketResponse = Response.Parser.ParseFrom(e.Data);
                receivedEvent.Set();
            }

            //Console.WriteLine(socketResponse.ToString());
        }

        private void OnReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException("Expecting DataReceived rather than MessageReceived from WebSocket client.");
        }

        private void OnSocketOpened(object sender, EventArgs e)
        {
            connected = true;
            connectionRetries = 5;
        }

        private void OnSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            lock(socketLock)
            {
                if (connectionRetries > 0)
                {
                    if (!connected)
                    {
                        Thread.Sleep(5000);
                    }
                    
                    connectionRetries -= 1;

                    try
                    {
                        webSocket.Open();

                        // This might be less of a good idea than it was when I originally did it,
                        // since I can no longer tell if we're waiting on a response to this call.
                        if (lastRequest != null)
                        {
                            SendRequest(lastRequest);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                    return;
                }
            }
            
            throw new Exception("Unexpected socket error.", e.Exception);
        }
        
        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    webSocket.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SynchronousApiClient() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
