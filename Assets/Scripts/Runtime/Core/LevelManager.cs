using UnityEngine;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Seviye / Gezegen Listesi")]
        [SerializeField] private PlanetDataSO[] planetDataList;
        [Tooltip("Yalnızca editör varsayılanı; oyun başlarken kayıtlı ilerleme bunun üzerine yazar.")]
        [SerializeField] private int currentLevelIndex;

        public int CurrentLevelIndex => currentLevelIndex;

        /// <summary>Son gezegendeyiz; bölüm sonunda "devam" yerine oyun sonu akışı çalışır.</summary>
        public bool IsLastLevel => planetDataList != null && currentLevelIndex >= planetDataList.Length - 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            LoadSavedLevel();
        }

        /// <summary>
        /// Seçim panelinde işaretlenen gezegenden başlatır. Seçim yoksa ya da
        /// artık kilitliyse <see cref="PlanetProgress.SelectedIndex"/> zaten
        /// açılmış en yüksek gezegene düşer, yani oyuncu kaldığı yerden devam eder.
        /// Sahnedeki <see cref="currentLevelIndex"/> yalnızca editör varsayılanıdır.
        /// </summary>
        private void LoadSavedLevel()
        {
            SetLevel(PlanetProgress.SelectedIndex);
        }

        public PlanetDataSO CurrentPlanetData
        {
            get
            {
                if (planetDataList == null || planetDataList.Length == 0) return null;

                // Inspector'dan aralık dışı değer girilse bile oynanış bozulmasın.
                return planetDataList[Mathf.Clamp(currentLevelIndex, 0, planetDataList.Length - 1)];
            }
        }

        /// <summary>Bir sonraki gezegen; son gezegendeysek null.</summary>
        public PlanetDataSO NextPlanetData => IsLastLevel ? null : GetPlanetData(currentLevelIndex + 1);

        /// <summary>Sıra dışı index'te null döner (clamp yapılmaz).</summary>
        public PlanetDataSO GetPlanetData(int levelIndex)
        {
            if (planetDataList == null) return null;
            if (levelIndex < 0 || levelIndex >= planetDataList.Length) return null;

            return planetDataList[levelIndex];
        }

        public void SetLevel(int levelIndex)
        {
            if (planetDataList == null || planetDataList.Length == 0) return;

            currentLevelIndex = Mathf.Clamp(levelIndex, 0, planetDataList.Length - 1);
        }
    }
}
