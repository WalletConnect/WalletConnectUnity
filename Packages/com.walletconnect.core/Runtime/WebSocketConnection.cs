using System;
using System.Text;
using System.Threading.Tasks;
using JamesFrowen.SimpleWeb;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Common;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

namespace WalletConnectUnity.Core
{
    public sealed class WebSocketConnection : IModule, IJsonRpcConnection
    {
        public string Name { get; } = "WebSocketConnection";
        public string Context { get; }

        public bool Connected => _client is { ConnectionState: ClientState.Connected };
        public bool Connecting => _client is { ConnectionState: ClientState.Connecting };
        public string Url { get; private set; }
        public bool IsPaused { get; }

        private SimpleWebClient _client;
        private bool _disposed;

        public event EventHandler<string> PayloadReceived;
        public event EventHandler Closed;
        public event EventHandler<Exception> ErrorReceived;
        public event EventHandler<object> Opened;
        public event EventHandler<Exception> RegisterErrored;

        public WebSocketConnection(string url)
        {
            Context = Guid.NewGuid().ToString();
            Url = url;
        }

        public Task Open()
        {
            Register();

            return Task.CompletedTask;
        }

        public Task Open<T>(T options)
        {
            if (typeof(string).IsAssignableFrom(typeof(T)))
            {
                var newUrl = options as string;

                if (!Validation.IsWsUrl(newUrl))
                    throw new ArgumentException(
                        $"[WebSocketConnection] Provided URL is not compatible with WebSocket connection: {newUrl}");

                Url = newUrl;
            }

            Register();
            return Task.CompletedTask;
        }

        private void Register()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WebSocketConnection));
            
            Log.level = Log.Levels.info;

            var tcpConfig = new TcpConfig(false, 5000, 0);
            _client = SimpleWebClient.Create(ushort.MaxValue, 5000, tcpConfig);

            _client.onConnect += () =>
            {
                Opened?.Invoke(this, EventArgs.Empty);
            };
            _client.onData += (data) =>
            {
                var json = Encoding.UTF8.GetString(data);
                PayloadReceived?.Invoke(this, json);
            };
            _client.onError += (e) => { ErrorReceived?.Invoke(this, e); };
            _client.onDisconnect += () =>
            {
                Closed?.Invoke(this, EventArgs.Empty);
            };

            _client.Connect(new Uri(Url));

            UnityEventsDispatcher.Instance.Tick += TickHandler;
        }

        private void TickHandler()
        {
            _client.ProcessMessageQueue();
        }

        private void OnError(string message)
        {
            ErrorReceived?.Invoke(this, new WebSocketException(message));

            WCLogger.LogError(Connecting
                ? $"[{Name}-{Context}] Error happened during connection. Error message: {message}"
                : $"[{Name}-{Context}] Error: {message}");
        }

        private void OnError<T>(IJsonRpcPayload ogPayload, Exception e)
        {
            if (ogPayload != null)
            {
                var payload = new JsonRpcResponse<T>(ogPayload.Id, new Error
                {
                    Code = e.HResult,
                    Data = null,
                    Message = e.Message
                }, default);

                var json = JsonConvert.SerializeObject(payload);

                // Trigger the payload event, converting the new JsonRpcResponse object to JSON string
                PayloadReceived?.Invoke(this, json);
            }

            WCLogger.LogError(e);
        }

        public Task Close()
        {
            _client.Disconnect();
            return Task.CompletedTask;
        }

        public Task SendRequest<T>(IJsonRpcRequest<T> requestPayload, object context)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(requestPayload);
                var message = Encoding.UTF8.GetBytes(payload);
                _client.Send(new ArraySegment<byte>(message));
            }
            catch (Exception e)
            {
                OnError<T>(requestPayload, e);
            }

            return Task.CompletedTask;
        }

        public Task SendResult<T>(IJsonRpcResult<T> responsePayload, object context)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(responsePayload);
                var message = Encoding.UTF8.GetBytes(payload);
                _client.Send(new ArraySegment<byte>(message));
            }
            catch (Exception e)
            {
                OnError<T>(responsePayload, e);
            }

            return Task.CompletedTask;
        }

        public Task SendError(IJsonRpcError errorPayload, object context)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(errorPayload);
                var message = Encoding.UTF8.GetBytes(payload);
                _client.Send(new ArraySegment<byte>(message));
            }
            catch (Exception e)
            {
                OnError<object>(null, e);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _client.Disconnect();
                UnityEventsDispatcher.Instance.Tick -= TickHandler;
            }

            _disposed = true;
        }
    }
}