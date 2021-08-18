using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Unity.Models;
using WalletConnectSharp.Unity.Network;
using WalletConnectSharp.Unity.Utils;

namespace WalletConnectSharp.Unity
{
    [RequireComponent(typeof(NativeWebSocketTransport))]
    public class WalletConnect : BindableMonoBehavior
    {
        public Dictionary<string, AppEntry> SupportedWallets
        {
            get;
            private set;
        }
        
        public AppEntry SelectedWallet { get; set; }

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

        public IEnumerator FetchWalletList()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://registry.walletconnect.org/data/wallets.json"))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
                
                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error Getting Wallet Info: " + webRequest.error);
                }
                else
                {
                    var json = webRequest.downloadHandler.text;

                    SupportedWallets = JsonConvert.DeserializeObject<Dictionary<string, AppEntry>>(json);

                    string[] sizes = new string[] {"sm", "md", "lg"};
                    foreach (var id in SupportedWallets.Keys)
                    {
                        var data = SupportedWallets[id];

                        foreach (var size in sizes)
                        {
                            var url = "https://registry.walletconnect.org/logo/" + size + "/" + id + ".jpeg";

                            using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(url))
                            {
                                yield return imageRequest.SendWebRequest();

                                if (imageRequest.isNetworkError)
                                {
                                    Debug.Log("Error Getting Wallet Icon: " + imageRequest.error);
                                }
                                else
                                {
                                    var texture = ((DownloadHandlerTexture) imageRequest.downloadHandler).texture;
                                    var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height),
                                        new Vector2(0.5f, 0.5f), 100.0f);
                                    
                                    if (size == "sm")
                                    {
                                        data.smallIcon = sprite;
                                    } 
                                    else if (size == "md")
                                    {
                                        data.medimumIcon = sprite;
                                    } 
                                    else if (size == "lg")
                                    {
                                        data.largeIcon = sprite;
                                    }
                                }
                            }
                        }
                        
                    }
                }
            }
        }

        public void OpenMobileWallet(AppEntry selectedWallet)
        {
            SelectedWallet = selectedWallet;
            
            OpenMobileWallet();
        }
        
        public void OpenDeepLink(AppEntry selectedWallet)
        {
            SelectedWallet = selectedWallet;
            
            OpenDeepLink();
        }

        public void OpenMobileWallet()
        {
#if UNITY_ANDROID
            var signingURL = ConnectURL.Split('@')[0];

            Application.OpenURL(signingURL);
#elif UNITY_IOS
            if (SelectedWallet == null)
            {
                throw new NotImplementedException(
                    "You must use OpenMobileWallet(AppEntry) or set SelectedWallet on iOS!");
            }
            else
            {
                string url;
                string encodedConnect = WebUtility.UrlEncode(ConnectURL);
                if (!string.IsNullOrWhiteSpace(SelectedWallet.mobile.universal))
                {
                    url = SelectedWallet.mobile.universal + "/wc?uri=" + encodedConnect;
                }
                else
                {
                    url = SelectedWallet.mobile.native + (SelectedWallet.mobile.native.EndsWith(":") ? "//" : "/") +
                          "wc?uri=" + encodedConnect;
                }

                var signingUrl = url.Split('?')[0];
                
                Debug.Log("Opening: " + signingUrl);
                Application.OpenURL(signingUrl);
            }
#else
            return;
#endif
        }

        public void OpenDeepLink()
        {
#if UNITY_ANDROID
            Application.OpenURL(ConnectURL);
#elif UNITY_IOS
            if (SelectedWallet == null)
            {
                throw new NotImplementedException(
                    "You must use OpenDeepLink(AppEntry) or set SelectedWallet on iOS!");
            }
            else
            {
                string url;
                string encodedConnect = WebUtility.UrlEncode(ConnectURL);
                if (!string.IsNullOrWhiteSpace(SelectedWallet.mobile.universal))
                {
                    url = SelectedWallet.mobile.universal + "/wc?uri=" + encodedConnect;
                }
                else
                {
                    url = SelectedWallet.mobile.native + (SelectedWallet.mobile.native.EndsWith(":") ? "//" : "/") +
                          "wc?uri=" + encodedConnect;
                }
                
                Debug.Log("Opening: " + url);
                Application.OpenURL(url);
            }
#else
            return;
#endif
        }
    }
}