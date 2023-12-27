using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectUnity.Core;
using WalletConnectUnity.Core.Networking;

namespace WalletConnectUnity.UI
{
    public class RemoteSprite
    {
        private readonly string _uri;
        private readonly WCLoadingAnimator _loadingAnimator;

        private bool _isLoading;
        private Sprite _sprite;
        private bool _isLoaded;


        private readonly HashSet<Image> _subscribedImages = new();

        private static readonly Dictionary<string, RemoteSprite> UriSpritesMap = new();

        public static RemoteSprite Create(string uri)
        {
            return UriSpritesMap.TryGetValue(uri, out var remoteSprite) ? remoteSprite : new RemoteSprite(uri);
        }

        private RemoteSprite(string uri)
        {
            _uri = uri;
            UriSpritesMap.Add(uri, this);
            _loadingAnimator = WCLoadingAnimator.Instance;
        }

        public void SubscribeImage(Image image)
        {
            if (!_isLoaded && !_isLoading)
                UnityEventsDispatcher.Instance.StartCoroutine(LoadRemoteSprite());

            if (_isLoaded)
            {
                UpdateImage(image);
            }
            else
            {
                if (_loadingAnimator != null)
                    _loadingAnimator.SubscribeGraphic(image);
            }

            _subscribedImages.Add(image);
        }

        public void UnsubscribeImage(Image image)
        {
            image.sprite = null;
            _subscribedImages.Remove(image);
        }

        private void UpdateImage(Image image)
        {
            if (_loadingAnimator != null)
                _loadingAnimator.UnsubscribeGraphic(image);

            image.sprite = _sprite;
            image.color = Color.white;
        }

        private IEnumerator LoadRemoteSprite()
        {
            _isLoading = true;

            using var uwr = UnityWebRequestTexture.GetTexture(_uri, true);

            uwr.SetWalletConnectRequestHeaders()
                .SetRequestHeader("accept", "image/jpeg,image/png");

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"[WalletConnectUnity] Failed to load remote sprite: {uwr.error}. URI: [{_uri}]. DownloadHandler error: {uwr.downloadHandler.error}"
                );
            }
            else
            {
                // While UnityWebRequest creates texture in the background (on other thread), some finishing work is done on main thread.
                // Skipping a few frames here to let Unity finish its work to reduce CPU spikes.
                for (var i = 0; i < 5; i++)
                    yield return null;

                var tex = DownloadHandlerTexture.GetContent(uwr);
                _sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100.0f);
                _isLoaded = true;

                foreach (var image in _subscribedImages)
                    UpdateImage(image);
            }

            _isLoading = false;
        }
    }
}