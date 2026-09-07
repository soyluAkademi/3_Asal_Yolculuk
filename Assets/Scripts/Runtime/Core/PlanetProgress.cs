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

        /// <summary>Seçim panelinde işaretlenen ve oynanacak gezegen.</summary>
        public const string SelectedIndexKey = "AsalYolculuk.SelectedPlanetIndex";

        /// <summary>
        /// Tamamlanmış bölüm sayısı. Kilit açma (<see cref="MaxUnlockedIndexKey"/>)
        /// ile ayrı tutulur: son bölüm bitince açılacak yeni bölüm olmadığı için
        /// kilit index'i ilerlemez, ama bölüm tamamlanmıştır.
        /// </summary>
        public const string CompletedCountKey = "AsalYolculuk.CompletedPlanetCount";

        /// <summary>İlk gezegen oyunun başında açıktır.</summary>
        private const int FirstUnlockedIndex = 0;

        public static int MaxUnlockedIndex => PlayerPrefs.GetInt(MaxUnlockedIndexKey, FirstUnlockedIndex);

        public static bool IsUnlocked(int planetIndex) => planetIndex >= 0 && planetIndex <= MaxUnlockedIndex;

        /// <summary>
        /// Tamamlanmış bölüm sayısı. Varsayılanı <see cref="MaxUnlockedIndex"/>'tir;
        /// bu alan eklenmeden önce oynamış oyuncularda tamamlanan bölüm sayısı
        /// açılmış en yüksek index'e eşit olduğu için ilerleme kaybolmaz.
        /// </summary>
        public static int CompletedCount => PlayerPrefs.GetInt(CompletedCountKey, MaxUnlockedIndex);

        public static bool IsCompleted(int planetIndex) => planetIndex >= 0 && planetIndex < CompletedCount;

        /// <summary>
        /// Bölümü tamamlandı olarak işaretler. Değer yalnızca <b>ileri</b> gider;
        /// bitmiş bir bölüm tekrar oynandığında sayaç geri düşmez.
        /// </summary>
        public static void MarkCompleted(int planetIndex)
        {
            int completedCount = planetIndex + 1;
            if (completedCount <= CompletedCount) return;

            PlayerPrefs.SetInt(CompletedCountKey, completedCount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Oyuncunun seçim panelinde işaretlediği gezegen. Kayıt yoksa ya da
        /// ilerleme sıfırlandığı için seçim artık kilitliyse, açılmış en yüksek
        /// gezegene düşer; böylece oynanış hiçbir zaman kilitli bölüme gitmez.
        /// </summary>
        public static int SelectedIndex
        {
            get
            {
                int savedIndex = PlayerPrefs.GetInt(SelectedIndexKey, MaxUnlockedIndex);
                return IsUnlocked(savedIndex) ? savedIndex : MaxUnlockedIndex;
            }
        }

        /// <summary>
        /// Seçimi kaydeder. Kilitli gezegen seçilemez; aynı değer tekrar
        /// yazılmaz (gereksiz disk erişimi olmasın).
        /// </summary>
        public static void Select(int planetIndex)
        {
            if (!IsUnlocked(planetIndex)) return;
            if (PlayerPrefs.GetInt(SelectedIndexKey, -1) == planetIndex) return;

            PlayerPrefs.SetInt(SelectedIndexKey, planetIndex);
            PlayerPrefs.Save();
        }

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
            PlayerPrefs.SetInt(SelectedIndexKey, FirstUnlockedIndex);
            PlayerPrefs.SetInt(CompletedCountKey, 0);
            PlayerPrefs.Save();
        }
    }
}
