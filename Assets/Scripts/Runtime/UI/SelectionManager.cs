using PayeGames.AsalYolculuk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Seçim panelindeki gezegen butonlarını yönetir: kilit görselleri, hover
    /// efektinin açık/kapalı olması, tıklanabilirlik ve hangi gezegenin seçili
    /// olduğunu gösteren chooseImg işareti.
    ///
    /// Dizilerdeki sıra gezegen sırasıdır: index 0 = 1. gezegen. Kilitler sırayla
    /// açılır — ilerleme <see cref="PlanetProgress"/> üzerinden okunduğu için
    /// sırası gelmeyen gezegen hiçbir koşulda açık görünmez veya seçilemez.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        [Header("Kilitler (1. gezegenden 8.'ye sıralı)")]
        [SerializeField] private Image[] lockImages;

        [Header("Gezegen Butonları (kilitlerle aynı sırada)")]
        [SerializeField] private Button[] planetButtons;

        [Header("Seçim İşaretleri / chooseImg (kilitlerle aynı sırada)")]
        [SerializeField] private GameObject[] chooseImages;

        [Header("Kilit Sprite'ları")]
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;

        // Kilit Image'ı derin bir child (btn_x/1.Gezegen/rectImg/kilitImg) olduğu
        // için hover efekti kilidin üstündeki ilk UIHoverFloat'tan bulunur;
        // böylece Inspector'da ikinci bir dizi doldurmaya gerek kalmaz.
        private UIHoverFloat[] hoverEffects;

        private void Awake()
        {
            CacheHoverEffects();
            RegisterPlanetButtons();
        }

        private void OnEnable()
        {
            RefreshLocks();
        }

        /// <summary>
        /// Kilit sprite'ını, hover efektini ve tıklanabilirliği açık/kapalı
        /// durumuna göre yazar; ardından seçimi açılmış **son** gezegene alır.
        /// Panel her açıldığında çalışır, yani oyuncu hiçbir gezegene tıklamadan
        /// Oyna'ya basarsa sıradaki (kaldığı) bölüm başlar.
        /// </summary>
        public void RefreshLocks()
        {
            if (lockImages == null) return;

            int maxUnlockedIndex = PlanetProgress.MaxUnlockedIndex;

            for (int i = 0; i < lockImages.Length; i++)
            {
                bool isUnlocked = i <= maxUnlockedIndex;

                ApplyHoverState(i, isUnlocked);
                ApplyButtonState(i, isUnlocked);

                Image lockImage = lockImages[i];
                if (lockImage == null) continue;

                Sprite targetSprite = isUnlocked ? unlockedSprite : lockedSprite;
                if (targetSprite == null) continue;

                lockImage.sprite = targetSprite;
            }

            // Panel her açılışta varsayılana döner: seçim, açılmış son gezegen.
            SelectPlanet(maxUnlockedIndex);
        }

        /// <summary>
        /// Gezegen butonuna basıldığında çağrılır: seçim kaydedilir ve yalnızca
        /// o gezegenin chooseImg'i açık kalır. Oyna butonu bu kayıttan hangi
        /// bölümün yükleneceğini okur (bkz. <see cref="LevelManager"/>).
        /// </summary>
        public void SelectPlanet(int planetIndex)
        {
            if (!PlanetProgress.IsUnlocked(planetIndex)) return;

            PlanetProgress.Select(planetIndex);
            ApplySelectionVisual(planetIndex);
        }

        private void ApplySelectionVisual(int selectedIndex)
        {
            if (chooseImages == null) return;

            for (int i = 0; i < chooseImages.Length; i++)
            {
                GameObject chooseImage = chooseImages[i];
                if (chooseImage == null) continue;

                chooseImage.SetActive(i == selectedIndex);
            }
        }

        private void ApplyHoverState(int index, bool isUnlocked)
        {
            if (hoverEffects == null || index >= hoverEffects.Length) return;

            UIHoverFloat hoverEffect = hoverEffects[index];
            if (hoverEffect == null) return;

            hoverEffect.SetHoverAllowed(isUnlocked);
        }

        private void ApplyButtonState(int index, bool isUnlocked)
        {
            if (planetButtons == null || index >= planetButtons.Length) return;

            Button planetButton = planetButtons[index];
            if (planetButton == null) return;

            planetButton.interactable = isUnlocked;
        }

        /// <summary>
        /// Tıklama davranışını bir kez bağlar; index closure ile yakalandığı
        /// için her buton kendi gezegenini seçer. Butonda AudioSource varsa
        /// tık sesi de çalınır (referans burada cache'lenir, tıklamada değil).
        /// </summary>
        private void RegisterPlanetButtons()
        {
            if (planetButtons == null) return;

            for (int i = 0; i < planetButtons.Length; i++)
            {
                Button planetButton = planetButtons[i];
                if (planetButton == null) continue;

                int planetIndex = i;

                if (planetButton.TryGetComponent(out AudioSource clickSource))
                {
                    planetButton.onClick.AddListener(() => SoundManager.PlayUiSfx(clickSource));
                }

                planetButton.onClick.AddListener(() => SelectPlanet(planetIndex));
            }
        }

        private void CacheHoverEffects()
        {
            if (lockImages == null)
            {
                hoverEffects = new UIHoverFloat[0];
                return;
            }

            hoverEffects = new UIHoverFloat[lockImages.Length];

            for (int i = 0; i < lockImages.Length; i++)
            {
                Image lockImage = lockImages[i];
                if (lockImage == null) continue;

                hoverEffects[i] = lockImage.GetComponentInParent<UIHoverFloat>(true);
            }
        }
    }
}
