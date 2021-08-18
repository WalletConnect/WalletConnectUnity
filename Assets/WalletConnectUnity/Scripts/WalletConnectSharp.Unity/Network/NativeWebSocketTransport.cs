using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;

namespace WalletConnectSharp.Unity.Network
{
    public class NativeWebSocketTransport : MonoBehaviour, ITransport
    {
        private bool opened = false;

        private UnityAsyncWebSocket client;
        private EventDelegator _eventDelegator;
        private bool wasPaused;
        private string currentUrl;
        private List<string> subscribedTopics = new List<string>();

        public bool Opened
        {
            get
            {
                return opened;
            }
        }

        public void AttachEventDelegator(EventDelegator eventDelegator)
        {
            this._eventDelegator = eventDelegator;
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.CancelConnection();
                Destroy(client);
            }
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<MessageReceivedEventArgs> OpenReceived;

        public async Task Open(string url)
        {
            if (url.StartsWith("https"))
                url = url.Replace("https", "wss");
            else if (url.StartsWith("http"))
                url = url.Replace("http", "ws");

            currentUrl = url;
            
            TaskCompletionSource<object> openEventCompleted = new TaskCompletionSource<object>(TaskCreationOptions.None);
            TaskCompletionSource<object> createdObjectCompleted = new TaskCompletionSource<object>(TaskCreationOptions.None);
            
            MainThreadUtil.Run(delegate
            {
                client = gameObject.AddComponent<UnityAsyncWebSocket>();
                client.url = url;
                client.source = this;
                
                client.OpenReceived += (sender, args) =>
                {
                    createdObjectCompleted.SetResult(args);

                    if (OpenReceived != null)
                    {
                        OpenReceived(this, args);
                    }
                };

                client.MessageReceived += OnMessageReceived;
                
                openEventCompleted.SetResult(client);
            });

            await openEventCompleted.Task;

            await createdObjectCompleted.Task;
        }

        private async void OnMessageReceived(byte[] bytes)
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);

            try
            {
                var msg = JsonConvert.DeserializeObject<NetworkMessage>(json);

                
                await SendMessage(new NetworkMessage()
                {
                    Payload = "",
                    Type = "ack",
                    Silent = true,
                    Topic = msg.Topic
                });

                if (this.MessageReceived != null)
                    MessageReceived(this, new MessageReceivedEventArgs(msg, this));
            }
            catch(Exception e)
            {
                Debug.Log("Exception " + e.Message);
            }   
        }

        public async Task Close()
        {
            await client.Close();
            
            Destroy(client);

            this.opened = false;
        }

        public async Task SendMessage(NetworkMessage message)
        {
            await this.client.SendMessage(message);
        }

        public async Task Subscribe(string topic)
        {
            Debug.Log("Subscribe");

            await SendMessage(new NetworkMessage()
            {
                Payload = "",
                Type = "sub",
                Silent = true,
                Topic = topic
            });

            if (!subscribedTopics.Contains(topic))
            {
                subscribedTopics.Add(topic);
            }

            opened = true;
        }

        public async Task Subscribe<T>(string topic, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
        {
            await Subscribe(topic);

            _eventDelegator.ListenFor(topic, callback);
        }
        
        public async Task Subscribe<T>(string topic, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
        {
            await Subscribe(topic);

            _eventDelegator.ListenFor(topic, callback);
        }

        #if UNITY_IOS
        private IEnumerator OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("IOS Pausing");
                wasPaused = true;
                
                //We need to close the Websocket Properly
                var closeTask = Task.Run(Close);
                var coroutineInstruction = new WaitForTask(closeTask);
                yield return coroutineInstruction;
            }
            else if (wasPaused)
            {
                Debug.Log("IOS Resuming");
                var openTask = Task.Run(() => Open(currentUrl));
                var coroutineInstruction = new WaitForTask(openTask);
                yield return coroutineInstruction;

                foreach (var topic in subscribedTopics)
                {
                    var subTask = Task.Run(() => Subscribe(topic));
                    var coroutineSubInstruction = new WaitForTask(subTask);
                    yield return coroutineSubInstruction;
                }
            }
        }
        #endif
    }
}