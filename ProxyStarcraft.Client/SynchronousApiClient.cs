using System;
using System.IO;
using System.Threading;

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
        private static object socketLock = new object();

        private WebSocket webSocket;

        private bool connected = false;
        private bool waiting = false;
        private int connectionRetries = 4;

        private Response socketResponse;

        public SynchronousApiClient(String address)
        {
            webSocket = new WebSocket(address);

            webSocket.DataReceived += OnReceivedData;
            webSocket.MessageReceived += OnReceivedMessage;
            webSocket.Opened += OnSocketOpened;
            webSocket.Error += OnSocketError;
        }

        public Observation GetRawObservation()
        {
            var response = Call(new Request { Observation = new RequestObservation() });
            return response.Observation.Observation;
        }

        public ResponseGameInfo GetGameInfo()
        {
            var response = Call(new Request { GameInfo = new RequestGameInfo() });
            return response.GameInfo;
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
        }

        private void OnSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            lock(socketLock)
            {
                if (!connected && connectionRetries > 0)
                {
                    connectionRetries -= 1;
                    Thread.Sleep(5000);
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
