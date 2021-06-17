using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WalletConnectSharp.Unity.Utils;

public class NFTScreen : BindableMonoBehavior
{
    [Inject]
    private NFTTokenList _tokenList;

    public GameObject connectScreen;
    
    public void OnWalletConnected()
    {
        connectScreen.SetActive(false);
        gameObject.SetActive(true);
        
        _tokenList.RebuildTokenList();
    }
}
