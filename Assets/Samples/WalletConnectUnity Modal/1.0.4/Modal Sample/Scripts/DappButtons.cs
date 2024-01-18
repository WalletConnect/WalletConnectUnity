using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using WalletConnectSharp.Common.Model.Errors;
using WalletConnectSharp.Common.Utils;
using WalletConnectSharp.Network.Models;
using WalletConnectUnity.Core;

namespace WalletConnectUnity.Modal.Sample
{
    public class DappButtons : MonoBehaviour
    {
        [SerializeField] private Button _disconnectButton;
        [SerializeField] private Button _personalSignButton;
        [SerializeField] private Button _transactionButton;

        private void Awake()
        {
            WalletConnect.Instance.ActiveSessionChanged += (_, @struct) =>
            {
                _disconnectButton.interactable = true;
                _personalSignButton.interactable = true;
                _transactionButton.interactable = true;
            };
        }

        public void OnDisconnectButton()
        {
            Debug.Log("[WalletConnectModalSample] OnDisconnectButton");

            _disconnectButton.interactable = false;
            _personalSignButton.interactable = false;
            _transactionButton.interactable = false;

            WalletConnectModal.Disconnect();
        }

        public async void OnPersonalSignButton()
        {
            Debug.Log("[WalletConnectModalSample] OnPersonalSignButton");

            var session = WalletConnect.Instance.ActiveSession;
            var sessionNamespace = session.Namespaces;
            var address = WalletConnect.Instance.ActiveSession.CurrentAddress(sessionNamespace.Keys.FirstOrDefault())
                .Address;

            var data = new PersonalSign("Hello world!", address);

            try
            {
                var result = await WalletConnect.Instance.RequestAsync<PersonalSign, string>(data);
                Notification.ShowMessage(
                    $"Received response.\nThis app cannot validate signatures yet.\n\nResponse: {result}");
            }
            catch (WalletConnectException e)
            {
                Notification.ShowMessage($"Personal Sign Request Error: {e.Message}");
                Debug.Log($"[WalletConnectModalSample] Personal Sign Error: {e.Message}");
            }
        }

        public async void OnTransactionButton()
        {
            Debug.Log("[WalletConnectModalSample] OnTransactionButton");

            var session = WalletConnect.Instance.ActiveSession;
            var sessionNamespace = session.Namespaces;
            var address = WalletConnect.Instance.ActiveSession.CurrentAddress(sessionNamespace.Keys.FirstOrDefault())
                .Address;

            var request = new EthSendTransaction(new Transaction()
            {
                From = address,
                To = address,
                Value = "0"
            });

            var signClient = WalletConnect.Instance.SignClient;
            try
            {
                var result = await signClient.Request<EthSendTransaction, string>(request);
                // var result = await WalletConnect.Instance.RequestAsync<EthSendTransaction, string>(request);
                Notification.ShowMessage($"Done!\nResponse: {result}");
            }
            catch (WalletConnectException e)
            {
                Notification.ShowMessage($"Transaction Request Error: {e.Message}");
                Debug.Log($"[WalletConnectModalSample] Transaction Error: {e.Message}");
            }
        }

        [RpcMethod("personal_sign")]
        [RpcRequestOptions(Clock.ONE_MINUTE, 99998)]
        public class PersonalSign : List<string>
        {
            public PersonalSign(string hexUtf8, string account) : base(new[] { hexUtf8, account })
            {
            }

            [Preserve]
            public PersonalSign()
            {
            }
        }

        public class Transaction
        {
            [JsonProperty("from")] public string From { get; set; }

            [JsonProperty("to")] public string To { get; set; }

            [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
            public string Gas { get; set; }

            [JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
            public string GasPrice { get; set; }

            [JsonProperty("value")] public string Value { get; set; }

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public string Data { get; set; } = "0x";
        }

        [RpcMethod("eth_sendTransaction"), RpcRequestOptions(Clock.ONE_MINUTE, 99997)]
        public class EthSendTransaction : List<Transaction>
        {
            public EthSendTransaction(params Transaction[] transactions) : base(transactions)
            {
            }

            [Preserve]
            public EthSendTransaction()
            {
            }
        }
    }
}