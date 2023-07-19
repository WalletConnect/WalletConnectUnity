using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using TMPro;
using UnityBinder;
using UnityEngine;
using WalletConnect;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectSharp.Sign.Models;
using WalletConnectUnity.Demo.Utils;

public class DemoDapp : BindableMonoBehavior
{
    public const string EthereumChainId = "eip155";

    [RpcMethod("eth_getBalance"), RpcRequestOptions(Clock.ONE_MINUTE, 99998)]
    public class EthGetBalance : List<string>
    {
        public EthGetBalance(string address, BigInteger? blockNumber = null) : base(new[]
        {
            address,
            blockNumber != null ? blockNumber.ToString() : "latest"
        })
        {
        }
    }

    public class Transaction
    {
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("to")]
        public string To { get; set; }
        
        [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
        public string Gas { get; set; }
        
        [JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
        public string GasPrice { get; set; }
        
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; } = "0x";
    }
    
    [RpcMethod("eth_sendTransaction"), RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
    public class EthSendTransaction : List<Transaction>
    {
        public EthSendTransaction(params Transaction[] transactions) : base(transactions)
        {
        }
    }

    [Inject]
    private WCSignClient _wc;

    public TextMeshProUGUI balanceText;

    public TextMeshProUGUI addressText;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CheckBalanceTask());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public (SessionStruct, string, string) GetCurrentAddress()
    {
        var currentSession = _wc.Session.Get(_wc.Session.Keys[0]);

        var defaultChain = currentSession.Namespaces.Keys.FirstOrDefault();
            
        if (string.IsNullOrWhiteSpace(defaultChain))
            return (default, null, null);

        var defaultNamespace = currentSession.Namespaces[defaultChain];

        if (defaultNamespace.Accounts.Length == 0)
            return (default, null, null);
            
        var fullAddress = defaultNamespace.Accounts[0];
        var addressParts = fullAddress.Split(":");
            
        var address = addressParts[2];
        var chainId = string.Join(':', addressParts.Take(2));

        return (currentSession, address, chainId);
    }

    public async void SendSignRequest()
    {
        var (session, address, chainId) = GetCurrentAddress();
        if (string.IsNullOrWhiteSpace(address))
            return;
        
        var request = new EthSendTransaction(new Transaction()
        {
            From = address,
            To = address,
            Value = "0"
        });

        var result = await _wc.Request<EthSendTransaction, string>(session.Topic, request, chainId);
        
        Debug.Log("Got result from request: " + result);
    }

    private IEnumerator CheckBalanceTask()
    {
        while (this != null)
        {
            yield return new WaitForSeconds(5);
            
            // First check to see if we're connected
            if (!_wc.Core.Relayer.Connected)
                continue;

            if (_wc.Session.Length == 0)
                continue;

            var (_, address, _) = GetCurrentAddress();
            if (string.IsNullOrWhiteSpace(address))
                continue;

            addressText.text = $"{string.Concat(address.Take(6))}...{string.Concat(address.TakeLast(4))}";
            
            // Now build the eth_getBalance request
            /*var request = new EthGetBalance(address);

            var resultTask = _wc.Request<EthGetBalance, BigInteger>(currentSession.Topic, request, chainId);

            yield return new WaitForTaskResult<BigInteger>(resultTask);

            var result = resultTask.Result;
            
            // Convert to correct units
            var finalResult = result / BigInteger.Pow(10, 18);

            balanceText.text = $"Balance: {finalResult} ETH";*/
        }
    }
}
