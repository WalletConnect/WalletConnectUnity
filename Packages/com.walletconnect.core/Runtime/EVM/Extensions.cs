using System;
using System.Linq;
using System.Threading.Tasks;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Common.Model.Errors;

namespace WalletConnectUnity.Core.Evm
{
    public static class EvmExtensions
    {
        public static async Task SwitchEthereumChainAsync(this IWalletConnect walletConnect, EthereumChain ethereumChain)
        {
            if (ethereumChain == null)
                throw new ArgumentNullException(nameof(ethereumChain));

            if (!walletConnect.ActiveSession.Namespaces.TryGetValue("eip155", out var @namespace)
                || !@namespace.Chains.Contains(ethereumChain.chainIdDecimal))
            {
                var request = new WalletAddEthereumChain(ethereumChain);

                try
                {
                    await walletConnect.RequestAsync<WalletAddEthereumChain, string>(request);
                }
                catch (WalletConnectException e)
                {
                    WCLogger.LogError(e);
                }
            }

            var data = new WalletSwitchEthereumChain(ethereumChain.chainIdHex);
            await walletConnect.RequestAsync<WalletSwitchEthereumChain, string>(data);
        }
    }
}