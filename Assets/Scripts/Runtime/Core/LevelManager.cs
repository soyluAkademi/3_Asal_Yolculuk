using UnityEngine;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.Core
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Seviye / Gezegen Listesi")]
        [SerializeField] private PlanetDataSO[] planetDataList;
        [SerializeField] private int currentLevelIndex;

        public int CurrentLevelIndex => currentLevelIndex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public PlanetDataSO CurrentPlanetData
        {
            get
            {
                if (planetDataList == null || planetDataList.Length == 0) return null;

                int clampedIndex = Mathf.Clamp(currentLevelIndex, 0, planetDataList.Length - 1);
                return planetDataList[clampedIndex];
            }
        }

        public void SetLevel(int levelIndex)
        {
            if (planetDataList == null || planetDataList.Length == 0) return;

            currentLevelIndex = Mathf.Clamp(levelIndex, 0, planetDataList.Length - 1);
        }
    }
}
