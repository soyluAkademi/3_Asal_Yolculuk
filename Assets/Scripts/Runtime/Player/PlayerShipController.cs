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

        private InputAction moveAction;
        private Vector2 currentVelocity;
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

        private void Update()
        {
            // Bölüm bittiğinde gemi kontrolü kapanır.
            if (!GameManager.IsGameplayActive)
            {
                currentVelocity = Vector2.zero;
                return;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector2 targetVelocity = input * moveSpeed;
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref currentVelocity, smoothTime);

            Vector3 nextPosition = selfTransform.position + (Vector3)(currentVelocity * Time.deltaTime);
            selfTransform.position = ClampToScreen(nextPosition);
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
