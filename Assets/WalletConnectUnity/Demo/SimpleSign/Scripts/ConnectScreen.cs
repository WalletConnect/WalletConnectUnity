using System;
using System.Linq;
using UnityBinder;
using UnityEngine;
using WalletConnect;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Utils;

namespace WalletConnectUnity.Demo.SimpleSign
{
    public class ConnectScreen : BindableMonoBehavior
    {
        public BlockchainList BlockchainList;
        public GameObject AuthScreen;

        [Inject]
        private WCSignClient WC;

        private void Start()
        {
            WC.OnSessionApproved += WCOnOnSessionApproved;

            WC.SessionDeleted += (sender, @event) =>
            {
                gameObject.SetActive(true);
                AuthScreen.SetActive(false);
            };
        }

        private void WCOnOnSessionApproved(object sender, SessionStruct e) => MTQ.Enqueue(() =>
        {
            // Enable auth example canvas and disable ourself
            gameObject.SetActive(false);
            AuthScreen.SetActive(true); 
        });

        public async void OnConnect()
        {
            if (WC.SignClient == null)
                await WC.InitSignClient();
            
            var chains = BlockchainList.SelectedChains;

            if (chains.Length == 0)
            {
                Debug.LogError("No chains selected!");
                return;
            }

            if (WC == null)
            {
                Debug.LogError("No WCSignClient scripts found in scene!");
                return;
            }

            // Connect Sign Client
            Debug.Log("Connecting sign client..");

            var requiredNamespaces = new RequiredNamespaces();

            if (chains.Any(c => c.ChainNamespace == Chain.EvmNamespace))
            {
                var eipChains = chains.Where(c => c.ChainNamespace == Chain.EvmNamespace);
                
                // TODO Make configurable
                var methods = new string[]
                {
                    "eth_sendTransaction",
                    "eth_signTransaction",
                    "eth_sign",
                    "personal_sign",
                    "eth_signTypedData",
                };

                var events = new string[]
                {
                    "chainChanged", "accountsChanged"
                };

                var chainIds = eipChains.Select(c => c.FullChainId).ToArray();
                
                requiredNamespaces.Add(Chain.EvmNamespace, new ProposedNamespace()
                {
                    Chains = chainIds,
                    Events = events,
                    Methods = methods
                });
            }
            
            // TODO Do other chain namespaces

            var dappConnectOptions = new ConnectOptions()
            {
                RequiredNamespaces = requiredNamespaces
            };

            var connectData = await WC.Connect(dappConnectOptions);
            
            Debug.Log($"Connection successful, URI: {connectData.Uri}");

            try
            {
                await connectData.Approval;
                
                // We need to move this to the main unity thread
                // TODO Perhaps ensure we are using Unity's Sync context inside WalletConnectSharp
                MTQ.Enqueue(() =>
                {
                    Debug.Log($"Connection approved, URI: {connectData.Uri}");
                });
            }
            catch (Exception e)
            {
                Debug.LogError(("Connection failed: " + e.Message));
                Debug.LogError(e);
            }
        }
    }
}
