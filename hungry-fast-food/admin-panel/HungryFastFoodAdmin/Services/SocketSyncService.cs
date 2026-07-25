// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Services\SocketSyncService.cs

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;

namespace HungryFastFoodAdmin.Services
{
    public class SocketSyncService : IDisposable
    {
        private readonly SocketIO _socket;
        private readonly string _socketUrl;
        private bool _isDisposed;

        public event Action<bool> ConnectionChanged;
        public event Action<string, string> SocketEventReceived;

        public SocketSyncService(string socketUrl = null)
        {
            _socketUrl = !string.IsNullOrEmpty(socketUrl)
                ? socketUrl
                : ConfigManager.GetAppSetting("SocketServerUrl", "http://localhost:5001");

            _socket = new SocketIO(new Uri(_socketUrl), new SocketIOOptions
            {
                Reconnection = true,
                ReconnectionAttempts = 5,
                ReconnectionDelayMax = 1000,
            });

            _socket.OnConnected += (sender, args) =>
            {
                ConnectionChanged?.Invoke(true);
            };

            _socket.OnDisconnected += (sender, reason) =>
            {
                ConnectionChanged?.Invoke(false);
            };

            RegisterEvents();
        }

        public async Task StartAsync()
        {
            if (_isDisposed) return;

            try
            {
                if (!_socket.Connected)
                {
                    await _socket.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("SocketSyncService failed to connect", ex);
                ConnectionChanged?.Invoke(false);
            }
        }

        private void RegisterEvents()
        {
            _socket.On("order_placed", async response =>
            {
                string payload = response.GetValue<string>(0);
                HandleSocketEvent("order_placed", payload);
                await Task.CompletedTask;
            });

            _socket.On("order_status_updated", async response =>
            {
                string payload = response.GetValue<string>(0);
                HandleSocketEvent("order_status_updated", payload);
                await Task.CompletedTask;
            });

            _socket.On("category_added", async response =>
            {
                string payload = response.GetValue<string>(0);
                HandleSocketEvent("category_added", payload);
                await Task.CompletedTask;
            });

            _socket.On("product_added", async response =>
            {
                string payload = response.GetValue<string>(0);
                HandleSocketEvent("product_added", payload);
                await Task.CompletedTask;
            });
        }

        private void HandleSocketEvent(string eventName, string payload)
        {
            try
            {
                SocketEventReceived?.Invoke(eventName, payload);
            }
            catch (Exception ex)
            {
                Logger.LogError($"SocketSyncService event handler failed for {eventName}", ex);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            try
            {
                _socket?.DisconnectAsync().Wait(1000);
                _socket?.Dispose();
            }
            catch { }
        }
    }
}
