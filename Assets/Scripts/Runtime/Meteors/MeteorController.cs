using UnityEngine;
using TMPro;
using PayeGames.AsalYolculuk.Combat;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Meteors
{
    public class MeteorController : MonoBehaviour, IPoolable
    {
        [Header("Hareket")]
        [SerializeField] private float minMoveSpeed = 2f;
        [SerializeField] private float maxMoveSpeed = 4f;
        [SerializeField] private float minRotationSpeed = 30f;
        [SerializeField] private float maxRotationSpeed = 90f;

        [Header("Ekran Dışı Sınır")]
        [Tooltip("Ekranın sol kenarından ne kadar uzakta despawn edileceği (ekran dışı tampon mesafesi)")]
        [SerializeField] private float despawnMargin = 1f;

        [Header("Sayı Görseli")]
        [SerializeField] private TMP_Text meteorTxt;

        [Header("Meteor Görseli")]
        [SerializeField] private SpriteRenderer meteorSpriteRenderer;

        private GameObjectPool ownerPool;
        private Transform selfTransform;
        private Camera gameCamera;
        private float despawnLeftOfX;
        private float screenRightEdgeX;
        private bool hasEnteredScreen;
        private float moveSpeed;
        private float rotationSpeed;
        private int number;
        private bool isPrimeNumber;

        public int Number => number;
        public bool IsPrimeNumber => isPrimeNumber;

        private void Awake()
        {
            selfTransform = transform;
            gameCamera = Camera.main;
        }

        public void OnSpawned(GameObjectPool owner)
        {
            ownerPool = owner;
            despawnLeftOfX = CalculateEdgeX(0f) - despawnMargin;
            screenRightEdgeX = CalculateEdgeX(1f);
            hasEnteredScreen = false;

            moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

            float speed = Random.Range(minRotationSpeed, maxRotationSpeed);
            rotationSpeed = Random.value < 0.5f ? speed : -speed;
        }

        public void SetNumber(int assignedNumber)
        {
            number = assignedNumber;
            isPrimeNumber = PrimeMathEngine.IsPrime(number);

            if (meteorTxt != null)
            {
                meteorTxt.text = number.ToString();
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (meteorSpriteRenderer == null) return;
            if (sprite == null) return;

            meteorSpriteRenderer.sprite = sprite;
        }

        private float CalculateEdgeX(float viewportX)
        {
            if (gameCamera == null) return 0f;

            Vector3 edge = gameCamera.ViewportToWorldPoint(new Vector3(viewportX, 0f, gameCamera.nearClipPlane));
            return edge.x;
        }

        public void OnDespawned()
        {
        }

        private void Update()
        {
            selfTransform.position += Vector3.left * moveSpeed * Time.deltaTime;
            selfTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            if (!hasEnteredScreen && selfTransform.position.x <= screenRightEdgeX)
            {
                hasEnteredScreen = true;
            }

            if (selfTransform.position.x <= despawnLeftOfX)
            {
                ownerPool.Despawn(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!hasEnteredScreen) return;
            if (!other.TryGetComponent(out Bullet bullet)) return;

            bullet.Despawn();
            ownerPool.Despawn(gameObject);
        }
    }
}
