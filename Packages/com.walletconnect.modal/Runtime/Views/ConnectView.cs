using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Views
{
    public class ConnectView : WCModalView
    {
        [SerializeField] private RectTransform _listRootTransform;
        [SerializeField] private ApprovalView _approvalView;
        [SerializeField] private WalletSearchView _walletSearchView;
        [SerializeField] private WCListSelect _listSelectPrefab;

        [SerializeField, Space] private ushort _itemsCounts = 5;

        [SerializeField, Space] private Sprite _wcLogoSprite;
        [SerializeField] private Sprite _allWalletsSprite;

        private readonly List<WCListSelect> _listItems = new(5);

        public override void Show(WCModal modal, IEnumerator effectCoroutine, object options = null)
        {
            if (_listItems.Count == 0)
                for (var i = 0; i < _itemsCounts; i++)
                    _listItems.Add(Instantiate(_listSelectPrefab, _listRootTransform));


            StartCoroutine(RefreshWalletsCoroutine());

            base.Show(modal, effectCoroutine, options);
        }

        public override void Hide()
        {
            base.Hide();
            StopAllCoroutines();
        }

        private IEnumerator RefreshWalletsCoroutine()
        {
            var index = 0;
            var totalWallets = _itemsCounts - 1; // the last item is "All Wallets" button

#if !UNITY_ANDROID && !UNITY_IOS
            _listItems[index].Initialize(new WCListSelect.Params
            {
                title = "WalletConnect",
                sprite = _wcLogoSprite,
                onClick = () =>
                {
                    parentModal.OpenView(_approvalView, parameters: new ApprovalView.Params());
                }
                // TODO: add "qr code" tag
            });

            index++;
#endif

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
                    }
                });
            }


            // "All Wallets" button
            _listItems[totalWallets].Initialize(new WCListSelect.Params
            {
                title = "All Wallets",
                sprite = _allWalletsSprite,
                onClick = () =>
                {
                    parentModal.OpenView(_walletSearchView);
                }
            });
        }
    }
}