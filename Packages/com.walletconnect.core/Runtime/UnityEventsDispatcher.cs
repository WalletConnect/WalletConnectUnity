using System;
using UnityEngine;
using System.Collections;
using WalletConnectUnity.Core.Utils;

namespace WalletConnectUnity.Core
{
    public sealed class UnityEventsDispatcher : MonoBehaviour
    {
        private Action _tick;

        private Coroutine _tickCoroutine;

        // TODO: Make this configurable
        private readonly IEnumerator _tickYieldInstruction = new WaitForNthFrame(3);

        private static UnityEventsDispatcher _instance;

        public static UnityEventsDispatcher Instance
        {
            get
            {
                if (_instance != null) return _instance;

                var go = new GameObject("[WalletConnect] UnityEventsDispatcher")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                DontDestroyOnLoad(go);

                _instance = go.AddComponent<UnityEventsDispatcher>();

                return _instance;
            }
        }

        private bool TickHasListeners => _tick?.GetInvocationList().Length > 0;

        /// <summary>
        /// Invoked every 3rd frame on the main thread.
        /// </summary>
        public event Action Tick
        {
            add
            {
                var wasEmpty = !TickHasListeners;

                _tick += value;

                if (!wasEmpty)
                    return;

                try
                {
                    _tickCoroutine = StartCoroutine(TickRoutine());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            remove
            {
                _tick -= value;

                if (!TickHasListeners)
                    StopCoroutine(_tickCoroutine);
            }
        }

        public static void InvokeNextFrame(Action action)
        {
            Instance.StartCoroutine(InvokeNextFrameRoutine(action));
        }

        private static IEnumerator InvokeNextFrameRoutine(Action action)
        {
            yield return null;
            action?.Invoke();
        }

        /// <summary>
        /// Invoked when the application is paused or resumed.
        /// </summary>
        public event Action<bool> ApplicationPause;

        /// <summary>
        /// Invoked when the application is quitting.
        /// </summary>
        public event Action ApplicationQuit;

        private IEnumerator TickRoutine()
        {
            while (enabled)
            {
                _tick?.Invoke();
                yield return _tickYieldInstruction;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            ApplicationPause?.Invoke(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            ApplicationQuit?.Invoke();
        }

        private void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}