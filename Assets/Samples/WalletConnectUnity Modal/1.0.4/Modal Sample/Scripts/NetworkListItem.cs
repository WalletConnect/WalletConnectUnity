using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WalletConnectUnity.UI;

namespace WalletConnectUnity.Modal.Sample
{
    public class NetworkListItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Outline _outline;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;

        public event Action<Chain, bool> Selected;

        private Chain _chain;
        private bool _selected;
        private RemoteSprite _remoteSprite;

        public void Initialize(Chain chain, Action<Chain, bool> onSelected)
        {
            _remoteSprite = RemoteSprite.Create(chain.IconUrl);
            _remoteSprite.SubscribeImage(_iconImage);

            _chain = chain;
            _nameText.text = chain.Name;
            _outline.effectColor = chain.PrimaryColor;

            Selected += onSelected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _selected = !_selected;
            UpdateSelectedState();

            Selected?.Invoke(_chain, _selected);
        }

        private void UpdateSelectedState()
        {
            _outline.enabled = _selected;
        }
    }
}