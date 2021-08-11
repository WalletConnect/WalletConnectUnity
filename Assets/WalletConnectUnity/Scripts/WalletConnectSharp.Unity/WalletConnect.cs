using System;
using System.Collections;
using System.Threading.Tasks;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity.Network;
using WalletConnectSharp.Unity.Utils;

namespace WalletConnectSharp.Unity
{
    [RequireComponent(typeof(NativeWebSocketTransport))]
    public class WalletConnect : BindableMonoBehavior
    {
        [Serializable]
        public class ConnectedEventNoSession : UnityEvent { }
        [Serializable]
        public class ConnectedEventWithSession : UnityEvent<WCSessionData> { }

        [BindComponent]
        private NativeWebSocketTransport _transport;

        private static WalletConnect _instance;

        public static WalletConnect Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public static WalletConnectSession ActiveSession
        {
            get
            {
                return _instance.Session;
            }
        }

        public string ConnectURL
        {
            get
            {
                return Protocol.URI;
            }
        }

        public bool persistThroughScenes = true;

        public bool waitForWalletOnStart = true;
        
        public string customBridgeUrl;
        
        public int chainId = 1;

        public ConnectedEventNoSession ConnectedEvent;

        public ConnectedEventWithSession ConnectedEventSession;

        public WalletConnectSession Session
        {
            get;
            private set;
        }

        [Obsolete("Use Session instead of Protocol")]
        public WalletConnectSession Protocol {
            get { return Session; }
            private set
            {
                Session = value;
            }
        }

        public bool Connected
        {
            get
            {
                return Protocol.Connected;
            }
        }

        [SerializeField]
        public ClientMeta AppData;

        protected override void Awake()
        {
            if (persistThroughScenes)
            {
                if (_instance != null)
                {
                    Destroy(gameObject);
                    return;
                }

                DontDestroyOnLoad(gameObject);
            }
            
            _instance = this;
            
            base.Awake();

            if (string.IsNullOrWhiteSpace(customBridgeUrl))
            {
                customBridgeUrl = null;
            }
            
            Session = new WalletConnectSession(AppData, customBridgeUrl, _transport, null, chainId);
            
            #if UNITY_ANDROID || UNITY_IOS
            //Whenever we send a request to the Wallet, we want to open the Wallet app
            Session.OnSend += (sender, session) => OpenMobileWallet();
            #endif

            if (waitForWalletOnStart)
            {
                StartConnect();
            }
        }

        public void StartConnect()
        {
            ConnectedEventWithSession allEvents = new ConnectedEventWithSession();
                
            allEvents.AddListener(delegate(WCSessionData arg0)
            {
                ConnectedEvent.Invoke();
                ConnectedEventSession.Invoke(arg0);
            });
                
            WaitForWalletConnection(allEvents);
        }

        public void WaitForWalletConnection(UnityEvent<WCSessionData> onConnected)
        {
            StartCoroutine(ConnectAsync(onConnected));
        }

        private IEnumerator ConnectAsync(UnityEvent<WCSessionData> onConnected)
        {
            Debug.Log("Waiting for Wallet connection");

            var connectTask = Task.Run(() => Session.ConnectSession());

            var coroutineInstruction = new WaitForTaskResult<WCSessionData>(connectTask);
            yield return coroutineInstruction;

            var task = coroutineInstruction.Source;

            if (task.Exception != null)
            {
                throw task.Exception;
            }
            
            onConnected.Invoke(task.Result);
        }

        public void OpenMobileWallet()
        {
#if UNITY_ANDROID
            var signingURL = ConnectURL.Split('@')[0];

            Application.OpenURL(signingURL);
#elif UNITY_IOS
            //TODO Implement IOS Deep Linking
#else
            return;
#endif
        }

        public void OpenDeepLink()
        {
#if UNITY_ANDROID
            Application.OpenURL(ConnectURL);
#elif UNITY_IOS
            //TODO Implement IOS Deep Linking
#else
            return;
#endif
        }
    }
}