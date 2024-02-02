using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace WalletConnectUnity.UI
{
    public class WCLoadingAnimator : MonoBehaviour
    {
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;
        [SerializeField] private float _speed = 1f;
        [SerializeField] private AnimationCurve _lerpCurve;

        private readonly HashSet<Graphic> _subscribedGraphics = new();
        private readonly HashSet<VisualElement> _subscribedVisualElements = new();

        private Color _currentColor;
        private bool _isAnimating;

        public static WCLoadingAnimator Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Subscribe<T>(T element) where T : class
        {
            if (!_isAnimating)
                StartCoroutine(AnimateColorRoutine());

            switch (element)
            {
                case Graphic graphic:
                    _subscribedGraphics.Add(graphic);
                    break;
                case VisualElement visualElement:
                    _subscribedVisualElements.Add(visualElement);
                    break;
            }
        }
        
        public void Unsubscribe<T>(T element) where T : class
        {
            switch (element)
            {
                case Graphic graphic:
                    _subscribedGraphics.Remove(graphic);
                    break;
                case VisualElement visualElement:
                    _subscribedVisualElements.Remove(visualElement);
                    break;
            }

            if (_subscribedGraphics.Count == 0 && _subscribedVisualElements.Count == 0)
                _isAnimating = false;
        }
        
        public void SubscribeGraphic(Graphic graphic)
        {
            if (!_isAnimating)
                StartCoroutine(AnimateColorRoutine());

            _subscribedGraphics.Add(graphic);
        }

        private IEnumerator AnimateColorRoutine()
        {
            _isAnimating = true;
            var t = 0f;
            while (_isAnimating)
            {
                _currentColor = Color.Lerp(_colorA, _colorB, _lerpCurve.Evaluate(t));
                t += Time.deltaTime * _speed;
                if (t > 1f)
                {
                    t = 0f;
                    (_colorA, _colorB) = (_colorB, _colorA);
                }

                foreach (var graphic in _subscribedGraphics)
                    graphic.color = _currentColor;

                foreach (var visualElement in _subscribedVisualElements)
                    visualElement.style.backgroundColor = _currentColor;

                yield return null;
            }
        }
    }
}