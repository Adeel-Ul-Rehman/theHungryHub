// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\SocketSyncService.cs
//
// Real-time order listener using Pusher Channels WebSocket API.
// Uses only System.Net.WebSockets.ClientWebSocket — zero external NuGet dependencies.
// Connects to Pusher, subscribes to "orders-channel", and fires SocketEventReceived
// whenever a "new-order" event arrives so the POS can pull the order immediately.

using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HungryFastFoodAdmin.Services
{
    public class SocketSyncService : IDisposable
    {
        // ── Pusher credentials (match backend .env) ────────────────────────────
        private const string PusherKey     = "af84d71102360c3b9bbf";
        private const string PusherCluster = "ap2";
        private const string OrderChannel  = "orders-channel";
        private const string NewOrderEvent = "new-order";

        private readonly string _pusherUrl =
            $"wss://ws-{PusherCluster}.pusher.com/app/{PusherKey}" +
            "?client=dotnet-pos&version=1.0.0&protocol=7";

        private ClientWebSocket _ws;
        private CancellationTokenSource _cts;
        private bool _isDisposed;
        private bool _isRunning;

        // ── Public events ──────────────────────────────────────────────────────
        public event Action<bool>           ConnectionChanged;
        public event Action<string, string> SocketEventReceived;

        // ── Start / connect ────────────────────────────────────────────────────
        public async Task StartAsync()
        {
            if (_isDisposed || _isRunning) return;
            _isRunning = true;

            // Run the connection loop on a background thread
            _ = Task.Run(RunLoopAsync);
            await Task.CompletedTask;
        }

        private async Task RunLoopAsync()
        {
            while (!_isDisposed)
            {
                _cts = new CancellationTokenSource();
                try
                {
                    await ConnectAndListenAsync(_cts.Token);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Pusher WebSocket disconnected, will reconnect in 5 s", ex);
                }

                ConnectionChanged?.Invoke(false);

                if (!_isDisposed)
                    await Task.Delay(5000); // back-off before reconnect
            }
        }

        private async Task ConnectAndListenAsync(CancellationToken ct)
        {
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

            Logger.Log("Pusher: connecting to " + _pusherUrl);
            await _ws.ConnectAsync(new Uri(_pusherUrl), ct);
            Logger.Log("Pusher: WebSocket connected");

            // ── Receive loop ───────────────────────────────────────────────────
            var buffer = new byte[16 * 1024];
            while (_ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logger.Log("Pusher: server closed connection");
                        return;
                    }
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                await HandleMessageAsync(sb.ToString(), ct);
            }
        }

        // ── Message handler ────────────────────────────────────────────────────
        private async Task HandleMessageAsync(string raw, CancellationToken ct)
        {
            try
            {
                using var doc = JsonDocument.Parse(raw);
                var root = doc.RootElement;

                if (!root.TryGetProperty("event", out var evtProp)) return;
                var eventName = evtProp.GetString();

                switch (eventName)
                {
                    // ── Pusher handshake ─────────────────────────────────────
                    case "pusher:connection_established":
                        Logger.Log("Pusher: connection established — subscribing to " + OrderChannel);
                        await SubscribeAsync(ct);
                        break;

                    // ── Subscription confirmed ───────────────────────────────
                    case "pusher_internal:subscription_succeeded":
                        Logger.Log("Pusher: subscribed to " + OrderChannel);
                        ConnectionChanged?.Invoke(true);
                        break;

                    // ── Pusher ping keepalive ────────────────────────────────
                    case "pusher:ping":
                        await SendJsonAsync(new { @event = "pusher:pong", data = new { } }, ct);
                        break;

                    // ── New order from the website ───────────────────────────
                    case NewOrderEvent:
                        Logger.Log("Pusher: new-order event received");
                        var data = root.TryGetProperty("data", out var dataProp)
                            ? dataProp.GetString() ?? dataProp.GetRawText()
                            : "{}";
                        SocketEventReceived?.Invoke("order_placed", data);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Pusher: failed to parse message: " + raw, ex);
            }
        }

        // ── Subscribe to Pusher channel ────────────────────────────────────────
        private Task SubscribeAsync(CancellationToken ct) =>
            SendJsonAsync(new
            {
                @event = "pusher:subscribe",
                data   = new { channel = OrderChannel }
            }, ct);

        // ── Generic JSON sender ────────────────────────────────────────────────
        private async Task SendJsonAsync(object payload, CancellationToken ct)
        {
            if (_ws?.State != WebSocketState.Open) return;
            var json  = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: ct
            );
        }

        // ── Dispose ────────────────────────────────────────────────────────────
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _isRunning  = false;

            try
            {
                _cts?.Cancel();
                _ws?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None)
                   .Wait(2000);
                _ws?.Dispose();
                _cts?.Dispose();
            }
            catch { /* swallow on shutdown */ }
        }
    }
}
