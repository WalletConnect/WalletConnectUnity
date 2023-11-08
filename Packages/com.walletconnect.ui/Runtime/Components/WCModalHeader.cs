using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace WalletConnectUnity.UI
{
    public sealed class WCModalHeader : MonoBehaviour
    {
        [field: SerializeField] public WCSnackbar Snackbar { get; private set; }

        [field: SerializeField] private RectTransform RootTransform { get; set; }

        [field: SerializeField] private TMP_Text TitleText { get; set; }

        [field: SerializeField] private Button LeftButton { get; set; }

        [field: SerializeField] private Button RightButton { get; set; }

        [field: SerializeField, Space] private WCModal Modal { get; set; }

        public float Height => RootTransform.rect.height;

        public string Title
        {
            get => TitleText.text;
            set => TitleText.text = value;
        }

        public bool LeftButtonActive
        {
            get => LeftButton.gameObject.activeSelf;
            set => LeftButton.gameObject.SetActive(value);
        }

        private void Awake()
        {
            Assert.IsNotNull(RootTransform, $"Missing {nameof(RootTransform)} reference in {name}");
            Assert.IsNotNull(TitleText, $"Missing {nameof(TitleText)} reference in {name}");
            Assert.IsNotNull(LeftButton, $"Missing {nameof(LeftButton)} reference in {name}");
            Assert.IsNotNull(RightButton, $"Missing {nameof(RightButton)} reference in {name}");

            Assert.IsNotNull(Modal, $"Missing {nameof(Modal)} reference in {name}");

            LeftButton.onClick.AddListener(OnLeftButtonClicked);
            RightButton.onClick.AddListener(OnRightButtonClicked);
        }

        private void OnLeftButtonClicked()
        {
            Modal.CloseView();
        }

        private void OnRightButtonClicked()
        {
            Modal.CloseModal();
        }
    }
}