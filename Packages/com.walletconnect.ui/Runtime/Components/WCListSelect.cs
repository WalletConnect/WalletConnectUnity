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

        public void Initialize(in Params parameters)
        {
            _title.text = parameters.title;
            _onClick = parameters.onClick;

            parameters.remoteSprite.SubscribeImage(_icon);
        }

        // Called by Button component's UnityEvent
        public void OnClicked()
        {
            _onClick?.Invoke();
        }

        public struct Params
        {
            public RemoteSprite remoteSprite;
            public string title;
            public Action onClick;
        }
    }
}