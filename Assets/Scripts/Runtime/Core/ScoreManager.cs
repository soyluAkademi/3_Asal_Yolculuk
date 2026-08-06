using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Bölüm puanını ve yolculuk boyunca keşfedilen farklı asal sayıları tutar;
    /// değişimleri event ile yayınlar. UI bu event'i dinler.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Puan Ayarları")]
        [Tooltip("Bölümü tamamlamak için gereken puan")]
        [SerializeField] private int targetScore = 100;
        [Tooltip("Ana meteor yok edildiğinde kazanılan puan")]
        [SerializeField] private int scorePerMeteor = 5;

        /// <summary>Parametreler: güncel puan, hedef puan.</summary>
        public event Action<int, int> OnScoreChanged;

        /// <summary>Hedef puana ulaşıldığında bir kez tetiklenir. GameManager dinler.</summary>
        public event Action OnTargetReached;

        public int CurrentScore { get; private set; }
        public int TargetScore => targetScore;

        // Yolculuk boyunca keşfedilen FARKLI asallar. SortedSet olduğu için aynı
        // asal kaç kez çıkarsa çıksın bir kez sayılır ve liste sıralı gelir.
        private readonly SortedSet<int> discoveredPrimes = new SortedSet<int>();

        /// <summary>Keşfedilen farklı asal sayısı; bitiş panelinde gösterilir.</summary>
        public int DiscoveredPrimeCount => discoveredPrimes.Count;

        /// <summary>Küçükten büyüğe sıralı, benzersiz asal listesi.</summary>
        public IReadOnlyCollection<int> DiscoveredPrimes => discoveredPrimes;

        private const string TotalScoreKey = "AsalYolculuk.TotalScore";

        /// <summary>
        /// Oyuncunun tüm oynayışları boyunca biriktirdiği puan. Bölüm sonunda veya
        /// oyun baştan başladığında sıfırlanmaz; <see cref="PlayerPrefs"/> üzerinde
        /// saklandığı için oyun kapatılıp açıldığında da korunur.
        /// </summary>
        public int TotalScore { get; private set; }

        private bool isTargetReached;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            TotalScore = PlayerPrefs.GetInt(TotalScoreKey, 0);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            SaveTotalScore();
            Instance = null;
        }

        /// <summary>
        /// Uygulama arka plana alındığında diske yazılır. WebGL ve mobilde
        /// <c>OnApplicationQuit</c> her zaman çalışmadığı için asıl güvence budur.
        /// </summary>
        private void OnApplicationPause(bool isPaused)
        {
            if (!isPaused) return;

            SaveTotalScore();
        }

        private void OnApplicationQuit()
        {
            SaveTotalScore();
        }

        public void AddMeteorScore()
        {
            AddScore(scorePerMeteor);
        }

        /// <summary>
        /// Tamamen ayrıştırılan meteorun kök sayısını asal çarpanlarına ayırıp keşif
        /// listesine ekler; 24 için 2 ve 3, 7 için 7 kaydedilir. Zaten keşfedilmiş
        /// asal tekrar sayılmaz: 7'den sonra 21 ayrıştırılırsa yalnızca 3 eklenir.
        /// Yalnızca meteor tüm parçalarıyla yok edildiğinde çağrılır; ekrandan kaçan
        /// veya gemiye çarpan meteorun çarpanları keşfedilmiş sayılmaz.
        /// </summary>
        public void RegisterDiscoveredPrimes(int rootNumber)
        {
            List<int> primeFactors = PrimeMathEngine.GetPrimeFactors(rootNumber);

            for (int index = 0; index < primeFactors.Count; index++)
            {
                discoveredPrimes.Add(primeFactors[index]);
            }
        }

        public void AddScore(int amount)
        {
            if (amount == 0) return;

            CurrentScore = Mathf.Max(0, CurrentScore + amount);
            OnScoreChanged?.Invoke(CurrentScore, targetScore);

            AddTotalScore(amount);
            TryRaiseTargetReached();
        }

        /// <summary>
        /// Toplam puana yalnızca kazanılan puan eklenir; ceza/eksi puan toplamı
        /// düşürmez, aksi halde bitiş ekranındaki toplam geri sayabilir.
        /// Disk yazımı burada yapılmaz (bkz. <see cref="SaveTotalScore"/>);
        /// <see cref="PlayerPrefs.SetInt"/> yalnızca bellekteki değeri günceller.
        /// </summary>
        private void AddTotalScore(int amount)
        {
            if (amount <= 0) return;

            TotalScore += amount;
            PlayerPrefs.SetInt(TotalScoreKey, TotalScore);
        }

        /// <summary>
        /// Toplam puanı diske yazar. Sık çağrılmamalı: WebGL'de bu işlem
        /// IndexedDB'ye flush yapar. Bölüm sonunda ve uygulama kapanırken çağrılır.
        /// </summary>
        public void SaveTotalScore()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Biriken toplam puanı sıfırlar. Normal oyun akışında çağrılmaz; ayarlar
        /// ekranındaki "ilerlemeyi sıfırla" gibi bir işlem için vardır.
        /// </summary>
        public void ResetTotalScore()
        {
            TotalScore = 0;
            PlayerPrefs.SetInt(TotalScoreKey, 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Canlar bittiğinde veya bölüm yeniden başladığında çağrılır. Yalnızca bölüm
        /// puanı sıfırlanır; toplam puan ve keşfedilen asallar bilinçli olarak
        /// korunur, tüm yolculuk boyunca birikir.
        /// </summary>
        public void ResetScore()
        {
            CurrentScore = 0;
            isTargetReached = false;
            OnScoreChanged?.Invoke(CurrentScore, targetScore);
        }

        /// <summary>
        /// Hedefe ulaşıldığında bölüm sonu akışını başlatır. Bölüm bittikten sonra
        /// akan puanlar (patlamakta olan meteorlar) event'i tekrar tetiklemesin diye
        /// bayrakla korunur.
        /// </summary>
        private void TryRaiseTargetReached()
        {
            if (isTargetReached) return;
            if (CurrentScore < targetScore) return;

            isTargetReached = true;
            OnTargetReached?.Invoke();
        }
    }
}
