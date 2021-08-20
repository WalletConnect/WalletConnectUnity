using System.Threading.Tasks;
using WalletConnectSharp.Core;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;

namespace WalletConnectSharp.Unity
{
    public class WalletConnectUnitySession : WalletConnectSession
    {
        private WalletConnect unityObjectSource;
        
        public WalletConnectUnitySession(SavedSession savedSession, WalletConnect source, ITransport transport = null, ICipher cipher = null, EventDelegator eventDelegator = null) : base(savedSession, transport, cipher, eventDelegator)
        {
            this.unityObjectSource = source;
        }

        public WalletConnectUnitySession(ClientMeta clientMeta, WalletConnect source, string bridgeUrl = null, ITransport transport = null, ICipher cipher = null, int chainId = 1, EventDelegator eventDelegator = null) : base(clientMeta, bridgeUrl, transport, cipher, chainId, eventDelegator)
        {
            this.unityObjectSource = source;
        }

        internal async Task<WCSessionData> SourceConnectSession()
        {
            return await base.ConnectSession();
        }

        public override async Task Connect()
        {
            await ConnectSession();
        }

        public override async Task<WCSessionData> ConnectSession()
        {
            TaskCompletionSource<WCSessionData> eventCompleted =
                new TaskCompletionSource<WCSessionData>(TaskCreationOptions.None);
            //Block this call and redirect to the source object
            unityObjectSource.ConnectedEventSession.AddListener(delegate(WCSessionData arg0)
            {
                eventCompleted.SetResult(arg0);
            });
            
            unityObjectSource.StartConnect();

            return await eventCompleted.Task;
        }
    }
}