using UnityEngine;
using PayeGames.AsalYolculuk.Meteors;
using PayeGames.AsalYolculuk.UI;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Bölüm sonu geçişlerini tek yerde toplar. Bölüm sonu paneli ekrandan çıktıktan
    /// sonra seviye ilerletilir, puan/can/sayı havuzu sıfırlanır ve oynanış yeniden
    /// başlatılır. Panel butonu bu sınıfın metotlarını çağırır.
    /// </summary>
    public class LevelFlowController : MonoBehaviour
    {
        [Header("Referanslar")]
        [SerializeField] private LevelCompletePanelView levelCompletePanel;
        [Tooltip("Yeni bölümde sayı havuzunun sıfırlanması için gerekli")]
        [SerializeField] private MeteorSpawner meteorSpawner;

        private bool isTransitioning;

        /// <summary>Bölüm sonu panelindeki "Yolculuğa Başla" butonuna bağlanır.</summary>
        public void GoToNextLevel()
        {
            // Butona arka arkaya basılırsa seviye iki kez atlanmasın.
            if (isTransitioning) return;
            if (LevelManager.Instance != null && LevelManager.Instance.IsLastLevel) return;

            isTransitioning = true;

            if (levelCompletePanel == null)
            {
                StartNextLevel();
                return;
            }

            levelCompletePanel.HideToLeft(StartNextLevel);
        }

        /// <summary>
        /// Panel ekrandan çıktıktan sonra çalışır: seviye ilerler, ilerleme verileri
        /// sıfırlanır ve durum <see cref="GameState.Gameplay"/> yapılarak üretim,
        /// ateş ve gemi kontrolü yeniden açılır.
        /// </summary>
        private void StartNextLevel()
        {
            AdvanceLevel();
            ResetLevelProgress();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Gameplay);
            }

            isTransitioning = false;
        }

        private void AdvanceLevel()
        {
            if (LevelManager.Instance == null) return;

            LevelManager.Instance.SetLevel(LevelManager.Instance.CurrentLevelIndex + 1);
        }

        /// <summary>Puan, can ve sayı havuzunu yeni bölüm için temizler.</summary>
        private void ResetLevelProgress()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetScore();
            }

            if (HealthManager.Instance != null)
            {
                HealthManager.Instance.ResetHealth();
            }

            // Seviye değiştiği için havuz zaten yeniden kurulur; tamamlanan sayı
            // listesinin taşınmadığından emin olmak için açıkça sıfırlanır.
            if (meteorSpawner == null) return;

            meteorSpawner.ResetNumberPool();
        }
    }
}
