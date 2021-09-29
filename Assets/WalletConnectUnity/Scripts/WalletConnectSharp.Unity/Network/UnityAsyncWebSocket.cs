using System;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;

public class UnityAsyncWebSocket : MonoBehaviour
{
    [HideInInspector]
    public ITransport source;
    private WebSocket client;
    
    [HideInInspector]
    public string url;
    
    public event WebSocketMessageEventHandler MessageReceived;
    public event EventHandler<MessageReceivedEventArgs> OpenReceived;

    async void Start()
    {
        if (url.StartsWith("https"))
            url = url.Replace("https", "wss");
        else if (url.StartsWith("http"))
            url = url.Replace("http", "ws");
            
        if (client != null)
            return;
        _socketOpen();
    }

    private async void _socketOpen()
    {
        client = new WebSocket(url);

        client.OnOpen += () =>
        {
            // subscribe now
            if (this.OpenReceived != null)
                OpenReceived(this, null);

        };

        client.OnMessage += delegate(byte[] data)
        {
            if (MessageReceived != null)
            {
                MessageReceived(data);
            }
        };
        client.OnClose += ClientTryReconnect;
        client.OnError += (e) => {

            Debug.Log("OnError " + e);

        };

        await client.Connect();
    }

    private void ClientTryReconnect(WebSocketCloseCode closeCode)
    {
        client = null;
        _socketOpen();
    }

    public void CancelConnection()
    {
        client.CancelConnection();
    }

    async void OnDestroy()
    {
        if (client != null && client.State == WebSocketState.Open)
        {
            await Close();
        }
    }

    private void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (client != null && client.State == WebSocketState.Open)
        {
            client.DispatchMessageQueue();
        }
#endif
    }

    public async Task Close()
    {
        client.OnClose -= ClientTryReconnect;
        await client.Close();
    }

    public async Task SendMessage(NetworkMessage message)
    {
        string finalJson = JsonConvert.SerializeObject(message);
            
        await this.client.SendText(finalJson);
    }
}