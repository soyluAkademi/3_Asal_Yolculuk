using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Meteors
{
    public class MeteorSpawner : MonoBehaviour
    {
        [Header("Üretim")]
        [SerializeField] private GameObjectPool meteorPool;
        [SerializeField] private float spawnRate = 1f;

        [Header("Meteor Görselleri")]
        [SerializeField] private Sprite[] meteorSprites;
        [Tooltip("Sprite dizisiyle AYNI sırada olmalı; index eşleşmesiyle her meteora kendi patlama efekti verilir")]
        [SerializeField] private GameObject[] explosionEffectPrefabs;
        [Tooltip("İlk seviyede kaç görselin kullanılabileceği; her seviyede bir tane daha açılır")]
        [SerializeField] private int baseSpriteCount = 3;

        [Header("Ekran Sınırları")]
        [Tooltip("Meteorların üretileceği dikey sınır (0 = ekranın altı, 1 = ekranın üstü)")]
        [SerializeField, Range(0f, 1f)] private float minViewportY = 0.05f;
        [SerializeField, Range(0f, 1f)] private float maxViewportY = 0.95f;
        [Tooltip("Ekranın sağ kenarından ne kadar uzakta (ekran dışında) üretileceği")]
        [SerializeField] private float spawnMargin = 1f;

        private Camera gameCamera;
        private float spawnTimer;
        private LevelNumberPool numberPool;

        private void Awake()
        {
            gameCamera = Camera.main;
        }

        private void OnEnable()
        {
            MeteorController.OnMeteorCompleted += HandleMeteorCompleted;
        }

        private void OnDisable()
        {
            MeteorController.OnMeteorCompleted -= HandleMeteorCompleted;
        }

        /// <summary>Tamamen yok edilen sayı bölüm sonuna kadar tekrar üretilmez.</summary>
        private void HandleMeteorCompleted(int rootNumber)
        {
            GetNumberPool().RegisterCompleted(rootNumber);
        }

        /// <summary>Bölüm baştan başladığında (canlar bittiğinde) çağrılır.</summary>
        public void ResetNumberPool()
        {
            GetNumberPool().Reset();
        }

        private void Update()
        {
            if (!GameManager.IsGameplayActive) return;

            spawnTimer += Time.deltaTime;

            float spawnInterval = 1f / spawnRate;
            if (spawnTimer < spawnInterval) return;

            spawnTimer -= spawnInterval;
            SpawnMeteor();
        }

        private void SpawnMeteor()
        {
            if (gameCamera == null) return;

            float viewportY = Random.Range(minViewportY, maxViewportY);
            Vector3 worldPoint = gameCamera.ViewportToWorldPoint(new Vector3(1f, viewportY, gameCamera.nearClipPlane));
            Vector3 spawnPosition = new Vector3(worldPoint.x + spawnMargin, worldPoint.y, 0f);

            GameObject instance = meteorPool.Spawn(spawnPosition, Quaternion.identity);
            if (instance.TryGetComponent(out MeteorController meteorController))
            {
                int visualIndex = GetRandomVisualIndex();
                meteorController.SetNumber(GetNumberPool().GetNumber());
                meteorController.SetVisuals(GetSprite(visualIndex), GetExplosionEffect(visualIndex));
            }
        }

        /// <summary>
        /// Sprite ve patlama efekti dizileri aynı sırada olduğu için tek bir index
        /// seçilir; böylece her meteor kendi görseline ait efekti kullanır.
        /// </summary>
        private int GetRandomVisualIndex()
        {
            if (meteorSprites == null || meteorSprites.Length == 0) return -1;

            int currentLevelIndex = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelIndex : 0;
            int unlockedCount = Mathf.Clamp(baseSpriteCount + currentLevelIndex, 1, meteorSprites.Length);

            return Random.Range(0, unlockedCount);
        }

        private Sprite GetSprite(int visualIndex)
        {
            if (visualIndex < 0 || visualIndex >= meteorSprites.Length) return null;

            return meteorSprites[visualIndex];
        }

        private GameObject GetExplosionEffect(int visualIndex)
        {
            if (explosionEffectPrefabs == null) return null;
            if (visualIndex < 0 || visualIndex >= explosionEffectPrefabs.Length) return null;

            return explosionEffectPrefabs[visualIndex];
        }

        /// <summary>
        /// Aktif gezegenin sayı havuzunu döner. Gezegen değiştiğinde havuz yeniden
        /// kurulur; böylece tamamlanan sayı listesi yeni bölüme taşınmaz.
        /// </summary>
        private LevelNumberPool GetNumberPool()
        {
            PlanetDataSO planetData = LevelManager.Instance != null ? LevelManager.Instance.CurrentPlanetData : null;

            if (numberPool == null || numberPool.PlanetData != planetData)
            {
                numberPool = new LevelNumberPool(planetData);
            }

            return numberPool;
        }
    }
}
