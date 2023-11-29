using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using WalletConnectSharp.Common.Logging;
using WalletConnectSharp.Core.Models.Publisher;
using WalletConnectSharp.Sign.Models;
using WalletConnectUnity.Core.Networking;

namespace WalletConnectUnity.Core
{
    public class Linker : IDisposable
    {
        private readonly IWalletConnect _walletConnect;
        private readonly Dictionary<string, uint> _sessionMessagesCounter = new();

        protected bool disposed;

        public Linker(IWalletConnect walletConnect)
        {
            _walletConnect = walletConnect;

            RegisterEventListeners();
        }

        private void RegisterEventListeners()
        {
            _walletConnect.SignClient.Core.Relayer.Publisher.OnPublishedMessage += OnPublisherPublishedMessage;
        }

        public static void OpenSessionProposalDeepLink(string uri, Wallet wallet)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException($"[Linker] Uri cannot be empty.");

#if UNITY_ANDROID
            // Android OS should handle wc: protocol, don't need to change anything
#elif UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
            uri = WebUtility.UrlEncode(uri);
            
            var link = Application.isMobilePlatform ? wallet.MobileLink : wallet.DesktopLink;

            if (string.IsNullOrWhiteSpace(link))
                throw new Exception(
                    $"[Linker] No link found for {Application.platform} platform in wallet {wallet.Name}.");

            if (!link.EndsWith("//"))
                link = $"{link}//";
            
            uri = $"{link}wc?uri={uri}";
#endif
            
            Debug.Log($"[Linker] Opening URL {uri}");
            
            Application.OpenURL(uri);
        }

        public static void OpenSessionRequestDeepLink(in SessionStruct session)
        {
            if (string.IsNullOrWhiteSpace(session.Topic))
                throw new Exception("[Linker] No session topic found in provided session. Cannot open deep link.");

            if (session.Peer.Metadata is { Redirect: not null })
            {
                Application.OpenURL(session.Peer.Metadata.Redirect.Native);
            }
            else
            {
                WCLogger.LogError(
                    $"[Linker] No redirect found for {session.Peer.Metadata.Name}. Cannot open deep link.");
            }
        }

        public void OpenSessionRequestDeepLink(string sessionTopic)
        {
            var session = _walletConnect.SignClient.Session.Get(sessionTopic);
            OpenSessionRequestDeepLink(in session);
        }

        public virtual void OpenSessionRequestDeepLink()
        {
            var session = _walletConnect.ActiveSession;
            OpenSessionRequestDeepLink(in session);
        }

        protected virtual void OnPublisherPublishedMessage(object sender, PublishParams publishParams)
        {
            Debug.Log(
                $"[Linker] OnPublisherPublishedMessage. Topic: {publishParams.Topic}. Topics in counter: {_sessionMessagesCounter.Count}");
            if (string.IsNullOrWhiteSpace(publishParams.Topic))
                return;

            if (_sessionMessagesCounter.TryGetValue(publishParams.Topic, out var messageCount))
            {
                Debug.Log($"[Linker] OnPublisherPublishedMessage. Message count: {messageCount}");
                if (messageCount != 0)
                {
                    _sessionMessagesCounter[publishParams.Topic] = messageCount - 1;
                    OpenSessionRequestDeepLink(publishParams.Topic);
                }
            }
        }

        internal void OpenSessionRequestDeepLinkAfterMessageFromSession(string sessionTopic)
        {
            Debug.Log($"[Linker] OpenSessionRequestDeepLinkAfterMessageFromSession. Topic: {sessionTopic}");
            if (_sessionMessagesCounter.TryGetValue(sessionTopic, out var messageCount))
            {
                _sessionMessagesCounter[sessionTopic] = messageCount + 1;
            }
            else
            {
                _sessionMessagesCounter.Add(sessionTopic, 1);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                _walletConnect.SignClient.Core.Relayer.Publisher.OnPublishedMessage -= OnPublisherPublishedMessage;
            }

            disposed = true;
        }
    }
}