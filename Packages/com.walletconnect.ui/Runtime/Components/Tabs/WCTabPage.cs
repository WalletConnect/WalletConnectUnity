using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    [Serializable]
    public class WCTabPage
    {
        [field: SerializeField] public string TabButtonLabel { get; private set; }
        [field: SerializeField] public RectTransform PageTransform { get; private set; }
    }
}