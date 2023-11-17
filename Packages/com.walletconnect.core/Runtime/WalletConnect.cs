using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Methods;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;

namespace WalletConnectUnity.Core
{
    public class WalletConnect : IWalletConnect
    {
        private static readonly Lazy<IWalletConnect> LazyInstance = new(() => new WalletConnect());
        public static IWalletConnect Instance { get; } = LazyInstance.Value;

        public ISignClient SignClient { get; private set; }

        public SessionStruct ActiveSession => SignClient.AddressProvider.DefaultSession;

        public bool IsInitialized { get; private set; }

        public bool IsConnected => !string.IsNullOrWhiteSpace(ActiveSession.Topic);

        private readonly SemaphoreSlim _initializationSemaphore = new(1, 1);

        public async Task<IWalletConnect> InitializeAsync()
        {
            try
            {
                await _initializationSemaphore.WaitAsync();

                if (IsInitialized)
                {
                    Debug.LogError("[WalletConnectUnity] Already initialized");
                    return this;
                }

                var projectConfig = ProjectConfiguration.Load();

                Assert.IsNotNull(projectConfig,
                    $"Project configuration not found. Expected to find it at <i>{ProjectConfiguration.ConfigPath}</i>");
                Assert.IsFalse(string.IsNullOrWhiteSpace(projectConfig.Id),
                    $"Project ID is not set in the project configuration asset ( <i>{ProjectConfiguration.ConfigPath}</i> ).");
                Assert.IsFalse(projectConfig.Metadata == null || string.IsNullOrWhiteSpace(projectConfig.Metadata.Name),
                    $"Project name is not set in the project configuration asset ( <i>{ProjectConfiguration.ConfigPath}</i> ).");

                if (projectConfig.LoggingEnabled)
                    WCLogger.Logger = new Logger();

                var storage = BuildStorage();

                SignClient = await WalletConnectSignClient.Init(new SignClientOptions
                {
                    Metadata = projectConfig.Metadata,
                    Name = projectConfig.Metadata.Name,
                    ProjectId = projectConfig.Id,
                    Storage = storage,
                    RelayUrlBuilder = new UnityRelayUrlBuilder(),
                    ConnectionBuilder = new NativeWebSocketConnectionBuilder()
                });

                IsInitialized = true;

                return this;
            }
            finally
            {
                _initializationSemaphore.Release();
            }
        }

        public async Task<bool> TryResumeSessionAsync()
        {
            var sessions = SignClient.Session.Values;
            if (sessions.Length == 0)
                return false;

            var session = sessions.FirstOrDefault(session => session.Acknowledged == true);

            if (string.IsNullOrWhiteSpace(session.Topic))
                return false;


            // TODO: finish
            SignClient.AddressProvider.DefaultSession = session;

            var acknowledgement = await SignClient.Extend(session.Topic);
            await acknowledgement.Acknowledged();

            return true;
        }

        public Task<ConnectedData> ConnectAsync(ConnectOptions options)
        {
            // TODO: wrap and handle deep link
            return SignClient.Connect(options);
        }

        public Task<TResponse> RequestAsync<TRequestData, TResponse>(TRequestData data)
        {
            ThrowIfNoActiveSession();
            return SignClient.Request<TRequestData, TResponse>(ActiveSession.Topic, data);
        }

        public Task DisconnectAsync()
        {
            ThrowIfNoActiveSession();
            return SignClient.Disconnect(ActiveSession.Topic, new SessionDelete());
        }

        private static IKeyValueStorage BuildStorage()
        {
            var path = $"{Application.persistentDataPath}/WalletConnect/storage.json";
            WCLogger.Log($"[WalletConnectUnity] Using storage path <i>{path}</i>");
            return new FileSystemStorage(path);
        }

        private void ThrowIfNoActiveSession()
        {
            if (!IsInitialized || string.IsNullOrWhiteSpace(ActiveSession.Topic))
                throw new Exception("No active session");
        }
    }
}