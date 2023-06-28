using System;
using TMPro;
using UnityBinder;
using UnityEngine;
using UnityEngine.UI;
using WalletConnect;
using WalletConnectUnity.Demo.Utils.Images;

namespace WalletConnectUnity.Demo.SimpleSign
{
    public class BlockchainRowItem : BindableMonoBehavior
    {
        public Image chainImage;

        [BindComponentInChildren]
        private TextMeshProUGUI chainName;

        [BindComponentInChildren]
        private Outline Outline;

        [BindComponentInChildren]
        private Button _button;

        private Chain _blockchain;
        public bool SelectedState { get; private set; }

        public Chain Blockchain
        {
            get
            {
                return _blockchain;
            }
            set
            {
                _blockchain = value;
                BlockchainUpdated();
            }
        }

        private void Start()
        {
            if (Blockchain != null)
                BlockchainUpdated();
            
            _button.onClick.AddListener(ButtonClicked);
        }

        private void ButtonClicked()
        {
            SelectedState = !SelectedState;
            UpdateSelectedState();
        }

        private void UpdateSelectedState()
        {
            var alpha = SelectedState ? 0.4f : 0.5f;
            var size = SelectedState ? new Vector2(4f, -4f) : new Vector2(1.9f, -1.9f);
            Outline.effectColor = new Color(Blockchain.PrimaryColor.r, Blockchain.PrimaryColor.g, Blockchain.PrimaryColor.b, alpha);
            Outline.effectDistance = size;
        }

        private void BlockchainUpdated()
        {
            if (!gameObject.activeSelf)
                return;
            
            ImageHelper.With(() => chainImage).ShowUrl(Blockchain.IconUrl);
            chainName.text = Blockchain.Name;
            
            UpdateSelectedState();
        }
    }
}