using UnityEngine;
using System.Collections;

namespace WalletConnectUnity.Modal.Sample
{
    public class MobileUtilsScript : MonoBehaviour
    {
        private int FramesPerSec;
        private float frequency = 1.0f;
        private string fps;

        private void Start()
        {
            StartCoroutine(FPS());
        }

        private IEnumerator FPS()
        {
            for (;;)
            {
                // Capture frame-per-second
                var lastFrameCount = Time.frameCount;
                var lastTime = Time.realtimeSinceStartup;

                yield return new WaitForSeconds(frequency);

                var timeSpan = Time.realtimeSinceStartup - lastTime;
                var frameCount = Time.frameCount - lastFrameCount;

                fps = $"FPS: {Mathf.RoundToInt(frameCount / timeSpan)}";
            }
        }


        private void OnGUI()
        {
            var style = new GUIStyle
            {
                fontSize = 3 * 20,
                normal =
                {
                    textColor = Color.red
                }
            };

            // Draw the label with the new style
            GUI.Label(new Rect(Screen.width - 300, 30, 300, 20), fps, style);
        }
    }
}