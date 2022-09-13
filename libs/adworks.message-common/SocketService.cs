using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using adworks.media_common;
using Serilog;
using ILogger = Serilog.ILogger;

namespace adworks.message_common
{
    public class SocketService: ISocketService
    {
        private ClientWebSocket _clientWebSocket;
        private ILogger _logger;

        public SocketService(ILogger logger)
        {
            _logger = logger;
        }

        // "ws://localhost:5000/messaging"
        public async Task Connect(string url)
        {
            this._clientWebSocket = new ClientWebSocket();
            await this._clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);

            _logger.Information($"Socket client connected to {url}");
        }

        public async Task StartListening(Action<WebSocketReceiveResult> handler)
        {
            if (this._clientWebSocket == null || this._clientWebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Socket connection not initialized");
            }

            var receiving = Receiving(this._clientWebSocket, handler);

            await receiving;
        }

        public async Task Disconnect()
        {
            if (this._clientWebSocket == null || this._clientWebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Socket connection not initialized");
            }
            await this._clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "RequestedByConsumer", CancellationToken.None);
            this._clientWebSocket = null;
        }

        public async Task Send(object payload)
        {
            if (this._clientWebSocket == null || this._clientWebSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Socket connection not initialized");
            }

            await Task.Run(async () =>
            {
                var bytes = SerializeHelper.Serialize(payload);
                await this._clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text,
                    endOfMessage: true, cancellationToken: CancellationToken.None);
                await this._clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            });
        }

        private async Task Receiving(ClientWebSocket ws, Action<WebSocketReceiveResult> handler)
        {
            var buffer = new byte[2048];

            while (true)
            {
                if (ws.State != WebSocketState.Open)
                {
                    break;
                }

                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    _logger.Information(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    handler(result);
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    _logger.Information("Binary data received");
                    handler(result);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
                else
                {
                    _logger.Information(@"Unhandled message type" + result.MessageType);
                }

            }
        }

        public bool IsDisconnected()
        {
            if (this._clientWebSocket == null)
            {
                return true;
            }
            return false;
        }
    }
}
