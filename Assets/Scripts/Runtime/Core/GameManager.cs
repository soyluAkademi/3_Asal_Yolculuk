using System;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Oyun durumunu (<see cref="GameState"/>) yönetir. Canlar bittiğinde bölümü
    /// başarısız duruma alır; üretim/ateş sistemleri bu durumu okuyup durur.
    /// Zaman yalnızca <see cref="GameState.Paused"/> durumunda durdurulur.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Oynanış devam ediyor mu? Sahnede GameManager yoksa (test sahneleri)
        /// sistemler eskisi gibi çalışmaya devam eder.
        /// </summary>
        public static bool IsGameplayActive => Instance == null || Instance.CurrentState == GameState.Gameplay;

        public event Action<GameState> OnGameStateChanged;

        public GameState CurrentState { get; private set; } = GameState.Gameplay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // HealthManager ve ScoreManager Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (HealthManager.Instance != null)
            {
                HealthManager.Instance.OnAllLivesLost += HandleAllLivesLost;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTargetReached += HandleTargetReached;
            }

            // Bir önceki denemeden donmuş zamanla başlanmasın.
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (HealthManager.Instance != null)
            {
                HealthManager.Instance.OnAllLivesLost -= HandleAllLivesLost;
            }

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnTargetReached -= HandleTargetReached;
            }

            if (Instance != this) return;

            // Sahne değişiminde donmuş zamanla kalınmasın.
            Time.timeScale = 1f;
            Instance = null;
        }

        private void HandleAllLivesLost()
        {
            SetState(GameState.LevelFailed);
        }

        private void HandleTargetReached()
        {
            SetState(GameState.LevelComplete);
        }

        public void SetState(GameState state)
        {
            if (CurrentState == state) return;

            CurrentState = state;

            // Sadece duraklatmada zaman durur. Bölüm başarısız olduğunda arka plan
            // kaymaya ve patlama sekansı akmaya devam etmeli.
            Time.timeScale = state == GameState.Paused ? 0f : 1f;

            OnGameStateChanged?.Invoke(state);
        }
    }
}
