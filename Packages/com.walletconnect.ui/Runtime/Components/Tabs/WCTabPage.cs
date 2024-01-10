using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    [Serializable]
    public class WCTabPage : MonoBehaviour
    {
        [field: SerializeField] public string TabButtonLabel { get; private set; }
        [field: SerializeField] public RectTransform PageTransform { get; private set; }

        public virtual float GetPageHeight() => PageTransform.sizeDelta.y;

        public virtual void Enable() => gameObject.SetActive(true);

        public virtual void Disable() => gameObject.SetActive(false);
    }
}