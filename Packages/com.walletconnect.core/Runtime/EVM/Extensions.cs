using System;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common.Model.Errors;

namespace WalletConnectUnity.Core.Evm
{
    public static class EvmExtensions
    {
        public static async Task SwitchEthereumChainAsync(this IWalletConnect walletConnect, EthereumChain ethereumChain)
        {
            if (ethereumChain == null)
                throw new ArgumentNullException(nameof(ethereumChain));
            
            
            if (!walletConnect.ActiveSession.Namespaces.TryGetValue("eip155", out var @namespace))
                throw new InvalidOperationException("EIP-155 namespace not found");
            
            var caip2ChainId = $"eip155:{ethereumChain.chainIdDecimal}";
            
            if (!@namespace.Chains.Contains(caip2ChainId))
            {
                var tcs = new TaskCompletionSource<bool>();
                walletConnect.ActiveChainIdChanged += OnChainChanged;
                
                var request = new WalletAddEthereumChain(ethereumChain);

                try
                {
                    await walletConnect.RequestAsync<WalletAddEthereumChain, string>(request);
                    
                    var data = new WalletSwitchEthereumChain(ethereumChain.chainIdHex);

                    var switchChainTask = walletConnect.RequestAsync<WalletSwitchEthereumChain, string>(data);
                    var chainChangedEventTask = tcs.Task;
            
                    try
                    {
                        await Task.WhenAll(switchChainTask, chainChangedEventTask);
                    }
                    finally
                    {
                        walletConnect.ActiveChainIdChanged -= OnChainChanged;
                    }
                }
                catch (WalletConnectException)
                {
                    // Wallet can decline if chain has already been added
                }
                
                void OnChainChanged(object sender, string chainId)
                {
                    if (chainId != $"eip155:{ethereumChain.chainIdDecimal}")
                        return;

                    tcs.SetResult(true);
                }
            }
            else
            {
                await walletConnect.SignClient.AddressProvider.SetDefaultChainIdAsync(caip2ChainId);
            }
        }
    }
}