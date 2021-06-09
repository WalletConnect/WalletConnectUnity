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

        public string ConnectURL
        {
            get
            {
                return Provider.URI;
            }
        }

        public bool persistThroughScenes = true;

        public bool waitForWalletOnStart = true;

        public ConnectedEventNoSession ConnectedEvent;

        public ConnectedEventWithSession ConnectedEventSession;

        public WalletConnectProtocol Provider { get; private set; }

        [SerializeField]
        public ClientMeta AppData;

        public override void Awake()
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
            
            Provider = new WalletConnectProtocol(AppData, _transport);

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

            var connectTask = Task.Run(() => Provider.Connect());

            var coroutineInstruction = new WaitForTaskResult<WCSessionData>(connectTask);
            yield return coroutineInstruction;

            var task = coroutineInstruction.Source;

            if (task.Exception != null)
            {
                throw task.Exception;
            }
            
            onConnected.Invoke(task.Result);
        }
    }
}