using UnityEngine;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Yön butonlarının bulunduğu panele eklenir. Butonların basılı durumunu
    /// okuyup gemiye yön vektörü olarak iletir. Klavye girdisi ile birlikte
    /// çalışır; ikisi birbirini ezmez.
    /// </summary>
    [DisallowMultipleComponent]
    public class TouchMovementPanel : MonoBehaviour
    {
        [Header("Hedef")]
        [SerializeField] private PlayerShipController shipController;

        [Header("Yön Butonları")]
        [SerializeField] private Button leftBtn;
        [SerializeField] private Button rightBtn;
        [SerializeField] private Button upBtn;
        [SerializeField] private Button downBtn;

        private HoldableButton leftHold;
        private HoldableButton rightHold;
        private HoldableButton upHold;
        private HoldableButton downHold;

        private void Awake()
        {
            leftHold = EnsureHoldable(leftBtn);
            rightHold = EnsureHoldable(rightBtn);
            upHold = EnsureHoldable(upBtn);
            downHold = EnsureHoldable(downBtn);
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Bölüm bittiğinde yön paneli tamamen kapanır. Abonelik C# event'i
        /// olduğu için GameObject kapalıyken de tetiklenir; panel oynanışa
        /// dönüldüğünde kendini geri açabilir.
        /// </summary>
        private void HandleGameStateChanged(GameState state)
        {
            // Duraklatmada panel yerinde kalır; sadece bölüm bittiğinde kapanır.
            if (state == GameState.Paused) return;

            gameObject.SetActive(state == GameState.Gameplay);
        }

        private void OnDisable()
        {
            if (shipController == null) return;

            shipController.SetButtonMoveInput(Vector2.zero);
        }

        private void Update()
        {
            if (shipController == null) return;

            Vector2 input = new Vector2(
                (IsHeld(rightHold) ? 1f : 0f) - (IsHeld(leftHold) ? 1f : 0f),
                (IsHeld(upHold) ? 1f : 0f) - (IsHeld(downHold) ? 1f : 0f));

            shipController.SetButtonMoveInput(input);
        }

        /// <summary>
        /// Basılı tutma takibi butonun kendi GameObject'inde olmalı; elle
        /// eklemeyi unutmamak için burada garantiye alınır.
        /// </summary>
        private static HoldableButton EnsureHoldable(Button button)
        {
            if (button == null) return null;

            if (button.TryGetComponent(out HoldableButton holdable)) return holdable;

            return button.gameObject.AddComponent<HoldableButton>();
        }

        private static bool IsHeld(HoldableButton holdable)
        {
            return holdable != null && holdable.IsPressed;
        }
    }
}
