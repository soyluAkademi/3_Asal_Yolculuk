using DG.Tweening;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Bölüm tamamlandığında gemiyi bölüm sonu sunum konumuna yumuşakça taşır.
    /// Son gezegende (oyun bitişi) taşımak yerine gemiyi ekranın sağından
    /// uçurup tamamen yok eder. Gemi kontrolü <see cref="PlayerShipController"/>
    /// içindeki durum kontrolüyle zaten kapandığı için input ile çakışma olmaz.
    /// </summary>
    public class PlayerLevelCompleteEffect : MonoBehaviour
    {
        [Header("Bölüm Sonu Konumu")]
        [Tooltip("Geminin bölüm sonunda gideceği dünya konumu")]
        [SerializeField] private Vector2 levelCompletePosition = new Vector2(0f, -5f);
        [Tooltip("Taşınmanın başlaması için beklenen süre")]
        [SerializeField] private float moveDelay = 0.2f;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        [Header("Oyun Sonu (son gezegen)")]
        [Tooltip("Geminin uçup gitmeye başlaması için beklenen süre")]
        [SerializeField] private float exitDelay = 0.2f;
        [SerializeField] private float exitDuration = 1.2f;
        [Tooltip("Önce hafif geri çekilip sonra fırlaması için InBack uygundur")]
        [SerializeField] private Ease exitEase = Ease.InBack;
        [Tooltip("Ekranın sağ kenarından ne kadar dışarı çıkacağı")]
        [SerializeField] private float exitMargin = 3f;
        [Tooltip("Uzaklaşma hissi için küçüleceği ölçek oranı")]
        [SerializeField, Range(0.05f, 1f)] private float exitScaleRatio = 0.3f;

        private Transform selfTransform;
        private Camera gameCamera;
        private Vector3 startPosition;
        private Vector3 baseScale;
        private Tween activeTween;

        private void Awake()
        {
            selfTransform = transform;
            gameCamera = Camera.main;
            startPosition = selfTransform.position;
            baseScale = selfTransform.localScale;
        }

        private void Start()
        {
            // GameManager Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            KillActiveTween();

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Gameplay)
            {
                ResetToStartPosition();
                return;
            }

            if (state != GameState.LevelComplete) return;

            // Son gezegende bir sonraki bölüm yok; gemi sunum konumuna gitmek
            // yerine sahneden tamamen çıkar.
            if (LevelManager.Instance != null && LevelManager.Instance.IsLastLevel)
            {
                FlyAwayAndDisappear();
                return;
            }

            MoveToLevelCompletePosition();
        }

        /// <summary>
        /// Yeni bölüm başlarken gemiyi görünür hâle getirip sahnedeki başlangıç
        /// konumuna ve ölçeğine döndürür.
        /// </summary>
        private void ResetToStartPosition()
        {
            KillActiveTween();

            gameObject.SetActive(true);
            selfTransform.position = startPosition;
            selfTransform.localScale = baseScale;
        }

        /// <summary>
        /// Gemiyi ekranın sağından dışarı fırlatıp küçülterek yok eder. Tween
        /// bitiminde obje kapatılır; durum event'i aboneliği C# event'i olduğu için
        /// obje pasifken de canlı kalır, böylece yeniden başlatmada gemi geri gelir.
        /// </summary>
        private void FlyAwayAndDisappear()
        {
            KillActiveTween();

            activeTween = DOTween.Sequence()
                .AppendInterval(exitDelay)
                .Append(selfTransform.DOMoveX(GetExitX(), exitDuration).SetEase(exitEase))
                .Join(selfTransform.DOScale(baseScale * exitScaleRatio, exitDuration))
                .OnComplete(() =>
                {
                    activeTween = null;
                    gameObject.SetActive(false);
                });
        }

        /// <summary>Ekranın sağ kenarının biraz dışı; kamera yoksa mevcut konumdan ölçülür.</summary>
        private float GetExitX()
        {
            if (gameCamera == null) return selfTransform.position.x + exitMargin;

            Vector3 rightEdge = gameCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, gameCamera.nearClipPlane));

            return rightEdge.x + exitMargin;
        }

        private void MoveToLevelCompletePosition()
        {
            KillActiveTween();

            Vector3 targetPosition = new Vector3(levelCompletePosition.x, levelCompletePosition.y, selfTransform.position.z);

            activeTween = selfTransform
                .DOMove(targetPosition, moveDuration)
                .SetDelay(moveDelay)
                .SetEase(moveEase);
        }

        private void KillActiveTween()
        {
            if (activeTween == null) return;

            activeTween.Kill();
            activeTween = null;
        }
    }
}
