using UnityEngine;

namespace WalletConnect.Unity.Utils
{
    public class WaitForNthFrame : CustomYieldInstruction
    {
        private readonly int _framesToWait;

        public override bool keepWaiting => Time.frameCount % _framesToWait != 0;

        public WaitForNthFrame(int n)
        {
            _framesToWait = n;
        }
    }
}