using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class DeepLinkPage : ApprovalViewPageBase
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private string _titleTextFormat = "Continue in {0}";

        public override Task InitializeAsync(
            Wallet wallet,
            WCModal modal,
            RemoteSprite remoteWalletIcon,
            CancellationToken cancellationToken)
        {
            _titleText.text = string.Format(_titleTextFormat, wallet.Name);
            return base.InitializeAsync(wallet, modal, remoteWalletIcon, cancellationToken);
        }

        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(Uri)) return;

            StartCoroutine(OpenSessionProposalDeepLinkRoutine());
        }
        
        private IEnumerator OpenSessionProposalDeepLinkRoutine()
        {
            // Skip one frame to not block the UI rendering
            yield return null;
            Linker.OpenSessionProposalDeepLink(Uri, Wallet);
        }
    }
}