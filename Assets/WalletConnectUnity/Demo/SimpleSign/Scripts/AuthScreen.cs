using UnityBinder;
using UnityEngine;
using WalletConnect;
using WalletConnectUnity.Utils;

namespace WalletConnectUnity.Demo.SimpleSign
{
    public class AuthScreen : BindableMonoBehavior
    {
        [Inject]
        private WCSignClient WC;

        public GameObject selectChainScreen;
        
        public async void SignOut()
        {
            await WC.Disconnect(WC.Session.Values[0].Topic);
            
            // TODO Perhaps ensure we are using Unity's Sync context inside WalletConnectSharp
            MTQ.Enqueue(() =>
            {
                selectChainScreen.SetActive(true);
                gameObject.SetActive(false);
            });
        }

        public async void SignMessage()
        {
            // TODO Send eth_sign message for example
        }
    }
}