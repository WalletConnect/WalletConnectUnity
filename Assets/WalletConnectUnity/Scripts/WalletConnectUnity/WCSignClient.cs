using System;
using System.Threading.Tasks;
using UnityBinder;
using UnityEngine;
using UnityEngine.Scripting;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Controllers;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectSharp.Sign.Models.Engine.Events;
using WalletConnectSharp.Sign.Models.Engine.Methods;

namespace WalletConnect
{
    [RequireComponent(typeof(WalletConnectUnity))]
    public class WCSignClient : BindableMonoBehavior, ISignClient
    {
        private static WCSignClient _currentInstance;

        public static WCSignClient Instance => _currentInstance;
        
        private bool _initialized = false;
        private bool _initializing = false;
        [BindComponent]
        private WalletConnectUnity WalletConnectUnity;
        
        public WalletConnectSignClient SignClient { get; private set; }

        public event EventHandler<ConnectedData> OnConnect;

        public event EventHandler<SessionStruct> OnSessionApproved; 

        public bool ConnectOnAwake => WalletConnectUnity.ConnectOnAwake;
        public bool ConnectOnStart => WalletConnectUnity.ConnectOnStart;
        
        protected override async void Awake()
        {
            base.Awake();
            
            if (_currentInstance == null || _currentInstance == this)
            {
                _currentInstance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }
            
            if (ConnectOnAwake)
            {
                await InitSignClient();
            }
        }

        private async void Start()
        {
            if (ConnectOnStart)
            {
                await InitSignClient();
            }
        }

        public async Task InitSignClient()
        {
            if (_initialized || _initializing)
                return;

            _initializing = true;
            try
            {
                await WalletConnectUnity.InitCore();

                SignClient = await WalletConnectSignClient.Init(new SignClientOptions()
                {
                    BaseContext = WalletConnectUnity.BaseContext,
                    Core = WalletConnectUnity.Core,
                    Metadata = WalletConnectUnity.ClientMetadata,
                    Name = WalletConnectUnity.ProjectName,
                    ProjectId = WalletConnectUnity.ProjectId,
                    Storage = WalletConnectUnity.Core.Storage,
                });

                _initialized = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _initializing = false;
            }
        }

        public string Name => SignClient.Name;
        public string Context => SignClient.Context;
        public EventDelegator Events => SignClient.Events;
        public PendingRequestStruct[] PendingSessionRequests => SignClient.PendingSessionRequests;
        public Metadata Metadata => SignClient.Metadata;
        public ICore Core => SignClient.Core;
        public IEngine Engine => SignClient.Engine;
        public ISession Session => SignClient.Session;
        public IProposal Proposal => SignClient.Proposal;
        public IPendingRequests PendingRequests => SignClient.PendingRequests;
        public SignClientOptions Options => SignClient.Options;
        public string Protocol => SignClient.Protocol;
        public int Version => SignClient.Version;
        public async Task<ConnectedData> Connect(ConnectOptions options)
        {
            WCLogger.Log("Test");
            var connectData = await SignClient.Connect(options);

            if (connectData == null)
                throw new Exception("Failed to connect");
            
            if (OnConnect != null)
                OnConnect(this, connectData);

            connectData.Approval = connectData.Approval.ContinueWith(task =>
            {
                var sessionResult = task.Result;

                if (OnSessionApproved != null)
                    OnSessionApproved(this, sessionResult);

                return sessionResult;
            });

            return connectData;
        }

        public Task<ProposalStruct> Pair(string uri)
        {
            // TODO Add event
            return SignClient.Pair(uri);
        }

        public Task<IApprovedData> Approve(ProposalStruct proposalStruct, params string[] approvedAddresses)
        {
            return SignClient.Approve(proposalStruct, approvedAddresses);
        }

        public Task<IApprovedData> Approve(ApproveParams @params)
        {
            return SignClient.Approve(@params);
        }

        public Task Reject(RejectParams @params)
        {
            return SignClient.Reject(@params);
        }

        public Task Reject(ProposalStruct proposalStruct, string message = null)
        {
            return SignClient.Reject(proposalStruct, message);
        }

        public Task Reject(ProposalStruct proposalStruct, Error error)
        {
            return SignClient.Reject(proposalStruct, error);
        }

        public Task<IAcknowledgement> UpdateSession(string topic, Namespaces namespaces)
        {
            return SignClient.UpdateSession(topic, namespaces);
        }

        public Task<IAcknowledgement> Extend(string topic)
        {
            return SignClient.Extend(topic);
        }

        public Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null)
        {
            return SignClient.Request<T, TR>(topic, data, chainId, expiry);
        }

        public Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response)
        {
            return SignClient.Respond<T, TR>(topic, response);
        }

        public Task Emit<T>(string topic, EventData<T> eventData, string chainId = null)
        {
            return SignClient.Emit<T>(topic, eventData, chainId);
        }

        public Task Ping(string topic)
        {
            return SignClient.Ping(topic);
        }

        public Task Disconnect(string topic, Error reason = null)
        {
            return SignClient.Disconnect(topic, reason);
        }

        public SessionStruct[] Find(RequiredNamespaces requiredNamespaces)
        {
            return SignClient.Find(requiredNamespaces);
        }

        public void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback, Func<string, JsonRpcResponse<bool>, Task> responseCallback)
        { 
            SignClient.HandleEventMessageType<T>(requestCallback, responseCallback);
        }
        
        #if !UNITY_MONO
        [Preserve]
        void SetupAOT()
        {
            // Reference all required models
            // This is required so AOT code is generated for these functions
            var historyFactory = new JsonRpcHistoryFactory(null);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionPropose, SessionProposeResponse>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionSettle, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionUpdate, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionExtend, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionDelete, Boolean>().GetType().FullName);
            Debug.Log(historyFactory.JsonRpcHistoryOfType<SessionPing, Boolean>().GetType().FullName);
            EventManager<string, GenericEvent<string>>.InstanceOf(null).PropagateEvent(null, null);
            throw new InvalidOperationException("This method is only for AOT code generation.");
        }
        #endif
    }
}