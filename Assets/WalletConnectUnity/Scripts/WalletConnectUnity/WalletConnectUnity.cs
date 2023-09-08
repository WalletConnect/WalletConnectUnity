using System;
using System.Threading.Tasks;
using UnityBinder;
using UnityEngine;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Storage;
using WalletConnectSharp.Storage.Interfaces;
using ILogger = WalletConnectSharp.Common.Logging.ILogger;

namespace WalletConnect
{
    [RequireComponent(typeof(WCWebSocketBuilder))]
    public class WalletConnectUnity : BindableMonoBehavior, ILogger
    {
        private static WalletConnectUnity _instance;

        public static WalletConnectUnity Instance => _instance;

        public string ProjectName;
        public string ProjectId;
        public Metadata ClientMetadata;
        
        public bool ConnectOnAwake;
        public bool ConnectOnStart;
        public bool EnableCoreLogging;
        public string BaseContext = "unity-game";

        public WCStorageType StorageType;

        [BindComponent]
        private WCWebSocketBuilder _builder;
        public WalletConnectCore Core { get; private set; }

        private TaskCompletionSource<bool> initTask;

        protected override async void Awake()
        {
            base.Awake();

            if (_instance == null || _instance == this)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }

            if (ConnectOnAwake)
            {
                await InitCore();
            }
        }

        // Start is called before the first frame update
        async void Start()
        {
            if (ConnectOnStart) 
            {
                await InitCore();
            }
        }

        internal async Task InitCore()
        {
            if (initTask != null)
            {
                await initTask.Task;
                return;
            }

            initTask = new TaskCompletionSource<bool>();

            var storage = BuildStorage();

            if (_builder == null)
                _builder = GetComponent<WCWebSocketBuilder>();

            if (EnableCoreLogging)
                WCLogger.Logger = this;

            Core = new WalletConnectCore(new CoreOptions()
            {
                Name = ProjectName,
                ProjectId = ProjectId,
                BaseContext = BaseContext,
                Storage = storage,
                ConnectionBuilder = _builder,
            });

            await Core.Start();
            
            initTask.SetResult(true);
        }

        public IKeyValueStorage BuildStorage()
        {
            switch (StorageType)
            {
                case WCStorageType.Disk:
                    var path = Application.persistentDataPath + "/walletconnect.json";
                    Debug.Log("Using storage location: " + path);
                    return new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json");
                case WCStorageType.None:
                    return new InMemoryStorage();
                default:
                    throw new Exception("Invalid value");
            }
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void LogError(string message)
        {
            Debug.LogError(message);
        }

        public void LogError(Exception e)
        {
            Debug.LogException(e);
        }
    }
}
