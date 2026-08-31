using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;
using PayeGames.AsalYolculuk.Meteors;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Madenleri sahneye çıkarır. İki farklı model bilinçli olarak birlikte kullanılır:
    /// <list type="bullet">
    /// <item>Ödüller, bir ana meteor tamamen yok edildiğinde o noktada şansa bağlı
    /// düşer — ödül başarıya bağlanır.</item>
    /// <item>Cezalar, zamanlayıcıyla ekranın sağından girer — iyi oynamak ceza
    /// getirmesin diye başarıya bağlanmaz.</item>
    /// </list>
    /// Hangi madenlerin çıkacağını aktif gezegenin <see cref="PlanetDataSO"/>
    /// ayarları belirler (kademeli açılım). Ödül ve ceza sıklığı çarpanları
    /// bilinçli olarak ayrıdır: "bol ödüllü" bir gezegen bol cezalı değildir.
    /// </summary>
    public class MineSpawner : MonoBehaviour
    {
        [Header("Havuz")]
        [SerializeField] private GameObjectPool minePool;

        [Header("Maden Verileri")]
        [Tooltip("Ödül madenleri; içlerinden rastgele biri seçilir")]
        [SerializeField] private MineDataSO[] rewardMines;
        [Tooltip("Ceza madenleri; içlerinden rastgele biri seçilir")]
        [SerializeField] private MineDataSO[] curseMines;

        [Header("Ödül Düşme Şansı")]
        [Tooltip("Ana meteor tamamen yok edildiğinde ödül düşme olasılığı")]
        [SerializeField, Range(0f, 1f)] private float rewardDropChance = 0.15f;

        [Header("Ceza Zamanlayıcısı")]
        [SerializeField] private float minCurseInterval = 15f;
        [SerializeField] private float maxCurseInterval = 20f;
        [Tooltip("İki ceza arasındaki en kısa süre (saniye). Gezegen çarpanı ne olursa " +
                 "olsun bu değerin altına inilmez; aralık ceza etki süresinden (10 sn) " +
                 "kısalırsa cezalar üst üste binip kalıcı hale gelir.")]
        [SerializeField] private float curseIntervalFloor = 10f;

        [Header("Ceza Üretim Konumu")]
        [Tooltip("Cezaların üretileceği dikey sınır (0 = ekranın altı, 1 = ekranın üstü)")]
        [SerializeField, Range(0f, 1f)] private float minViewportY = 0.1f;
        [SerializeField, Range(0f, 1f)] private float maxViewportY = 0.9f;
        [Tooltip("Ekranın sağ kenarından ne kadar uzakta (ekran dışında) üretileceği")]
        [SerializeField] private float spawnMargin = 1f;

        private Camera gameCamera;
        private float curseTimer;
        private float nextCurseInterval;

        private void Awake()
        {
            gameCamera = Camera.main;
            ScheduleNextCurse();
        }

        private void OnEnable()
        {
            MeteorController.OnMeteorCompleted += HandleMeteorCompleted;
        }

        private void OnDisable()
        {
            MeteorController.OnMeteorCompleted -= HandleMeteorCompleted;
        }

        // GameManager'ın Awake'i garanti çalışmış olsun diye abonelik Start'ta yapılır.
        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Oynanış dışına çıkıldığında sahnedeki madenler toplanır ve ceza sayacı
        /// sıfırlanır; yeni bölüm ekranda asılı kalmış madenlerle başlamasın.
        /// </summary>
        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Paused) return;

            curseTimer = 0f;
            ScheduleNextCurse();

            if (state == GameState.Gameplay) return;

            MineController.DespawnAll();
        }

        /// <summary>Ana meteor tamamen yok edildi: ödül madeni düşebilir.</summary>
        private void HandleMeteorCompleted(int rootNumber, Vector3 position)
        {
            if (!GameManager.IsGameplayActive) return;
            if (!IsRewardEnabled()) return;
            if (Random.value > rewardDropChance * GetRewardRateMultiplier()) return;

            SpawnMine(GetRandomMine(rewardMines), position);
        }

        private void Update()
        {
            if (!GameManager.IsGameplayActive) return;
            if (!IsCurseEnabled()) return;

            curseTimer += Time.deltaTime;
            if (curseTimer < nextCurseInterval) return;

            curseTimer = 0f;
            ScheduleNextCurse();
            SpawnMine(GetRandomMine(curseMines), GetRightEdgeSpawnPosition());
        }

        /// <summary>
        /// Bir sonraki cezanın gecikmesini belirler. Gezegenin ceza çarpanı arttıkça
        /// aralık kısalır, ancak <see cref="curseIntervalFloor"/> altına inemez:
        /// aralık ceza etki süresinden kısalırsa cezalar üst üste biner, oyuncu
        /// yavaşlatmadan ve meteor yağmurundan hiç çıkamaz — bu ceza değil felçtir.
        /// </summary>
        private void ScheduleNextCurse()
        {
            float interval = Random.Range(minCurseInterval, maxCurseInterval);
            float multiplier = GetCurseRateMultiplier();

            if (multiplier > 0f)
            {
                interval /= multiplier;
            }

            nextCurseInterval = Mathf.Max(interval, curseIntervalFloor);
        }

        private void SpawnMine(MineDataSO data, Vector3 position)
        {
            if (data == null) return;
            if (minePool == null) return;

            GameObject instance = minePool.Spawn(position, Quaternion.identity);
            if (!instance.TryGetComponent(out MineController mine)) return;

            mine.SetData(data);
        }

        private static MineDataSO GetRandomMine(MineDataSO[] mines)
        {
            if (mines == null || mines.Length == 0) return null;

            return mines[Random.Range(0, mines.Length)];
        }

        private Vector3 GetRightEdgeSpawnPosition()
        {
            if (gameCamera == null) return Vector3.zero;

            float viewportY = Random.Range(minViewportY, maxViewportY);
            Vector3 worldPoint = gameCamera.ViewportToWorldPoint(
                new Vector3(1f, viewportY, gameCamera.nearClipPlane));

            return new Vector3(worldPoint.x + spawnMargin, worldPoint.y, 0f);
        }

        private bool IsRewardEnabled()
        {
            PlanetDataSO planetData = GetPlanetData();
            return planetData == null || planetData.AreRewardMinesEnabled;
        }

        private bool IsCurseEnabled()
        {
            PlanetDataSO planetData = GetPlanetData();
            return planetData == null || planetData.AreCurseMinesEnabled;
        }

        private float GetRewardRateMultiplier()
        {
            PlanetDataSO planetData = GetPlanetData();
            return planetData == null ? 1f : planetData.RewardRateMultiplier;
        }

        private float GetCurseRateMultiplier()
        {
            PlanetDataSO planetData = GetPlanetData();
            return planetData == null ? 1f : planetData.CurseRateMultiplier;
        }

        private static PlanetDataSO GetPlanetData()
        {
            return LevelManager.Instance != null ? LevelManager.Instance.CurrentPlanetData : null;
        }
    }
}
