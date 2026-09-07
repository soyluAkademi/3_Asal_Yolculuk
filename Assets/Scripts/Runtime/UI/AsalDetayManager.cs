using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Bölüm boyunca tamamen parçalanan her ana meteor için detay listesine bir
    /// satır (asalDetayHolder) ekler. Satırlar meteor bittiği anda oluşturulur;
    /// panel kapalıyken de kayıt tutulabilsin diye dinleyici Awake'te bağlanır.
    /// Yeni bölüm başladığında liste sıfırlanır.
    /// </summary>
    public class AsalDetayManager : MonoBehaviour
    {
        /// <summary>Listede gösterilecek tek bir meteorun verisi.</summary>
        public readonly struct MeteorDetay
        {
            /// <summary>Vuruş sırası (1'den başlar).</summary>
            public int Order { get; }

            /// <summary>Parçalanan ana meteorun sayısı.</summary>
            public int Number { get; }

            /// <summary>Sayının asal çarpanları; asal bir meteorda tek eleman içerir.</summary>
            public IReadOnlyList<int> PrimeFactors { get; }

            /// <summary>Meteorun kendisi asalsa true; bu satır "bölünmez" olarak gösterilir.</summary>
            public bool IsPrime { get; }

            public MeteorDetay(int order, int number, IReadOnlyList<int> primeFactors, bool isPrime)
            {
                Order = order;
                Number = number;
                PrimeFactors = primeFactors;
                IsPrime = isPrime;
            }
        }

        [Header("Liste")]
        [Tooltip("Satırların ekleneceği ScrollRect içeriği (content)")]
        [SerializeField] private RectTransform content;
        [Tooltip("Her meteor için üretilecek satır prefabı (asalDetayHolder)")]
        [SerializeField] private GameObject asalDetayHolderPrefab;

        private readonly List<GameObject> spawnedHolders = new List<GameObject>();
        private readonly List<MeteorDetay> details = new List<MeteorDetay>();
        private GameState previousState = GameState.MainMenu;

        /// <summary>Bu bölümde parçalanan meteorların kayıtları, vuruş sırasına göre.</summary>
        public IReadOnlyList<MeteorDetay> Details => details;

        private void Awake()
        {
            MeteorController.OnMeteorCompleted += HandleMeteorCompleted;
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            MeteorController.OnMeteorCompleted -= HandleMeteorCompleted;

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Duraklatmadan dönüşte de <see cref="GameState.Gameplay"/> durumuna
        /// geçildiği için liste yalnızca gerçek bölüm başlangıcında, yani bir
        /// önceki durum <see cref="GameState.Paused"/> değilken sıfırlanır.
        /// </summary>
        private void HandleGameStateChanged(GameState state)
        {
            bool isLevelStart = state == GameState.Gameplay && previousState != GameState.Paused;
            previousState = state;

            if (!isLevelStart) return;

            ClearList();
        }

        /// <summary>
        /// Meteorun konumu bu liste için gerekmez; yalnızca sayısı kaydedilir.
        /// </summary>
        private void HandleMeteorCompleted(int rootNumber, Vector3 position)
        {
            AddDetail(rootNumber);
        }

        private void AddDetail(int number)
        {
            List<int> primeFactors = PrimeMathEngine.GetPrimeFactors(number);
            MeteorDetay detail = new MeteorDetay(details.Count + 1, number, primeFactors, PrimeMathEngine.IsPrime(number));

            details.Add(detail);
            SpawnHolder(detail);
        }

        private void SpawnHolder(MeteorDetay detail)
        {
            if (asalDetayHolderPrefab == null) return;
            if (content == null) return;

            GameObject holder = Instantiate(asalDetayHolderPrefab, content);
            holder.name = $"asalDetayHolder_{detail.Order}_{detail.Number}";

            if (holder.TryGetComponent(out AsalDetayHolderView view))
            {
                view.Bind(detail);
            }

            spawnedHolders.Add(holder);
        }

        /// <summary>Yeni bölüm başlarken önceki bölümün satırlarını temizler.</summary>
        public void ClearList()
        {
            for (int index = 0; index < spawnedHolders.Count; index++)
            {
                if (spawnedHolders[index] == null) continue;

                Destroy(spawnedHolders[index]);
            }

            spawnedHolders.Clear();
            details.Clear();
        }
    }
}
