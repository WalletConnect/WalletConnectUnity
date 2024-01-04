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
        [SerializeField] private RectTransform _viewRoot;

        [Space]
        [SerializeField] private GameObject _walletIconRoot;

        [SerializeField] private GameObject _fallbackWalletIconRoot;

        [Space]
        [SerializeField] private RectTransform _qrCodeRoot;

        [SerializeField] private RawImage _qrCodeRawImage;
        [SerializeField] private float _qrCodePadding = 20f;

        public override async Task InitializeAsync(
            Wallet wallet,
            WCModal modal,
            RemoteSprite remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            WCLoadingAnimator.Instance.SubscribeGraphic(_qrCodeRawImage);

            ResizeQrCode();

            var validWallet = remoteWalletIcon != null;
            _walletIconRoot.SetActive(validWallet);
            _fallbackWalletIconRoot.SetActive(!validWallet);

            await base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);
            var texture = QRCode.EncodeTexture(Uri);

            WCLoadingAnimator.Instance.UnsubscribeGraphic(_qrCodeRawImage);

            _qrCodeRawImage.texture = texture;
        }

        private void ResizeQrCode()
        {
            // Stretch the QR code to the full width of the view, but keep the aspect ratio and padding
            var qrCodeSize = _viewRoot.rect.width - _qrCodePadding * 2;
            var oldQrCodeHeight = _qrCodeRoot.sizeDelta.y;
            var newSizeDelta = new Vector2(qrCodeSize, qrCodeSize);
            _qrCodeRoot.sizeDelta = newSizeDelta;

            // Resize the container to fit the QR code
            var heightDelta = newSizeDelta.y - oldQrCodeHeight;
            (transform as RectTransform)!.sizeDelta += new Vector2(0, heightDelta);
        }

        public override void Disable()
        {
            base.Disable();
            _qrCodeRawImage.texture = null;
        }
    }
}