using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    [AddComponentMenu("WalletConnect/UI/WC Modal")]
    public class WCModal : MonoBehaviour
    {
        [Header("Scene References")] [SerializeField]
        private Canvas _rootCanvas;

        [SerializeField] private Canvas _globalBackgroundCanvas;

        [SerializeField] private RectTransform _rootTransform;

        [field: SerializeField] public WCModalHeader Header { get; private set; }


        [Header("Settings")] [SerializeField] private TransformConfig _mobileTransformConfig;
        [SerializeField] private TransformConfig _desktopTransformConfig;

        public bool IsOpen => _rootCanvas.enabled;

        private readonly Stack<WCModalView> _viewsStack = new();
        private bool _hasGlobalBackground;

        private void Awake()
        {
            _hasGlobalBackground = _globalBackgroundCanvas != null;
        }

        public void OpenView(WCModalView view, string modalTitle, WCModal modal = null, object parameters = null)
        {
            if (_viewsStack.Count == 0)
                EnableModal();

            if (_viewsStack.Count > 0)
                _viewsStack.Peek().Hide();

            modal ??= this;

            _viewsStack.Push(view);
            var resizeCoroutine = ResizeModalRoutine(CalculateModalHeight(view.GetRequiredHeight()));
            view.Show(modal, resizeCoroutine, parameters);

            Header.Title = modalTitle;
            Header.LeftButtonActive = _viewsStack.Count > 1;
        }

        public void CloseView()
        {
            if (_viewsStack.Count <= 0) return;

            var currentView = _viewsStack.Pop();
            currentView.Hide();

            if (_viewsStack.Count == 1)
            {
                Header.Title = "WalletConnect";
                Header.LeftButtonActive = false;
            }

            if (_viewsStack.Count > 0)
            {
                var nextView = _viewsStack.Peek();
                var resizeCoroutine = ResizeModalRoutine(CalculateModalHeight(nextView.GetRequiredHeight()));
                nextView.Show(this, resizeCoroutine);
            }
            else
            {
                DisableModal();
            }
        }

        public void CloseModal()
        {
            if (_viewsStack.Count > 0)
            {
                var lastView = _viewsStack.Pop();
                lastView.Hide();
            }

            _viewsStack.Clear();
            DisableModal();
        }

        private IEnumerator ResizeModalRoutine(float targetHeight)
        {
            var originalHeight = _rootTransform.sizeDelta.y;
            var elapsedTime = 0f;
            var duration = .25f; // TODO: serialize this

            while (elapsedTime < duration)
            {
                var lerp = Mathf.Lerp(originalHeight, targetHeight, elapsedTime / duration);
                _rootTransform.sizeDelta = new Vector2(_rootTransform.sizeDelta.x, lerp);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _rootTransform.sizeDelta = new Vector2(_rootTransform.sizeDelta.x, targetHeight);
        }

        private float CalculateModalHeight(float viewHeight)
        {
            return viewHeight + Header.Height;
        }

        private void EnableModal()
        {
            ApplyTransformConfig(
#if UNITY_ANDROID || UNITY_IOS
                _mobileTransformConfig
#else
                _desktopTransformConfig
#endif
            );

            _rootCanvas.enabled = true;

            Header.Title = "WalletConnect";
            Header.LeftButtonActive = false;

            if (_hasGlobalBackground)
                _globalBackgroundCanvas.enabled = true;
        }

        private void DisableModal()
        {
            _rootCanvas.enabled = false;

            if (_hasGlobalBackground)
                _globalBackgroundCanvas.enabled = false;
        }

        private void ApplyTransformConfig(TransformConfig config)
        {
            _rootTransform.anchorMin = config.anchorMin;
            _rootTransform.anchorMax = config.anchorMax;
            _rootTransform.sizeDelta = config.sizeDelta;
            _rootTransform.pivot = config.pivot;
        }

        [Serializable]
        private struct TransformConfig
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 sizeDelta;
            public Vector2 pivot;
        }
    }
}