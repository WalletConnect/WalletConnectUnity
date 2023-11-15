using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityBinder;
using UnityEngine;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Pairing;
using WalletConnectSharp.Core.Models.Publisher;
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
        
        [BindComponent]
        private WalletConnectUnity WalletConnectUnity;
        
        public WalletConnectSignClient SignClient { get; private set; }

        public event EventHandler<ConnectedData> OnConnect;

        public event EventHandler<SessionStruct> OnSessionApproved; 

        public bool ConnectOnAwake => WalletConnectUnity.ConnectOnAwake;
        public bool ConnectOnStart => WalletConnectUnity.ConnectOnStart;

        public bool SetDefaultSessionOnApproval = true;

        private TaskCompletionSource<bool> initTask = null;
        
        public List<string> OpenWalletMethods = new List<string>();
        
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
            if (initTask != null)
            {
                await initTask.Task;
                return;
            }

            if (SignClient != null)
                return;
            
            initTask = new TaskCompletionSource<bool>();

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

                SignClient.SessionExpired += SessionExpired;
                SignClient.PairingExpired += PairingExpired;
                SignClient.SessionProposed += SessionProposed;
                SignClient.SessionConnected += SessionConnected;
                SignClient.SessionConnectionErrored += SessionConnectionErrored;
                SignClient.SessionUpdateRequest += SessionUpdateRequest;
                SignClient.SessionExtendRequest += SessionExtendRequest;
                SignClient.SessionUpdated += SessionUpdated;
                SignClient.SessionExtended += SessionExtended;
                SignClient.SessionPinged += SessionPinged;
                SignClient.SessionDeleted += SessionDeleted;
                SignClient.SessionRejected += SessionRejected;
                SignClient.SessionApproved += SessionApproved;
                SignClient.PairingPinged += PairingPinged;
                SignClient.PairingDeleted += PairingDeleted;
                
                initTask.SetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                
                initTask.SetException(e);
            }
            finally
            {
                // Just in case we get here by cosmic ray
                initTask.TrySetResult(false);
            }
        }

        public SessionStruct DefaultSession;
        
        public string Name => SignClient.Name;
        public string Context => SignClient.Context;
        public PendingRequestStruct[] PendingSessionRequests => SignClient.PendingSessionRequests;
        public event EventHandler<SessionStruct> SessionExpired;
        public event EventHandler<PairingEvent> PairingExpired;
        public event EventHandler<SessionProposalEvent> SessionProposed;
        public event EventHandler<SessionStruct> SessionConnected;
        public event EventHandler<Exception> SessionConnectionErrored;
        public event EventHandler<SessionUpdateEvent> SessionUpdateRequest;
        public event EventHandler<SessionEvent> SessionExtendRequest;
        public event EventHandler<SessionEvent> SessionUpdated;
        public event EventHandler<SessionEvent> SessionExtended;
        public event EventHandler<SessionEvent> SessionPinged;
        public event EventHandler<SessionEvent> SessionDeleted;
        public event EventHandler<SessionStruct> SessionRejected;
        public event EventHandler<SessionStruct> SessionApproved;
        public event EventHandler<PairingEvent> PairingPinged;
        public event EventHandler<PairingEvent> PairingDeleted;
        public Metadata Metadata => SignClient.Metadata;
        public IAddressProvider AddressProvider => SignClient.AddressProvider;
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
            WCLogger.Log("Starting connect");
            var connectData = await SignClient.Connect(options);

            if (connectData == null)
                throw new Exception("Failed to connect");
            
            if (OnConnect != null)
                OnConnect(this, connectData);

            Task<SessionStruct> ogApproval = connectData.Approval;

            async Task<SessionStruct> ApprovalWrapper()
            {
                WCLogger.Log("Waiting for approval from wallet");
                await ogApproval;
                
                WCLogger.Log("Got approval");
                var sessionResult = ogApproval.Result;
                
                OnSessionApproval(sessionResult);

                return sessionResult;
            }

            connectData.Approval = ApprovalWrapper();

            if (!WalletConnectUnity.UseDeeplink || WalletConnectUnity.DefaultWallet == null) return connectData;
            
            WalletConnectUnity.DefaultWallet.OpenSessionProposalDeepLink(connectData);

            return connectData;
        }
        
        public TypedEventHandler<T, TR> SessionRequestEvents<T, TR>()
        {
            return SignClient.SessionRequestEvents<T, TR>();
        }

        internal void OnSessionApproval(SessionStruct session)
        {
            OnSessionApproved?.Invoke(this, session);

            if (SetDefaultSessionOnApproval)
                DefaultSession = session;
            
            WalletConnectUnity.FindAndSetDefaultWallet(session.Peer.Metadata);
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

        public Task<IAcknowledgement> UpdateSession(Namespaces namespaces)
        {
            ValidateDefaultSessionNotNull();
            return UpdateSession(DefaultSession.Topic, namespaces);
        }

        public Task<IAcknowledgement> Extend(string topic)
        {
            return SignClient.Extend(topic);
        }

        public Task<IAcknowledgement> Extend()
        {
            ValidateDefaultSessionNotNull();
            return Extend(DefaultSession.Topic);
        }

        public Task<TR> Request<T, TR>(string topic, T data, string chainId = null, long? expiry = null)
        {
            var method = RpcMethodAttribute.MethodForType<T>();
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            if (OpenWalletMethods.Contains(method))
            {
                EventUtils.ListenOnce<PublishParams>((sender, args) =>
                    {
                        if (args.Topic != topic)
                            return;

                        var session = SignClient.Session.Get(args.Topic);
                        if (session.Peer.Metadata is { Redirect: not null })
                            Application.OpenURL(session.Peer.Metadata.Redirect.Native);
                    },
                    handler => Core.Relayer.Publisher.OnPublishedMessage += handler,
                    handler => Core.Relayer.Publisher.OnPublishedMessage -= handler
                );
            }
#endif
            return SignClient.Request<T, TR>(topic, data, chainId, expiry);
        }

        public Task<TR> Request<T, TR>(T data, string chainId = null, long? expiry = null)
        {
            ValidateDefaultSessionNotNull();
            return Request<T, TR>(DefaultSession.Topic, data, chainId, expiry);
        }

        public Task Respond<T, TR>(string topic, JsonRpcResponse<TR> response)
        {
            return SignClient.Respond<T, TR>(topic, response);
        }

        public Task Respond<T, TR>(JsonRpcResponse<TR> response)
        {
            ValidateDefaultSessionNotNull();
            return Respond<T, TR>(DefaultSession.Topic, response);
        }

        public Task Emit<T>(string topic, EventData<T> eventData, string chainId = null)
        {
            return SignClient.Emit<T>(topic, eventData, chainId);
        }

        public Task Emit<T>(EventData<T> eventData, string chainId = null)
        {
            ValidateDefaultSessionNotNull();
            return Emit<T>(eventData, chainId);
        }

        public Task Ping(string topic)
        {
            return SignClient.Ping(topic);
        }

        public Task Ping()
        {
            ValidateDefaultSessionNotNull();
            return Ping(DefaultSession.Topic);
        }

        public Task Disconnect(string topic, Error reason = null)
        {
            return SignClient.Disconnect(topic, reason);
        }

        public Task Disconnect(Error reason = null)
        {
            ValidateDefaultSessionNotNull();
            return Disconnect(DefaultSession.Topic, reason);
        }

        public SessionStruct[] Find(RequiredNamespaces requiredNamespaces)
        {
            return SignClient.Find(requiredNamespaces);
        }

        public void HandleEventMessageType<T>(Func<string, JsonRpcRequest<SessionEvent<T>>, Task> requestCallback, Func<string, JsonRpcResponse<bool>, Task> responseCallback)
        { 
            SignClient.HandleEventMessageType<T>(requestCallback, responseCallback);
        }
        
        private void ValidateDefaultSessionNotNull()
        {
            if (string.IsNullOrWhiteSpace(DefaultSession.Topic))
            {
                throw new Exception("No default session set. Set DefaultSession before invoking this method");
            }
        }
        
        public void Dispose()
        {
            SignClient?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}