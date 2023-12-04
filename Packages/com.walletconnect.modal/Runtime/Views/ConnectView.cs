using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class ConnectView : WCModalView
    {
        [SerializeField] private RectTransform _listRootTransform;
        [SerializeField] private ApprovalView _approvalView;
        [SerializeField] private WalletSearchView _walletSearchView;

        [Header("QR Code")] [SerializeField] private bool _showQrCodeOnDesktop = true;
        [SerializeField] private RectTransform _qrCodeArea;
        [SerializeField] private GameObject _qrWalletIcon;
        [SerializeField] private RawImage _qrCodeRawImage;
        [SerializeField] private Color _qrCodeWaitColor = Color.gray;

        [Header("Settings"), SerializeField] private ushort _itemsCounts = 5;

        [Header("Asset References"), SerializeField]
        private Sprite _wcLogoSprite;

        [SerializeField] private Sprite _allWalletsSprite;
        [SerializeField] private Sprite _copyIconSprite;
        [SerializeField] private WCListSelect _listSelectPrefab;

        private readonly List<WCListSelect> _listItems = new(5);

        protected string Uri { get; private set; }

        private void Awake()
        {
#if (!UNITY_IOS && !UNITY_ANDROID)
            // Resize to fit the QR code
            if (_showQrCodeOnDesktop)
            {
                _qrCodeArea.gameObject.SetActive(true);

                var sizeDelta = rootTransform.sizeDelta;
                var newY = sizeDelta.y + _qrCodeArea.rect.height;
                rootTransform.sizeDelta = new Vector2(sizeDelta.x, newY);
            }
#endif
        }

        public override async void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            if (_listItems.Count == 0)
                for (var i = 0; i < _itemsCounts; i++)
                    _listItems.Add(Instantiate(_listSelectPrefab, _listRootTransform));

            StartCoroutine(RefreshWalletsCoroutine());

            modal.Header.SetCustomLeftButton(_copyIconSprite, OnCopyLinkClick);

            base.Show(modal, effectCoroutine, options);

#if (!UNITY_IOS && !UNITY_ANDROID)
            await ShowQrCodeAndCopyButtonAsync();
#endif
        }

        public override void Hide()
        {
            base.Hide();
            StopAllCoroutines();

            parentModal.Header.RemoveCustomLeftButton();

            _qrCodeRawImage.texture = null;
            _qrCodeRawImage.color = _qrCodeWaitColor;
            _qrWalletIcon.SetActive(false);
        }

        private void OnCopyLinkClick()
        {
            parentModal.Header.Snackbar.Show(WCSnackbar.Type.Success, "Link copied");
            GUIUtility.systemCopyBuffer = Uri;
        }

        private IEnumerator RefreshWalletsCoroutine()
        {
            var index = 0;
            var totalWallets = _itemsCounts - 1; // the last item is "All Wallets" button

            // TODO: show one recent wallet

            // ReSharper disable once UselessBinaryOperation
            var walletsToLoad = totalWallets - index;
            using var uwr = WalletConnectModal.WalletsRequestsFactory.GetWallets(1, walletsToLoad);

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[WalletConnectUnity] Failed to get wallets: {uwr.error}", this);
                yield break;
            }

            var response = JsonConvert.DeserializeObject<GetWalletsResponse>(uwr.downloadHandler.text);

            for (var i = index; i < totalWallets; i++)
            {
                var wallet = response.Data[i - index];

                var remoteSprite = RemoteSprite.Create($"https://api.web3modal.com/getWalletImage/{wallet.ImageId}");
                _listItems[i].Initialize(new WCListSelect.Params
                {
                    title = wallet.Name,
                    remoteSprite = remoteSprite,
                    onClick = () =>
                    {
                        parentModal.OpenView(_approvalView, parameters: new ApprovalView.Params
                        {
                            walletIconRemoteSprite = remoteSprite,
                            walletData = wallet
                        });
                    },
                });
            }


            // "All Wallets" button
            _listItems[totalWallets].Initialize(new WCListSelect.Params
            {
                title = "All wallets",
                sprite = _allWalletsSprite,
                onClick = () => { parentModal.OpenView(_walletSearchView); },
                borderColor = new Color(0.2784f, 0.6313f, 1, 0.08f)
            });
        }

        private async Task ShowQrCodeAndCopyButtonAsync()
        {
            var connectedData = await WalletConnectModal.ConnectionController.GetConnectionDataAsync();
            Uri = connectedData.Uri;

            var texture = QRCode.EncodeTexture(Uri);
            _qrCodeRawImage.texture = texture;
            _qrCodeRawImage.color = Color.white;

            _qrWalletIcon.SetActive(true);
        }
    }
}