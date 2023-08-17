using System.Threading.Tasks;
using UnityBinder;
using UnityEngine;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Crypto;
using WalletConnectSharp.Storage;

namespace WalletConnect
{
    [RequireComponent(typeof(WCWebSocketBuilder))]
    public class WalletConnectUnity : BindableMonoBehavior
    {
        private static WalletConnectUnity _instance;

        public static WalletConnectUnity Instance => _instance;
        
        public string ProjectName;
        public string ProjectId;
        public Metadata ClientMetadata;
        
        public bool ConnectOnAwake;
        public bool ConnectOnStart;
        public string BaseContext = "unity-game";
        
        private bool _initialized = false;
        private bool _initializing = false;

        [BindComponent]
        private WCWebSocketBuilder _builder;
        public WalletConnectCore Core { get; private set; }

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
            if (_initialized || _initializing)
                return;

            _initializing = true;

            try
            {
                WCLogger.Logger = new WCUnityLogger();
                
                var storage = new FileSystemStorage(Application.persistentDataPath + "/walletconnect.json");
                //var keychain = new KeyChain(storage);

                //var crypto = new WCUnityCrypto(keychain);

                if (_builder == null)
                    _builder = GetComponent<WCWebSocketBuilder>();

                Core = new WalletConnectCore(new CoreOptions()
                {
                    Name = ProjectName,
                    ProjectId = ProjectId,
                    BaseContext = BaseContext,
                    Storage = storage,
                    ConnectionBuilder = _builder,
                    //CryptoModule = crypto,
                });

                await Core.Start();

                _initialized = true;
            }
            finally
            {
                _initializing = false;
            }
        }
    }
}
