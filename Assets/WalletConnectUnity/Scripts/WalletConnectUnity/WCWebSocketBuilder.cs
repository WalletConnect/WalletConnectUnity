using UnityEngine;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Interfaces;

namespace WalletConnect
{
    // We need execute always, because WalletConnectSharp may 
    // attempt to use this even after the game (in-editor) has stopped
    [ExecuteAlways]
    public class WCWebSocketBuilder : MonoBehaviour, IConnectionBuilder
    {
        public IJsonRpcConnection CreateConnection(string url)
        {
            var websocket = gameObject.AddComponent<WCWebSocket>();
            websocket.Url = url;

            return websocket;
        }
    }
}