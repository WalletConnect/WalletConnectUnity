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

        private bool _isLoading;
        private Sprite _sprite;

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
        }

        public void SubscribeImage(Image image)
        {
            if (_sprite == null && !_isLoading)
                UnityEventsDispatcher.Instance.StartCoroutine(LoadRemoteSprite());

            if (_sprite != null)
                UpdateImage(image);

            _subscribedImages.Add(image);
        }

        public void UnsubscribeImage(Image image)
        {
            image.sprite = null;
            _subscribedImages.Remove(image);
        }

        private void UpdateImage(Image image)
        {
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
                    $"[WalletConnectUnity] Failed to load remote sprite: {uwr.error}. DownloadHandler error: {uwr.downloadHandler.error}"
                );
            }
            else
            {
                var tex = DownloadHandlerTexture.GetContent(uwr);
                _sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100.0f);

                foreach (var image in _subscribedImages)
                    UpdateImage(image);
            }

            _isLoading = false;
        }
    }
}