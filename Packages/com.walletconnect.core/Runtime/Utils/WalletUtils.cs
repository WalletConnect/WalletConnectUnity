using WalletConnectUnity.Core.Networking;

namespace WalletConnectUnity.Core.Utils
{
    public class WalletUtils
    {
        public static bool IsWalletInstalled(Wallet wallet)
        {
            if (wallet.MobileLink == null || wallet.MobileLink.StartsWith("http"))
                return false;

            var link = wallet.MobileLink;

            if (!link.EndsWith("//"))
                link = $"{link}//";

            link = $"{link}wc";

            return Linker.CanOpenURL(link);
        }
    }
}