using PayeGames.AsalYolculuk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Seçim panelindeki gezegen kilitlerini kayıtlı ilerlemeye göre günceller.
    /// Dizideki sıra gezegen sırasıdır: index 0 = 1. gezegen. Kilitler sırayla
    /// açılır — ilerleme <see cref="PlanetProgress"/> üzerinden okunduğu için
    /// sırası gelmeyen gezegen hiçbir koşulda açık görünmez.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        [Header("Kilitler (1. gezegenden 8.'ye sıralı)")]
        [SerializeField] private Image[] lockImages;

        [Header("Kilit Sprite'ları")]
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;

        private void OnEnable()
        {
            RefreshLocks();
        }

        /// <summary>
        /// Her kilidin sprite'ını açık/kapalı durumuna göre yazar. Panel her
        /// açıldığında çalışır; oyundan menüye dönüldüğünde ilerleme tazelenir.
        /// </summary>
        public void RefreshLocks()
        {
            if (lockImages == null) return;

            int maxUnlockedIndex = PlanetProgress.MaxUnlockedIndex;

            for (int i = 0; i < lockImages.Length; i++)
            {
                Image lockImage = lockImages[i];
                if (lockImage == null) continue;

                bool isUnlocked = i <= maxUnlockedIndex;
                Sprite targetSprite = isUnlocked ? unlockedSprite : lockedSprite;

                if (targetSprite == null) continue;

                lockImage.sprite = targetSprite;
            }
        }
    }
}
