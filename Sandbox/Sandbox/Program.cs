using System;
using System.IO;
using System.Threading;

using SC2APIProtocol;

using WebSocket4Net;

namespace Sandbox
{
    class Program
    {
        private static bool socketOpened = false;

        static void Main(string[] args)
        {
            using (WebSocket webSocket = new WebSocket("ws://127.0.0.1:5000/sc2api"))
            {
                webSocket.DataReceived += HandleReceivedData;
                webSocket.MessageReceived += HandleReceivedMessage;
                webSocket.Opened += WebSocket_Opened;
                webSocket.Error += WebSocket_Error;
                webSocket.Closed += WebSocket_Closed;

                webSocket.Open();

                while (!socketOpened)
                {
                    Thread.Sleep(50);
                }

                using (var mem = new MemoryStream())
                {
                    using (var stream = new Google.Protobuf.CodedOutputStream(mem))
                    {
                        var request = new Request() { Ping = new RequestPing() };
                        request.WriteTo(stream);
                    }

                    var data = mem.ToArray();

                    webSocket.Send(data, 0, data.Length);
                }

                while (true)
                {
                    Thread.Sleep(200);
                }
            }
        }

        private static void WebSocket_Closed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        }

        private static void WebSocket_Opened(object sender, EventArgs e)
        {
            socketOpened = true;
        }

        private static void HandleReceivedData(object sender, DataReceivedEventArgs e)
        {
            var response = Response.Parser.ParseFrom(e.Data);
            Console.WriteLine(response);
        }

        private static void HandleReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            var response = Response.Parser.ParseFrom(System.Text.Encoding.Default.GetBytes(e.Message));

            Console.WriteLine(response);
        }

        //private static void HandleWebSocketMessage(object sender, MessageEventArgs e)
        //{
        //    var response = Response.Parser.ParseFrom(e.RawData);

        //    Console.WriteLine(response);
        //}
    }
}
