using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Gezegen kilit ilerlemesini kalıcı olarak tutan statik yardımcı.
    /// Tek bir değer saklanır: açılmış en yüksek gezegen index'i (0 tabanlı).
    /// Kilitler sırayla açıldığı için tek sayı yeterlidir; ayrı ayrı bayrak
    /// tutmak "sırası gelmeden açılmama" kuralını kendiliğinden bozardı.
    ///
    /// Not: <c>SaveManager</c> geldiğinde bu alan oraya taşınacak; anahtar adı
    /// korunursa mevcut oyuncu ilerlemesi kaybolmaz (bkz. YAPILACAKLAR 4.1).
    /// </summary>
    public static class PlanetProgress
    {
        public const string MaxUnlockedIndexKey = "AsalYolculuk.MaxUnlockedPlanetIndex";

        /// <summary>İlk gezegen oyunun başında açıktır.</summary>
        private const int FirstUnlockedIndex = 0;

        public static int MaxUnlockedIndex => PlayerPrefs.GetInt(MaxUnlockedIndexKey, FirstUnlockedIndex);

        public static bool IsUnlocked(int planetIndex) => planetIndex <= MaxUnlockedIndex;

        /// <summary>
        /// Gezegeni açar. Değer yalnızca **ileri** gider; daha düşük bir index
        /// gelirse yok sayılır (bölüm tekrar oynandığında ilerleme geri alınmasın).
        /// </summary>
        public static void Unlock(int planetIndex)
        {
            if (planetIndex <= MaxUnlockedIndex) return;

            PlayerPrefs.SetInt(MaxUnlockedIndexKey, planetIndex);
            PlayerPrefs.Save();
        }

        /// <summary>Normal akışta kullanılmaz; "ilerlemeyi sıfırla" ve test için.</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.SetInt(MaxUnlockedIndexKey, FirstUnlockedIndex);
            PlayerPrefs.Save();
        }
    }
}
