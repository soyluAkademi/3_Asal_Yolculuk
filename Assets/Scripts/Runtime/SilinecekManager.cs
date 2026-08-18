using UnityEngine;
using UnityEngine.InputSystem;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk
{
    /// <summary>
    /// GEÇİCİ TEST ARACI — teslimden önce silinecek.
    ///
    /// Bitiş akışını tüm bölümleri oynamadan test etmek için kullanılır. Hiçbir
    /// sistem bu sınıfa referans vermez; sahnedeki objesiyle birlikte silindiğinde
    /// oyun hiçbir şekilde etkilenmez.
    /// </summary>
    public class SilinecekManager : MonoBehaviour
    {
        /// <summary>Test için elle seçilen başlangıç bölümü (1 tabanlı).</summary>
        public enum TestLevel
        {
            Bolum1 = 1,
            Bolum2 = 2,
            Bolum3 = 3,
            Bolum4 = 4,
            Bolum5 = 5,
            Bolum6 = 6,
            Bolum7 = 7,
            Bolum8 = 8
        }

        [Header("Başlangıç")]
        [Tooltip("İşaretliyse oyuncunun kaldığı bölümden devam edilir (normal oyun davranışı). " +
                 "Belirli bir bölümü test etmek için kaldır.")]
        [SerializeField] private bool useSavedProgress = true;
        [Tooltip("Play'e basınca başlanacak bölüm; yalnızca Use Saved Progress kapalıyken geçerli")]
        [SerializeField] private TestLevel startLevel = TestLevel.Bolum1;

        [Header("Kısayol")]
        [Tooltip("İşaretliyse aşağıdaki tuş bölümü anında tamamlar")]
        [SerializeField] private bool enableCompleteShortcut = true;
        [Tooltip("Bölümü anında tamamlayan tuş. Çalışmıyorsa Game view'a tıklayıp odağı ver.")]
        [SerializeField] private Key completeLevelKey = Key.F9;
        [Tooltip("İşaretliyse aşağıdaki tuş kalan tüm canları bitirir")]
        [SerializeField] private bool enableFailShortcut = true;
        [Tooltip("Tüm canları anında bitiren tuş. Çalışmıyorsa Game view'a tıklayıp odağı ver.")]
        [SerializeField] private Key failLevelKey = Key.F10;

        private void Start()
        {
            // Normal akışta LevelManager kaldığı bölümü Awake'inde yüklemiştir;
            // buradan müdahale edilirse o kayıt ezilir.
            if (useSavedProgress) return;

            // Tüm manager'ların Awake'i bittikten sonra çalışsın.
            if (LevelManager.Instance == null) return;

            // Enum 1 tabanlı, LevelManager 0 tabanlı index bekliyor.
            LevelManager.Instance.SetLevel((int)startLevel - 1);
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (enableCompleteShortcut && Keyboard.current[completeLevelKey].wasPressedThisFrame)
            {
                CompleteLevelInstantly();
            }

            if (enableFailShortcut && Keyboard.current[failLevelKey].wasPressedThisFrame)
            {
                FailLevelInstantly();
            }
        }

        /// <summary>
        /// Tuş çalışmadığında kullanılacak yedek yol: Play modundayken Inspector'da
        /// bileşenin sağ üstündeki ⋮ menüsünden çağrılır.
        /// </summary>
        [ContextMenu("Bolumu Tamamla")]
        private void CompleteLevelFromContextMenu()
        {
            CompleteLevelInstantly();
        }

        /// <summary>Tuş çalışmadığında kullanılacak yedek yol (Inspector ⋮ menüsü).</summary>
        [ContextMenu("Canlari Bitir")]
        private void FailLevelFromContextMenu()
        {
            FailLevelInstantly();
        }

        /// <summary>
        /// Hedef puanı bir kerede ekler; normal akışın aynısı çalışır
        /// (ScoreManager.OnTargetReached → GameManager → bölüm sonu sunumu).
        /// </summary>
        private void CompleteLevelInstantly()
        {
            if (ScoreManager.Instance == null) return;
            if (!GameManager.IsGameplayActive) return;

            int missingScore = ScoreManager.Instance.TargetScore - ScoreManager.Instance.CurrentScore;
            if (missingScore <= 0) return;

            ScoreManager.Instance.AddScore(missingScore);
        }

        /// <summary>
        /// Kalan canları tek tek eksiltir; normal akışın aynısı çalışır
        /// (HealthManager.OnAllLivesLost → GameManager + PlayerDeathEffect → oyun sonu paneli).
        /// Meteor çarpması taklit edilmediği için kamera sarsıntısı ve hasar efekti oynamaz.
        /// </summary>
        private void FailLevelInstantly()
        {
            if (HealthManager.Instance == null) return;
            if (!GameManager.IsGameplayActive) return;

            while (HealthManager.Instance.IsAlive)
            {
                HealthManager.Instance.LoseLife();
            }
        }
    }
}
