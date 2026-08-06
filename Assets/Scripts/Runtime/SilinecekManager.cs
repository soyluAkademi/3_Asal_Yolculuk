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
        [Header("Başlangıç")]
        [Tooltip("İşaretliyse oyun son bölümden (son gezegenden) başlar")]
        [SerializeField] private bool startAtLastLevel;

        [Header("Kısayol")]
        [Tooltip("İşaretliyse aşağıdaki tuş bölümü anında tamamlar")]
        [SerializeField] private bool enableCompleteShortcut = true;
        [Tooltip("Bölümü anında tamamlayan tuş. Çalışmıyorsa Game view'a tıklayıp odağı ver.")]
        [SerializeField] private Key completeLevelKey = Key.F9;

        private void Start()
        {
            // Tüm manager'ların Awake'i bittikten sonra çalışsın.
            if (!startAtLastLevel) return;
            if (LevelManager.Instance == null) return;

            // SetLevel aralığa clamp'lediği için son bölüme gitmek üzere
            // taşan bir değer verilir; LevelManager'a ek üye gerekmez.
            LevelManager.Instance.SetLevel(int.MaxValue);
        }

        private void Update()
        {
            if (!enableCompleteShortcut) return;
            if (Keyboard.current == null) return;
            if (!Keyboard.current[completeLevelKey].wasPressedThisFrame) return;

            CompleteLevelInstantly();
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
    }
}
