using System;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Bölüm puanını tutar ve değişimleri event ile yayınlar. UI bu event'i dinler.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Puan Ayarları")]
        [Tooltip("Bölümü tamamlamak için gereken puan")]
        [SerializeField] private int targetScore = 100;
        [Tooltip("Ana meteor yok edildiğinde kazanılan puan")]
        [SerializeField] private int scorePerMeteor = 5;

        /// <summary>Parametreler: güncel puan, hedef puan.</summary>
        public event Action<int, int> OnScoreChanged;

        /// <summary>Hedef puana ulaşıldığında bir kez tetiklenir. GameManager dinler.</summary>
        public event Action OnTargetReached;

        public int CurrentScore { get; private set; }
        public int TargetScore => targetScore;

        private bool isTargetReached;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddMeteorScore()
        {
            AddScore(scorePerMeteor);
        }

        public void AddScore(int amount)
        {
            if (amount == 0) return;

            CurrentScore = Mathf.Max(0, CurrentScore + amount);
            OnScoreChanged?.Invoke(CurrentScore, targetScore);

            TryRaiseTargetReached();
        }

        /// <summary>Canlar bittiğinde veya bölüm yeniden başladığında çağrılır.</summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            isTargetReached = false;
            OnScoreChanged?.Invoke(CurrentScore, targetScore);
        }

        /// <summary>
        /// Hedefe ulaşıldığında bölüm sonu akışını başlatır. Bölüm bittikten sonra
        /// akan puanlar (patlamakta olan meteorlar) event'i tekrar tetiklemesin diye
        /// bayrakla korunur.
        /// </summary>
        private void TryRaiseTargetReached()
        {
            if (isTargetReached) return;
            if (CurrentScore < targetScore) return;

            isTargetReached = true;
            OnTargetReached?.Invoke();
        }
    }
}
