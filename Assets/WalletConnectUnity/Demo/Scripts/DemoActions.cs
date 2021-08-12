using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Core.Models.Ethereum;
using WalletConnectSharp.Core.Models.Ethereum.Types;
using WalletConnectSharp.Unity;
using WalletConnectUnity.Demo.Scripts;

public class DemoActions : MonoBehaviour
{
    public Text resultText;
    public Text accountText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        accountText.text = "Connected:\n" + WalletConnect.ActiveSession.Accounts[0];
    }

    public async void PersonalSign()
    {
        var address = WalletConnect.ActiveSession.Accounts[0];

        var results = await WalletConnect.ActiveSession.EthPersonalSign(address, "This is a test!");

        resultText.text = results;
        resultText.gameObject.SetActive(true);
    }
    
    public async void SendTransaction()
    {
        var address = WalletConnect.ActiveSession.Accounts[0];
        var transaction = new TransactionData()
        {
            data = "0x",
            from = address,
            to = address,
            gas = "21000",
            value = "0",
            chainId = 2,
        };

        var results = await WalletConnect.ActiveSession.EthSendTransaction(transaction);

        resultText.text = results;
        resultText.gameObject.SetActive(true);
    }
    
    public async void SignTransaction()
    {
        var address = WalletConnect.ActiveSession.Accounts[0];
        var transaction = new TransactionData()
        {
            data = "0x",
            from = address,
            to = address,
            gas = "21000",
            value = "0",
            chainId = 2,
            nonce = "0",
            gasPrice = "50000000000"
        };

        var results = await WalletConnect.ActiveSession.EthSignTransaction(transaction);

        resultText.text = results;
        resultText.gameObject.SetActive(true);
    }
    
    public async void SignTypedData()
    {
        var address = WalletConnect.ActiveSession.Accounts[0];

        var results = await WalletConnect.ActiveSession.EthSignTypedData(address, DemoSignTypedData.ExampleData, DemoSignTypedData.Eip712Domain);

        resultText.text = results;
        resultText.gameObject.SetActive(true);
    }
}
