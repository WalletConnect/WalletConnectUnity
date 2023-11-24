using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using WalletConnectUnity.Core.Networking;
using WalletConnectUnity.Core.Utils;

namespace WalletConnectUnity.UI
{
    public class WCTabsSwitch : MonoBehaviour
    {
        [SerializeField] private RectTransform _rootTransform;

        [SerializeField] private RectTransform _activeTabBackground;

        // [SerializeField] private RectTransform _mainBackground;
        [SerializeField] private float _transitionDuration = .25f;
        [SerializeField] private WCTabButton _tabButtonPrefab;

        [SerializeField] private ConnectionTypeTabConfigDictionary _tabs = new();

        private WCModal _modal;
        private TabConfig _activeTab;
        private bool _tabTransitionInProgress;
        private bool _initialized;

        // TODO: split into two classes!

        private void Initialize()
        {
            if (_initialized) return;

            foreach (var kvp in _tabs)
            {
                var wcTab = Instantiate(_tabButtonPrefab, _rootTransform);
                wcTab.Label.text = kvp.Value.label;
                wcTab.Clicked += (_, _) => OnTabClicked(kvp.Value);
                wcTab.Hide();

                kvp.Value.tabButtonButton = wcTab;
            }

            _initialized = true;
        }

        public void Enable(WCModal modal, Wallet wallet)
        {
            Assert.IsTrue(_tabs.Count != 0, "Tabs have not been created.");

            Initialize();

            _modal = modal;

            if (wallet.MobileLink != null)
                if (_tabs.TryGetValue(ConnectionType.Mobile, out var tabConfig))
                    tabConfig.tabButtonButton.Show();

            if (wallet.DesktopLink != null)
                if (_tabs.TryGetValue(ConnectionType.Desktop, out var tabConfig))
                    tabConfig.tabButtonButton.Show();

            // if (wallet.WebappLink != null)
            //     if (_tabs.TryGetValue(ConnectionType.Webapp, out var tabConfig))
            //         tabConfig.tabButton.Show();

            var visibleTabs = _tabs.Values
                .Where(t => t.tabButtonButton.IsVisible)
                .ToArray();

            if (visibleTabs.Length != 1)
            {
                gameObject.SetActive(true);

                // Resize main background to fit all visible tabs
                // StartCoroutine(ResizeMainBackground(visibleTabs));

                StartCoroutine(MakeTabActive(visibleTabs.First()));

                LayoutRebuilder.ForceRebuildLayoutImmediate(_rootTransform);
            }
            else
            {
                _activeTab = visibleTabs.First();
                _activeTab.tabView.SetActive(true);
            }
        }

        public void Disable()
        {
            foreach (var tabConfig in _tabs.Values)
            {
                tabConfig.tabButtonButton.Hide();
                tabConfig.tabView.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        private void OnTabClicked(TabConfig tab)
        {
            if (_tabTransitionInProgress) return;

            StartCoroutine(MakeTabActive(tab));
        }

        // private IEnumerator ResizeMainBackground(TabConfig[] visibleTabs)
        // {
        //     yield return null;
        //     var tabsWidth = visibleTabs.Sum(t => t.tabButtonButton.RootTransform.sizeDelta.x);
        //     _mainBackground.sizeDelta = new Vector2(tabsWidth + 14, _mainBackground.sizeDelta.y);
        // }

        private IEnumerator MakeTabActive(TabConfig tabConfig)
        {
            _tabTransitionInProgress = true;

            _activeTab.tabButtonButton.Deselect();

            // Adjust tabs background so that it fits the new active tab
            yield return AdjustBackgroundTransformRoutine(tabConfig.tabButtonButton);
            tabConfig.tabButtonButton.Select();

            // Disable old tab view, resize modal to fit new tab view, enable new tab view
            _activeTab.tabView.SetActive(false);
            var newTabViewRectTransform = (RectTransform)tabConfig.tabView.transform;
            var switchSizeDelta = ((RectTransform)transform).sizeDelta;
            yield return
                _modal.ResizeModalRoutine(newTabViewRectTransform.sizeDelta.y + switchSizeDelta.y + 8);

            tabConfig.tabView.SetActive(true);
            tabConfig.tabButtonButton.Select();
            _tabTransitionInProgress = false;
        }

        private IEnumerator AdjustBackgroundTransformRoutine(WCTabButton tabButton)
        {
            var initialPosition = _activeTabBackground.anchoredPosition;
            var targetPosition = tabButton.RootTransform.anchoredPosition;

            var initialSize = _activeTabBackground.sizeDelta;
            var targetSize = tabButton.RootTransform.sizeDelta;

            var timeElapsed = 0f;

            while (timeElapsed < _transitionDuration)
            {
                var t = timeElapsed / _transitionDuration;

                _activeTabBackground.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);
                _activeTabBackground.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);

                timeElapsed += Time.deltaTime;

                yield return null;
            }

            _activeTabBackground.anchoredPosition = targetPosition;
            _activeTabBackground.sizeDelta = targetSize;
        }

        [Serializable]
        private class TabConfig
        {
            public string label;
            public GameObject tabView;
            [HideInInspector] public WCTabButton tabButtonButton;
        }

        private enum ConnectionType
        {
            Mobile = 1,
            Desktop,
            Webapp
        }

        [Serializable]
        private class ConnectionTypeTabConfigDictionary : SerializableDictionary<ConnectionType, TabConfig>
        {
        }
    }
}