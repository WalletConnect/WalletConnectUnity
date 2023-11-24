using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    [AddComponentMenu("WalletConnect/UI/WC Button")]
    public class WCButton : Button
    {
        public override void OnSelect(BaseEventData eventData)
        {
            // Unity automatically selects the button after pressed which we don't want when using a cursor
            var currentState = currentSelectionState;
            if (currentState is SelectionState.Highlighted) return;
            
            base.OnSelect(eventData);
        }
    }
}