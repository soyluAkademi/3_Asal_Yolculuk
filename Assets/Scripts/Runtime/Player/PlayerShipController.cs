using UnityEngine;
using UnityEngine.InputSystem;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.Player
{
    [DisallowMultipleComponent]
    public class PlayerShipController : MonoBehaviour
    {
        [Header("Hareket")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float smoothTime = 0.12f;

        [Header("Ekran Sınırları")]
        [SerializeField] private float shipHalfWidth = 0.5f;
        [SerializeField] private float shipHalfHeight = 0.5f;
        [Tooltip("Geminin yatayda ulaşabileceği sol sınır (0 = ekranın solu, 1 = ekranın sağı)")]
        [SerializeField, Range(0f, 1f)] private float minViewportX = 0f;
        [Tooltip("Geminin yatayda ulaşabileceği sağ sınır (0 = ekranın solu, 1 = ekranın sağı)")]
        [SerializeField, Range(0f, 1f)] private float maxViewportX = 1f;

        // Aşağı çekim, geminin o anki hızının en fazla bu oranı kadar olabilir.
        // Aksi halde çekim hızı geçer, gemi hiç yukarı çıkamaz ve ceza felce döner.
        private const float MaxPullRatio = 0.7f;

        // Geminin o anki hızı ile SmoothDamp'in kendi iç tamponu ayrı tutulur.
        // Aynı değişkeni ikisi için de geçirmek yumuşatmayı bozuyordu.
        private Vector2 currentVelocity;
        private Vector2 smoothVelocity;

        // Yerçekimi Taşı cezası bunları geçici olarak değiştirir.
        private float moveSpeedMultiplier = 1f;
        private float downwardPull;

        private InputAction moveAction;
        private Transform selfTransform;
        private Camera gameCamera;

        private void Awake()
        {
            selfTransform = transform;
            gameCamera = Camera.main;

            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
        }

        private void OnEnable()
        {
            moveAction.Enable();
        }

        private void OnDisable()
        {
            moveAction.Disable();
        }

        /// <summary>
        /// Hareket hızı çarpanı (Yerçekimi Taşı: 0.5 = yarı hız).
        /// </summary>
        public void SetMoveSpeedMultiplier(float multiplier)
        {
            moveSpeedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void ResetMoveSpeedMultiplier()
        {
            moveSpeedMultiplier = 1f;
        }

        /// <summary>
        /// Gemiyi sürekli aşağı çeken kuvvet (birim/saniye). Kullanım anında
        /// geminin güncel hızına göre sınırlanır; gemi her koşulda yukarı
        /// çıkabilmelidir.
        /// </summary>
        public void SetDownwardPull(float pull)
        {
            downwardPull = Mathf.Max(0f, pull);
        }

        public void ResetDownwardPull()
        {
            downwardPull = 0f;
        }

        private void Update()
        {
            // Bölüm bittiğinde gemi kontrolü kapanır.
            if (!GameManager.IsGameplayActive)
            {
                currentVelocity = Vector2.zero;
                smoothVelocity = Vector2.zero;
                return;
            }

            float effectiveSpeed = moveSpeed * moveSpeedMultiplier;

            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector2 targetVelocity = input * effectiveSpeed;
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref smoothVelocity, smoothTime);

            Vector3 nextPosition = selfTransform.position + (Vector3)(currentVelocity * Time.deltaTime);
            nextPosition.y -= GetClampedPull(effectiveSpeed) * Time.deltaTime;

            selfTransform.position = ClampToScreen(nextPosition);
        }

        /// <summary>
        /// Çekim, yumuşatmanın dışında doğrudan konuma uygulanır; hız vektörüne
        /// eklenseydi SmoothDamp onu sürekli geri emer ve etki hissedilmezdi.
        /// </summary>
        private float GetClampedPull(float effectiveSpeed)
        {
            if (downwardPull <= 0f) return 0f;

            return Mathf.Min(downwardPull, effectiveSpeed * MaxPullRatio);
        }

        private Vector3 ClampToScreen(Vector3 position)
        {
            if (gameCamera == null) return position;

            Vector3 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, gameCamera.nearClipPlane));
            Vector3 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1f, 1f, gameCamera.nearClipPlane));

            float minX = Mathf.Lerp(bottomLeft.x, topRight.x, minViewportX);
            float maxX = Mathf.Lerp(bottomLeft.x, topRight.x, maxViewportX);

            position.x = Mathf.Clamp(position.x, minX + shipHalfWidth, maxX - shipHalfWidth);
            position.y = Mathf.Clamp(position.y, bottomLeft.y + shipHalfHeight, topRight.y - shipHalfHeight);

            return position;
        }
    }
}
