using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WalletConnectUnity.Core.Tests
{
    public class InitializationTests
    {
        [Test]
        public async Task InitializeAsync_WhenNotInitialized_BecomesInitialized()
        {
            using var wc = new WalletConnect();

            Assert.IsFalse(wc.IsInitialized);

            await wc.InitializeAsync();

            Assert.IsTrue(wc.IsInitialized);
        }

        [Test]
        public async Task InitializeAsync_WhenCalledInParallel_LogsErrorAndBecomesInitialized()
        {
            using var wc = new WalletConnect();

            Assert.IsFalse(wc.IsInitialized);

            var task1 = wc.InitializeAsync();
            var task2 = wc.InitializeAsync();

            await Task.WhenAll(task1, task2);

            LogAssert.Expect(LogType.Error, "[WalletConnectUnity] Already initialized");
            Assert.IsTrue(wc.IsInitialized);
        }
    }
}