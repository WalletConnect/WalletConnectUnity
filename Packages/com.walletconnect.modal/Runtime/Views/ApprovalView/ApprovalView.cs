using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class ApprovalView : WCModalView
    {
        [Space, SerializeField] private ApprovalViewTabsController _tabsController;

        [Space, SerializeField] private ApprovalViewPageBase _qrCodePage;
        [SerializeField] private ApprovalViewPageBase _deepLinkPage;

        private CancellationTokenSource _viewCts;
        private Wallet _walletData;

        private void Awake()
        {
            _tabsController.Initialize();
        }

        public override async void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            var parameters = (Params)options;

            Assert.IsNotNull(parameters, "QRCodeView options is not Params type or null.");

            _walletData = parameters.walletData;
            _viewCts = new CancellationTokenSource();

            base.Show(modal, effectCoroutine, options);

            if (parameters.walletData == null)
            {
                await _qrCodePage.InitializeAsync(
                    parameters.walletData,
                    modal,
                    parameters.walletIconRemoteSprite,
                    _viewCts.Token);
            }
            else
            {
                await Task.WhenAll(
                    _qrCodePage.InitializeAsync(
                        parameters.walletData,
                        modal,
                        parameters.walletIconRemoteSprite,
                        _viewCts.Token),
                    _deepLinkPage.InitializeAsync(
                        parameters.walletData,
                        modal,
                        parameters.walletIconRemoteSprite,
                        _viewCts.Token)
                );
            }

            _tabsController.Enable(parameters.walletData);

            await WaitForUserConnectionAsync(_viewCts.Token);
        }

        public override void Hide()
        {
            base.Hide();

            _viewCts.Cancel();

            _qrCodePage.Disable();
            _deepLinkPage.Disable();
            _tabsController.Disable();
        }

        public override string GetTitle()
        {
            return _walletData == null ? base.GetTitle() : _walletData.Name;
        }

        private async Task WaitForUserConnectionAsync(CancellationToken cancellationToken)
        {
            var connectedData = await WalletConnectModal.ConnectionController.GetConnectionDataAsync(cancellationToken);

            try
            {
                // Wait for the approval task or cancellation (if the QRCodeView is closed)
                await Task.WhenAny(connectedData.Approval, Task.Delay(Timeout.Infinite, cancellationToken));

                if (_viewCts.Token.IsCancellationRequested)
                    return;

                if (connectedData.Approval.IsCompletedSuccessfully)
                {
                    _ = await connectedData.Approval;
                    // UpdateUI(sessionData);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public class Params
        {
            public RemoteSprite walletIconRemoteSprite;
            public Wallet walletData;
        }
    }
}