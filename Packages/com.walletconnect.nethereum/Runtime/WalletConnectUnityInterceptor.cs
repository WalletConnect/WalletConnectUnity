using System;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using WalletConnectSharp.Common.Logging;
using WalletConnectUnity.Core;

namespace WalletConnectUnity.Nethereum
{
    public class WalletConnectUnityInterceptor : RequestInterceptor
    {
        public readonly IWalletConnect WalletConnectInstance;
        public readonly WalletConnectInterceptor WalletConnectInterceptor;

        public WalletConnectUnityInterceptor(IWalletConnect walletConnectInstance, WalletConnectInterceptor walletConnectInterceptor)
        {
            WalletConnectInstance = walletConnectInstance;
            WalletConnectInterceptor = walletConnectInterceptor;
        }

        public WalletConnectUnityInterceptor(IWalletConnect walletConnectInstance)
        {
            WalletConnectInstance = walletConnectInstance;
            WalletConnectInterceptor = new WalletConnectInterceptor(new WalletConnectServiceCore(walletConnectInstance.SignClient));
        }

        public override Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync,
            RpcRequest request,
            string route = null)
        {
            var result = WalletConnectInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, request, route);

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            try
            {
                if (WalletConnectInterceptor.SignMethods.Contains(request.Method))
                {
                    var activeSessionTopic = WalletConnectInstance.ActiveSession.Topic;
                    WalletConnectInstance.Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
                }
            }
            catch (Exception e)
            {
                WCLogger.LogError(e);
            }
#endif

            return result;
        }

        public override Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync,
            string method,
            string route = null, params object[] paramList)
        {
            var result = WalletConnectInterceptor.InterceptSendRequestAsync(interceptedSendRequestAsync, method, route, paramList);

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            try
            {
                if (WalletConnectInterceptor.SignMethods.Contains(method))
                {
                    var activeSessionTopic = WalletConnectInstance.ActiveSession.Topic;
                    WalletConnectInstance.Linker.OpenSessionRequestDeepLinkAfterMessageFromSession(activeSessionTopic);
                }
            }
            catch (Exception e)
            {
                WCLogger.LogError(e);
            }
#endif

            return result;
        }
    }
}