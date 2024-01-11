using System;
using UnityEngine;

namespace WalletConnectUnity.UI
{
    public class OrientationTracker : MonoBehaviour
    {
        public static event EventHandler<ScreenOrientation> OrientationChanged;

        private ScreenOrientation _lastOrientation;

        private static OrientationTracker _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                Debug.LogError("OrientationTracker already exists. Destroying new instance.", gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _lastOrientation = Screen.orientation;
        }

        private void FixedUpdate()
        {
            var orientation = Screen.orientation;

            if (orientation != _lastOrientation)
            {
                _lastOrientation = orientation;
                OrientationChanged?.Invoke(this, orientation);
            }
        }

        public static void Enable() => _instance.enabled = true;

        public static void Disable() => _instance.enabled = false;
    }
}