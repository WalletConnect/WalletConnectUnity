using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    public class WCListSelect : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;

        private WCModalView _targetView;
        private object _targetViewParameters;
        private Action _onClick;
        private RemoteSprite _remoteSprite;

        private bool _initialized;

        public void Initialize(in Params parameters)
        {
            if (_initialized)
            {
                Reset();
            }

            _title.text = parameters.title;
            _onClick = parameters.onClick;
            _remoteSprite = parameters.remoteSprite;

            if (parameters.remoteSprite == null)
            {
                _icon.sprite = parameters.sprite;
                _icon.color = Color.white;
            }
            else
            {
                parameters.remoteSprite.SubscribeImage(_icon);
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            _initialized = true;
        }

        // Called by Button component's UnityEvent
        public void OnClicked()
        {
            _onClick?.Invoke();
        }

        public void Reset()
        {
            _remoteSprite?.UnsubscribeImage(_icon);
            _icon.color = new Color(1, 1, 1, 0.1f);
            _title.text = string.Empty;
        }

        public struct Params
        {
            public RemoteSprite remoteSprite;
            public Sprite sprite; // used if remoteSprite is null
            public string title;
            public Action onClick;
        }
    }
}