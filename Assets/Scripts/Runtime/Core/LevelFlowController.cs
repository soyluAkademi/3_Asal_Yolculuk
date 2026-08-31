using System.Collections;
using MaskTransitions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Meteors;
using PayeGames.AsalYolculuk.Player;
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

        [Header("Yeniden Başlatma")]
        [Tooltip("Üç can bitince açılan oyun sonu paneli")]
        [SerializeField] private GameOverPanelView gameOverPanel;
        [Tooltip("Geminin diriltilmesi için gemideki PlayerDeathEffect")]
        [SerializeField] private PlayerDeathEffect playerDeathEffect;

        [Header("Geçiş (Mask Transitions)")]
        [Tooltip("Ekranın kapanma/açılma süresi (tek yön)")]
        [SerializeField] private float transitionHalfDuration = 0.5f;
        [Tooltip("Ekran kapandıktan sonra sıfırlamanın yapılması için beklenen ek pay")]
        [SerializeField] private float transitionResetPadding = 0.1f;
        [Tooltip("Ana menü butonunun yükleyeceği sahne")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Buton Tık Sesi")]
        [Tooltip("Tıklanınca kendi üzerindeki AudioSource'u çalacak butonlar. Klip " +
            "butondaki AudioSource'un Clip alanına atanır; Play On Awake kapalı olmalı")]
        [SerializeField] private Button[] clickSoundButtons;

        private bool isTransitioning;

        private void Awake()
        {
            RegisterButtonSounds();
        }

        /// <summary>
        /// Butonların tık sesini bağlar. Butonun kendi işlevi Inspector'daki onClick
        /// bağından gelir; buraya yalnızca ses dinleyicisi eklenir. AudioSource
        /// referansı burada bir kez cache'lenir, tıklama anında GetComponent çağrılmaz.
        /// </summary>
        private void RegisterButtonSounds()
        {
            if (clickSoundButtons == null) return;

            for (int index = 0; index < clickSoundButtons.Length; index++)
            {
                Button button = clickSoundButtons[index];
                if (button == null) continue;
                if (!button.TryGetComponent(out AudioSource clickSource)) continue;

                button.onClick.AddListener(() => SoundManager.PlayUiSfx(clickSource));
            }
        }

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

        /// <summary>
        /// Oyun sonu panelindeki "Tekrar Dene" butonuna bağlanır. Sahne değişmez;
        /// ekran geçişle kapanır, kapalıyken bölüm sıfırlanıp gemi diriltilir,
        /// ardından ekran tekrar açılır.
        /// </summary>
        public void RestartLevel()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            StartCoroutine(RestartRoutine());
        }

        private IEnumerator RestartRoutine()
        {
            // Geçiş animasyonu ve beklemeler ölçekli zaman kullanır; panel duraklamışken
            // açılmış olabileceği için zaman burada normale döndürülür.
            Time.timeScale = 1f;

            bool hasTransition = TransitionManager.Instance != null;
            if (hasTransition)
            {
                TransitionManager.Instance.PlayStartHalfTransition(transitionHalfDuration);

                // Sıfırlama ekran tamamen kapalıyken yapılır; oyuncu geminin
                // yerine ışınlanmasını ve meteorların silinmesini görmez.
                yield return new WaitForSeconds(transitionHalfDuration + transitionResetPadding);
            }

            RestartCurrentLevel();

            if (hasTransition)
            {
                TransitionManager.Instance.PlayEndHalfTransition(transitionHalfDuration);
            }

            isTransitioning = false;
        }

        /// <summary>
        /// Seviye ilerletilmeden bölümü baştan kurar. GDD gereği bölüm puanı, tamamlanan
        /// meteor listesi ve özel sayı listesi başlangıç durumuna döner.
        /// </summary>
        private void RestartCurrentLevel()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.Hide();
            }

            ResetLevelProgress(revertLevelScore: true);

            if (playerDeathEffect != null)
            {
                playerDeathEffect.RestoreShip();
            }

            // Durum değişimi HUD panellerini geri getirir ve gemiyi başlangıç
            // konumuna alır; bu yüzden gemi diriltildikten sonra çağrılır.
            if (GameManager.Instance == null) return;

            GameManager.Instance.SetState(GameState.Gameplay);
        }

        /// <summary>
        /// Oyun sonu panelindeki ve duraklatma panelindeki "Ana Menü" butonuna
        /// bağlanır. Yarım kalan bölümün puanı toplamdan düşülüp diske yazılır,
        /// kalınan bölüm kaydedilir, ardından geçişle sahne yüklenir.
        /// </summary>
        public void GoToMainMenu()
        {
            if (isTransitioning) return;

            isTransitioning = true;
            Time.timeScale = 1f;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RevertLevelScoreFromTotal();
                ScoreManager.Instance.SaveTotalScore();
            }

            SaveCurrentLevel();

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadLevel(mainMenuSceneName);
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Oyuncunun kaldığı bölümü diske yazar; oyun yeniden açıldığında
        /// <see cref="LevelManager"/> bu değerden başlar. Bölüm tamamlanmadığı
        /// için değer ilerlemez, yalnızca mevcut ilerleme sabitlenir.
        /// </summary>
        private static void SaveCurrentLevel()
        {
            if (LevelManager.Instance == null) return;

            PlanetProgress.Unlock(LevelManager.Instance.CurrentLevelIndex);
        }

        private void AdvanceLevel()
        {
            if (LevelManager.Instance == null) return;

            LevelManager.Instance.SetLevel(LevelManager.Instance.CurrentLevelIndex + 1);

            // Yeni gezegen ana menüdeki seçim panelinde kilitsiz görünsün diye
            // ilerleme SetLevel sonrasındaki (clamp'lenmiş) index'ten okunur.
            PlanetProgress.Unlock(LevelManager.Instance.CurrentLevelIndex);

            // Seçim de yeni gezegene taşınır; kilit açıldıktan sonra çağrılır,
            // aksi halde henüz kilitli olan index seçilemezdi.
            PlanetProgress.Select(LevelManager.Instance.CurrentLevelIndex);
        }

        /// <summary>
        /// Puan, can ve sayı havuzunu bölüm için temizler.
        /// </summary>
        /// <param name="revertLevelScore">
        /// Bölüm kaybedildiğinde <c>true</c>: o bölümde kazanılan puan kalıcı toplamdan
        /// geri alınır. Bölüm tamamlandığında <c>false</c>: kazanılan puan toplamda kalır.
        /// </param>
        private void ResetLevelProgress(bool revertLevelScore = false)
        {
            if (ScoreManager.Instance != null)
            {
                if (revertLevelScore)
                {
                    ScoreManager.Instance.RevertLevelScoreFromTotal();
                }
                else
                {
                    ScoreManager.Instance.ResetScore();
                }

                // Toplam puan burada diske yazılır; oyun beklenmedik şekilde
                // kapanırsa en fazla bir bölümlük puan kaybolur.
                ScoreManager.Instance.SaveTotalScore();
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
