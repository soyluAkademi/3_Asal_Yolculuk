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
        [Tooltip("Yedek üretim hızı (saniyedeki meteor sayısı). Normalde aktif gezegenin " +
                 "PlanetDataSO değeri kullanılır; bu değer yalnızca gezegen verisi yokken devreye girer.")]
        [SerializeField] private float spawnRate = 1f;

        [Header("Meteor Görselleri")]
        [SerializeField] private Sprite[] meteorSprites;
        [Tooltip("Sprite dizisiyle AYNI sırada olmalı; index eşleşmesiyle her meteora kendi patlama efekti verilir")]
        [SerializeField] private GameObject[] explosionEffectPrefabs;
        [Tooltip("İlk seviyede kaç görselin kullanılabileceği; her seviyede bir tane daha açılır")]
        [SerializeField] private int baseSpriteCount = 3;

        [Header("Patlama Sesi")]
        [Tooltip("Bu obje üzerindeki ses kaynağı. Klip doğrudan kaynağın Clip alanına " +
            "atanır; Play On Awake kapalı olmalı. Meteor patlar patlamaz havuza döndüğü " +
            "için ses meteorun kendi üzerinde çalınamaz, sahnede kalıcı olan bu kaynak kullanılır")]
        [SerializeField] private AudioSource explosionSource;

        [Header("Ekran Sınırları")]
        [Tooltip("Meteorların üretileceği dikey sınır (0 = ekranın altı, 1 = ekranın üstü)")]
        [SerializeField, Range(0f, 1f)] private float minViewportY = 0.05f;
        [SerializeField, Range(0f, 1f)] private float maxViewportY = 0.95f;
        [Tooltip("Ekranın sağ kenarından ne kadar uzakta (ekran dışında) üretileceği")]
        [SerializeField] private float spawnMargin = 1f;

        private Camera gameCamera;
        private float spawnTimer;

        // Meteor Çağırıcı cezası bunu geçici olarak artırır.
        private float spawnRateMultiplier = 1f;
        private LevelNumberPool numberPool;

        private void Awake()
        {
            gameCamera = Camera.main;

            // Hız çarpanı static olduğu için sahne değişiminde (ve Editor'da domain
            // reload kapalıysa) önceki oturumdan yavaşlamış hâlde kalabilir.
            MeteorController.ResetSpeedMultiplier();
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
        private void HandleMeteorCompleted(int rootNumber, Vector3 position)
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

            float currentSpawnRate = GetSpawnRate();
            if (currentSpawnRate <= 0f) return;

            spawnTimer += Time.deltaTime;

            float spawnInterval = 1f / currentSpawnRate;
            if (spawnTimer < spawnInterval) return;

            spawnTimer -= spawnInterval;
            SpawnMeteor();
        }

        /// <summary>
        /// Meteor Çağırıcı gibi etkiler için üretim hızı çarpanı (2 = iki kat sık).
        /// Gezegenin kendi hızının <b>üzerine</b> uygulanır; böylece ceza ileri
        /// gezegenlerde otomatik olarak daha sert hissedilir.
        /// </summary>
        public void SetSpawnRateMultiplier(float multiplier)
        {
            spawnRateMultiplier = Mathf.Max(0f, multiplier);
        }

        public void ResetSpawnRateMultiplier()
        {
            spawnRateMultiplier = 1f;
        }

        /// <summary>
        /// Aktif gezegenin üretim hızı (saniyedeki meteor sayısı). Bölüm ilerledikçe
        /// artar; gezegen verisi bağlı değilse Inspector'daki yedek değere düşülür,
        /// böylece gezegensiz test sahneleri eskisi gibi çalışır.
        /// </summary>
        private float GetSpawnRate()
        {
            return GetBaseSpawnRate() * spawnRateMultiplier;
        }

        private float GetBaseSpawnRate()
        {
            PlanetDataSO planetData = LevelManager.Instance != null ? LevelManager.Instance.CurrentPlanetData : null;
            if (planetData == null) return spawnRate;
            if (planetData.MeteorSpawnRate <= 0f) return spawnRate;

            return planetData.MeteorSpawnRate;
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
                meteorController.SetExplosionAudio(explosionSource);
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
