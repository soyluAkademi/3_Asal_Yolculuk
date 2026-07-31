using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.Meteors
{
    /// <summary>
    /// Bir bölümün sayı havuzu. GDD'deki %70 öncelikli liste / %30 rastgele aralık
    /// kuralını uygular ve tamamen yok edilen sayıları bölüm sonuna kadar havuzdan
    /// çıkarır. ScriptableObject'teki liste değiştirilmez; üzerinde çalışılan kopya
    /// bu sınıfta tutulur.
    /// </summary>
    public class LevelNumberPool
    {
        private const float PriorityNumberChance = 0.7f;
        private const int MaxRandomAttempts = 20;

        // Gezegen verisi yoksa kullanılacak güvenli varsayılan aralık.
        private const int FallbackMinNumber = 2;
        private const int FallbackMaxNumber = 99;

        private readonly List<int> availablePriorityNumbers = new List<int>();
        private readonly HashSet<int> completedNumbers = new HashSet<int>();

        public LevelNumberPool(PlanetDataSO planetData)
        {
            PlanetData = planetData;
            Reset();
        }

        public PlanetDataSO PlanetData { get; }

        /// <summary>Bölüm baştan başladığında (canlar bittiğinde) havuzu ilk hâline döndürür.</summary>
        public void Reset()
        {
            completedNumbers.Clear();
            availablePriorityNumbers.Clear();

            if (PlanetData == null) return;

            IReadOnlyList<int> priorityNumbers = PlanetData.PriorityNumberList;
            for (int index = 0; index < priorityNumbers.Count; index++)
            {
                availablePriorityNumbers.Add(priorityNumbers[index]);
            }
        }

        /// <summary>Bir meteor tamamen yok edildi; sayısı bölüm sonuna kadar tekrar üretilmez.</summary>
        public void RegisterCompleted(int number)
        {
            if (!completedNumbers.Add(number)) return;

            availablePriorityNumbers.Remove(number);
        }

        public int GetNumber()
        {
            bool usePriorityNumber = availablePriorityNumbers.Count > 0 && Random.value < PriorityNumberChance;
            if (usePriorityNumber)
            {
                int randomIndex = Random.Range(0, availablePriorityNumbers.Count);
                return availablePriorityNumbers[randomIndex];
            }

            return GetRangeNumber();
        }

        private int GetRangeNumber()
        {
            int minNumber = PlanetData != null ? PlanetData.MinNumber : FallbackMinNumber;
            int maxNumber = PlanetData != null ? PlanetData.MaxNumber : FallbackMaxNumber;

            int rangeSize = maxNumber - minNumber + 1;
            if (rangeSize <= 0) return minNumber;

            // Normal durumda aralığın çok küçük bir kısmı tamamlanmış olur; rastgele deneme yeterli.
            for (int attempt = 0; attempt < MaxRandomAttempts; attempt++)
            {
                int candidate = Random.Range(minNumber, maxNumber + 1);
                if (!completedNumbers.Contains(candidate)) return candidate;
            }

            // Aralık büyük ölçüde tükenmiş olabilir; rastgele bir noktadan başlayıp tarar.
            int startOffset = Random.Range(0, rangeSize);
            for (int step = 0; step < rangeSize; step++)
            {
                int candidate = minNumber + (startOffset + step) % rangeSize;
                if (!completedNumbers.Contains(candidate)) return candidate;
            }

            // Aralıktaki her sayı tamamlanmış: oyunun durmaması için kural gevşetilir.
            return Random.Range(minNumber, maxNumber + 1);
        }
    }
}
