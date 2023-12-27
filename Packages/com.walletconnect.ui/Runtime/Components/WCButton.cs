using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    [AddComponentMenu("WalletConnect/UI/WC Button")]
    public class WCButton : Button
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _border;
        [SerializeField] private Image _ring;

        [Space, SerializeField] private Config _normalConfig;
        [SerializeField] private Config _highlightedConfig;
        [SerializeField] private Config _selectedConfig;
        [SerializeField] private Config _pressedConfig;

        private bool _isPointerInside;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateVisuals(_normalConfig);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            // Unity automatically selects the button after pressed which we don't want when using a cursor
            var currentState = currentSelectionState;
            if (currentState is SelectionState.Highlighted) return;

            UpdateVisuals(_selectedConfig);

            base.OnSelect(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            UpdateVisuals(_highlightedConfig);
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            UpdateVisuals(_normalConfig);
            base.OnPointerExit(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            UpdateVisuals(_pressedConfig);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            UpdateVisuals(_isPointerInside ? _highlightedConfig : _normalConfig);
            base.OnPointerUp(eventData);
        }

        private void UpdateVisuals(Config config)
        {
            _background.color = config.backgroundColor;
            _border.color = config.borderColor;
            _ring.color = config.ringColor;
        }

        [Serializable]
        private struct Config
        {
            public Color backgroundColor;
            public Color borderColor;
            public Color ringColor;
        }
    }
}