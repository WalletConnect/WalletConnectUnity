using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine.TestTools;
using WalletConnectSharp.Common.Model.Relay;
using WalletConnectSharp.Crypto;
using WalletConnectSharp.Network;
using WalletConnectSharp.Network.Models;

// ReSharper disable AccessToDisposedClosure

namespace WalletConnectUnity.Core.Tests
{
    public class WebSocketConnectionTests
    {
        private const string DefaultGoodWsURL = "wss://relay.walletconnect.com";

        private static readonly string EnvironmentDefaultGoodWsURL =
            Environment.GetEnvironmentVariable("RELAY_ENDPOINT");

        private static readonly string GoodWsURL = !string.IsNullOrWhiteSpace(EnvironmentDefaultGoodWsURL)
            ? EnvironmentDefaultGoodWsURL
            : DefaultGoodWsURL;

        private static readonly JsonRpcRequest<TopicData> TestIrnRequest =
            new(RelayProtocols.DefaultProtocol.Subscribe,
                new TopicData { Topic = "ca838d59a3a3fe3824dab9ca7882ac9a2227c5d0284c88655b261a2fe85db270" });

        public static async Task<string> BuildGoodURL()
        {
            var crypto = new Crypto();
            await crypto.Init();

            var auth = await crypto.SignJwt(GoodWsURL);

            var relayUrlBuilder = new RelayUrlBuilder();
            return relayUrlBuilder.FormatRelayRpcUrl(
                GoodWsURL,
                RelayProtocols.Default,
                RelayConstants.Version.ToString(),
                ProjectConfiguration.Load().Id,
                auth
            );
        }

        [Test]
        public async Task ConnectionOpensSuccessfully()
        {
            LogAssert.ignoreFailingMessages = true;

            var url = await BuildGoodURL();

            using var wsc = new WebSocketConnection(url);

            await ((IJsonRpcConnection)wsc).Open();

            while (!wsc.Connected)
                await Task.Delay(500);

            Assert.IsTrue(wsc.Connected);
        }

        [Test]
        public async Task ConnectionClosesSuccessfully()
        {
            LogAssert.ignoreFailingMessages = true;

            var url = await BuildGoodURL();

            using var wsc = new WebSocketConnection(url);

            await ((IJsonRpcConnection)wsc).Open();

            while (!wsc.Connected)
                await Task.Delay(500);

            await ((IJsonRpcConnection)wsc).Close();

            Assert.IsFalse(wsc.Connected);
        }

        [Test]
        public async Task ThrowsOnSendRequestWhenClosed()
        {
            LogAssert.ignoreFailingMessages = true;

            var url = await BuildGoodURL();

            using var wsc = new WebSocketConnection(url);

            Assert.ThrowsAsync<IOException>(async () => await ((IJsonRpcConnection)wsc).SendRequest(TestIrnRequest, null));
        }


        [Test]
        public async Task SendsRequestSuccessfully()
        {
            LogAssert.ignoreFailingMessages = true;

            var url = await BuildGoodURL();

            using var wsc = new WebSocketConnection(url);

            await ((IJsonRpcConnection)wsc).Open();

            while (!wsc.Connected)
                await Task.Delay(500);

            var task = ((IJsonRpcConnection)wsc).SendRequest(TestIrnRequest, null);
            await task;

            Assert.IsTrue(task.IsCompleted);
        }
    }

    public class TopicData
    {
        [JsonProperty("topic")]
        public string Topic;
    }
}