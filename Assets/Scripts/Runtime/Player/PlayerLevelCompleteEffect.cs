using DG.Tweening;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Bölüm tamamlandığında gemiyi bölüm sonu sunum konumuna yumuşakça taşır.
    /// Gemi kontrolü <see cref="PlayerShipController"/> içindeki durum kontrolüyle
    /// zaten kapandığı için taşıma sırasında input ile çakışma olmaz.
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

        private Transform selfTransform;
        private Vector3 startPosition;
        private Tween activeTween;

        private void Awake()
        {
            selfTransform = transform;
            startPosition = selfTransform.position;
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

            MoveToLevelCompletePosition();
        }

        /// <summary>Yeni bölüm başlarken gemiyi sahnedeki başlangıç konumuna alır.</summary>
        private void ResetToStartPosition()
        {
            KillActiveTween();
            selfTransform.position = startPosition;
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
