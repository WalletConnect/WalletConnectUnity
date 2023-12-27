using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class QRCodePage : ApprovalViewPageBase
    {
        [SerializeField] private RawImage _qrCodeRawImage;

        public override async Task InitializeAsync(
            Wallet wallet,
            WCModal modal,
            RemoteSprite remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            WCLoadingAnimator.Instance.SubscribeGraphic(_qrCodeRawImage);

            await base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);
            var texture = QRCode.EncodeTexture(Uri);

            WCLoadingAnimator.Instance.UnsubscribeGraphic(_qrCodeRawImage);

            _qrCodeRawImage.texture = texture;
        }

        public override void Disable()
        {
            base.Disable();
            _qrCodeRawImage.texture = null;
        }
    }
}