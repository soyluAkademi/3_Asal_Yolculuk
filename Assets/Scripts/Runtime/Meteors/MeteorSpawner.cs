using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Meteors
{
    public class MeteorSpawner : MonoBehaviour
    {
        private const float PriorityNumberChance = 0.7f;

        [Header("Üretim")]
        [SerializeField] private GameObjectPool meteorPool;
        [SerializeField] private float spawnRate = 1f;

        [Header("Meteor Görselleri")]
        [SerializeField] private Sprite[] meteorSprites;
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

        private void Awake()
        {
            gameCamera = Camera.main;
        }

        private void Update()
        {
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
                meteorController.SetNumber(GenerateNumber());
                meteorController.SetSprite(GetRandomSprite());
            }
        }

        private Sprite GetRandomSprite()
        {
            if (meteorSprites == null || meteorSprites.Length == 0) return null;

            int currentLevelIndex = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelIndex : 0;
            int unlockedCount = Mathf.Clamp(baseSpriteCount + currentLevelIndex, 1, meteorSprites.Length);

            int randomIndex = Random.Range(0, unlockedCount);
            return meteorSprites[randomIndex];
        }

        private int GenerateNumber()
        {
            PlanetDataSO planetData = LevelManager.Instance != null ? LevelManager.Instance.CurrentPlanetData : null;
            if (planetData == null) return Random.Range(2, 100);

            bool usePriorityNumber = planetData.PriorityNumberList.Count > 0 && Random.value < PriorityNumberChance;
            if (usePriorityNumber)
            {
                int randomIndex = Random.Range(0, planetData.PriorityNumberList.Count);
                return planetData.PriorityNumberList[randomIndex];
            }

            return Random.Range(planetData.MinNumber, planetData.MaxNumber + 1);
        }
    }
}
