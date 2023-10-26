using System;
using System.Threading.Tasks;
using WalletConnectSharp.Sign.Interfaces;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;

namespace WalletConnect.Unity
{
    public interface IWalletConnectUnity
    {
        public ISignClient SignClient { get; }
        
        public SessionStruct ActiveSession { get; }
        
        public bool IsInitialized { get;}
        
        public bool IsConnected { get; }
        
        public event EventHandler<SessionStruct> ActiveSessionChanged; 

        public Task<IWalletConnectUnity> InitializeAsync();

        public Task<bool> TryResumeSessionAsync();
        
        public Task<ConnectedData> ConnectAsync(ConnectOptions options);

        public Task<TResponse> RequestAsync<TRequestData, TResponse>(TRequestData data);

        public Task DisconnectAsync();
    }
}