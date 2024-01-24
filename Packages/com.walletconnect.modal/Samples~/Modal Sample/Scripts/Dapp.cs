using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectSharp.Sign.Models;
using WalletConnectSharp.Sign.Models.Engine;
using WalletConnectUnity.Core;

namespace WalletConnectUnity.Modal.Sample
{
    public class Dapp : MonoBehaviour
    {
        [Space, SerializeField] private NetworkListItem _networkListItemPrefab;

        [Space, SerializeField] private Transform _networkListContainer;
        [SerializeField] private Button _continueButton;

        [Space, SerializeField] private GameObject _dappButtons;
        [SerializeField] private GameObject _networkList;

        private readonly HashSet<Chain> _selectedChains = new();

        private void Start()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            // When WalletConnectModal is ready, enable buttons and subscribe to other events.
            // WalletConnectModal.SignClient can be null if WalletConnectModal is not ready.
            WalletConnectModal.Ready += (sender, args) =>
            {
                // SessionResumed is true if Modal resumed session from storage
                if (args.SessionResumed)
                {
                    EnableDappButtons();
                }
                else
                {
                    EnableNetworksList();
                }

                // Invoked after wallet connected
                WalletConnect.Instance.ActiveSessionChanged += (_, @struct) =>
                {
                    if (string.IsNullOrEmpty(@struct.Topic))
                        return;
                    
                    Debug.Log($"[WalletConnectModalSample] Session connected. Topic: {@struct.Topic}");
                    EnableDappButtons();
                };

                // Invoked after wallet disconnected
                WalletConnect.Instance.SessionDisconnected += (_, _) =>
                {
                    Debug.Log($"[WalletConnectModalSample] Session deleted.");
                    EnableNetworksList();
                };
            };
        }

        private void EnableDappButtons()
        {
            _networkList.SetActive(false);
            _dappButtons.SetActive(true);
        }

        private void EnableNetworksList()
        {
            _dappButtons.SetActive(false);
            _networkList.SetActive(true);

            if (_networkListContainer.childCount == 0)
            {
                foreach (var chain in Chain.All)
                {
                    var item = Instantiate(_networkListItemPrefab, _networkListContainer);
                    item.Initialize(chain, OnNetworkSelected);
                }
            }
        }

        private void OnNetworkSelected(Chain chain, bool selected)
        {
            if (selected)
                _selectedChains.Add(chain);
            else
                _selectedChains.Remove(chain);

            _continueButton.interactable = _selectedChains.Count != 0;
        }

        public void OnContinueButton()
        {
            var options = new WalletConnectModalOptions
            {
                ConnectOptions = BuildConnectOptions()
            };

            WalletConnectModal.Open(options);
        }

        private ConnectOptions BuildConnectOptions()
        {
            var requiredNamespaces = new RequiredNamespaces();

            if (_selectedChains.Any(c => c.ChainNamespace == Chain.EvmNamespace))
            {
                var eipChains = _selectedChains.Where(c => c.ChainNamespace == Chain.EvmNamespace);

                var methods = new[]
                {
                    "eth_sendTransaction",
                    "personal_sign",
                };

                var events = new[]
                {
                    "chainChanged", "accountsChanged"
                };

                var chainIds = eipChains.Select(c => c.FullChainId).ToArray();

                requiredNamespaces.Add(Chain.EvmNamespace, new ProposedNamespace()
                {
                    Chains = chainIds,
                    Events = events,
                    Methods = methods
                });
            }
            else
            {
                throw new Exception("No EVM chains selected.");
            }

            return new ConnectOptions
            {
                RequiredNamespaces = requiredNamespaces
            };
        }
    }
}