using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using ProxyStarcraft.Proto;
using WebSocket4Net;
using System.Linq;

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
        // Unique action IDs. May change. Should get from Abilities dictionary instead.
        private const int Move = 16;
        private const int Attack = 23;

        private static object socketLock = new object();

        private WebSocket webSocket;

        private bool connected = false;
        private bool waiting = false;
        private int connectionRetries = 4;

        private Response socketResponse;

        // Note: these only potentially changes at the beginning of a game, so we will avoid calling for them repeatedly.
        private ResponseGameInfo gameInfo;
        private Dictionary<uint, UnitTypeData> unitTypes;
        private Dictionary<uint, AbilityData> abilities;

        private Translator translator;

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
            gameInfo = gameInfo ?? Call(new Request { GameInfo = new RequestGameInfo() }).GameInfo;
            unitTypes = unitTypes ?? Call(new Request { Data = new RequestData { UnitTypeId = true } }).Data.Units.ToDictionary(unitType => unitType.UnitId);
            abilities = abilities ?? Call(new Request { Data = new RequestData { AbilityId = true } }).Data.Abilities.ToDictionary(ability => ability.AbilityId);

            var response = Call(new Request { Observation = new RequestObservation() });
            var gameState = new GameState(gameInfo, response.Observation.Observation, unitTypes, abilities);

            translator = translator ?? new Translator(gameState);

            return gameState;
        }

        public List<AvailableAbility> GetAbilities(ulong unitTag)
        {
            var queryRequest = new Request { Query = new RequestQuery { } };

            queryRequest.Query.Abilities.Add(new RequestQueryAvailableAbilities { UnitTag = unitTag });

            var response = Call(queryRequest);

            return response.Query.Abilities[0].Abilities.ToList();
        }

        public void SendOrders(IEnumerable<IOrder> orders)
        {
            var actionRequest = new Request { Action = new RequestAction() };

            foreach (var order in orders)
            {
                actionRequest.Action.Actions.Add(buildAction(order));
            }

            // TODO: Check response for errors
            var actionResponse = Call(actionRequest);
        }

        private Proto.Action buildAction(IOrder order)
        {
            ActionRawUnitCommand unitCommand;

            switch (order)
            {
                case MoveOrder moveOrder:
                    unitCommand = new ActionRawUnitCommand { AbilityId = Move, TargetWorldSpacePos = new Point2D { X = moveOrder.X, Y = moveOrder.Y } };
                    break;
                case AttackOrder attackOrder:
                    unitCommand = new ActionRawUnitCommand { AbilityId = Attack, TargetUnitTag = attackOrder.Target.Tag };
                    break;
                case BuildOrder buildOrder:
                    var buildAbilityId = translator.GetAbilityId(buildOrder);
                    unitCommand = new ActionRawUnitCommand { AbilityId = (int)buildAbilityId, TargetWorldSpacePos = new Point2D { X = buildOrder.X, Y = buildOrder.Y } };
                    break;
                case TrainOrder trainOrder:
                    var trainAbilityId = translator.GetAbilityId(trainOrder);
                    unitCommand = new ActionRawUnitCommand { AbilityId = (int)trainAbilityId };
                    break;
                case HarvestOrder harvestOrder:
                    var harvestAbilityId = translator.GetHarvestAbility(harvestOrder.Unit);
                    unitCommand = new ActionRawUnitCommand { AbilityId = (int)harvestAbilityId, TargetUnitTag = harvestOrder.Target.Tag };
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            unitCommand.UnitTags.Add(order.Unit.Tag);
            return new Proto.Action { ActionRaw = new ActionRaw { UnitCommand = unitCommand } };
        }
        
        public void Step()
        {
            Step(1);
        }

        public void Step(uint stepCount)
        {
            Call(new Request { Step = new RequestStep { Count = stepCount } });
        }

        public void LeaveGame()
        {
            Call(new Request { LeaveGame = new RequestLeaveGame() });
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

            if (createGameResponse.Status != Status.InitGame)
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
                });

            return joinGameResponse.Status == Status.InGame;
        }

        public Response Call(Request request)
        {
            Connect();

            using (var mem = new MemoryStream())
            {
                using (var stream = new Google.Protobuf.CodedOutputStream(mem))
                {
                    request.WriteTo(stream);
                }

                var data = mem.ToArray();

                lock (socketLock)
                {
                    webSocket.Send(data, 0, data.Length);
                    waiting = true;
                }
            }

            // TODO: Use a proper synchronization primitive for this
            while (waiting)
            {
                Thread.Sleep(5);
            }

            lock(socketLock)
            {
                var retval = socketResponse;
                socketResponse = null;
                return retval;
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

        private void OnReceivedData(object sender, DataReceivedEventArgs e)
        {
            lock (socketLock)
            {
                socketResponse = Response.Parser.ParseFrom(e.Data);
                waiting = false;
            }
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
                    webSocket.Open();
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
