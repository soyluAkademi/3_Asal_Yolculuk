using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Bölüm tamamlandığında sağdan sola kayarak gelen bölüm sonu paneli.
    /// Kayma davranışı <see cref="SlidingPanelView"/> tarafından yönetilir.
    /// Detay butonu bu paneli sola gönderip detay panelini sağdan getirir;
    /// geri dön butonu ise işlemi tersine çevirir.
    /// </summary>
    public class LevelCompletePanelView : SlidingPanelView
    {
        [Header("Gezegen Bilgisi")]
        [SerializeField] private TMP_Text kaldiginGezegenTxt;
        [SerializeField] private TMP_Text hedefGezegenTxt;
        [Tooltip("Tamamlanan gezegenin sirasi: \"1. GEZEGEN\"")]
        [SerializeField] private TMP_Text kaldiginGezegenNo;
        [Tooltip("Gecilecek gezegenin sirasi: \"2. GEZEGEN\"")]
        [SerializeField] private TMP_Text hedefGezegenNo;
        [SerializeField] private Image kaldiginImg;
        [SerializeField] private Image hedefImg;

        [Header("Detay Paneli")]
        [Tooltip("Bölüm sonu detaylarının gösterildiği panel (detayPanel)")]
        [SerializeField] private SlidingPanelView detailPanel;
        [Tooltip("Detay panelini açan buton")]
        [SerializeField] private Button detayBtn;
        [Tooltip("Detay panelinin içindeki geri dön butonu")]
        [SerializeField] private Button geriDonBtn;

        [Header("Animasyon")]
        [Tooltip("Bölüm bittikten sonra panelin gelmesi için beklenen süre (patlama efekti için pay)")]
        [SerializeField] private float showDelay = 0.5f;

        [Header("Ses")]
        [Tooltip("Bölüm tamamlandığında çalacak kutlama jingle'ı (örn. levelUp)")]
        [SerializeField] private AudioClip levelCompleteClip;
        [SerializeField, Range(0f, 1f)] private float levelCompleteVolume = 1f;

        private const string PlanetOrderFormat = "{0}. GEZEGEN";

        protected override void Awake()
        {
            base.Awake();

            RegisterButton(detayBtn, OpenDetailPanel);
            RegisterButton(geriDonBtn, CloseDetailPanel);
        }

        /// <summary>
        /// Butonun tıklama davranışını bağlar ve varsa üzerindeki AudioSource'u
        /// tık sesi olarak çalar. AudioSource referansı Awake'te bir kez cache'lenir;
        /// tıklama anında GetComponent çağrılmaz. Ses seviyesi ayarlardan okunduğu
        /// için efektler kapalıyken tık duyulmaz.
        /// </summary>
        private static void RegisterButton(Button button, UnityAction onClick)
        {
            if (button == null) return;

            if (button.TryGetComponent(out AudioSource clickSource))
            {
                button.onClick.AddListener(() => SoundManager.PlayUiSfx(clickSource));
            }

            button.onClick.AddListener(onClick);
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Tık sesi lambda ile eklendiği için listener'lar toplu kaldırılır;
            // Inspector'dan bağlanmış kalıcı listener'lar bundan etkilenmez.
            if (detayBtn != null)
            {
                detayBtn.onClick.RemoveAllListeners();
            }

            if (geriDonBtn != null)
            {
                geriDonBtn.onClick.RemoveAllListeners();
            }

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Gameplay)
            {
                Hide();

                if (detailPanel != null)
                {
                    detailPanel.Hide();
                }

                return;
            }

            if (state != GameState.LevelComplete) return;

            // Son gezegende farklı bir bitiş paneli açılacak.
            if (LevelManager.Instance != null && LevelManager.Instance.IsLastLevel) return;

            Show();
        }

        public void Show()
        {
            FillPlanetInfo();
            ShowFromRight(showDelay, PlayLevelCompleteSfx);
        }

        /// <summary>
        /// Bölüm panelini sola gönderir, tamamen çıktığında detay panelini
        /// sağdan getirir. İki panel aynı anda ekranda görünmez.
        /// </summary>
        private void OpenDetailPanel()
        {
            if (detailPanel == null) return;

            HideToLeft(detailPanel.ShowFromRight);
        }

        /// <summary>Detay panelini sola gönderir ve bölüm panelini sağdan geri getirir.</summary>
        private void CloseDetailPanel()
        {
            if (detailPanel == null) return;

            detailPanel.HideToLeft(ShowFromRight);
        }

        /// <summary>
        /// Jingle, patlama sesleri dinsin diye <see cref="showDelay"/> beklendikten
        /// sonra, panel kaymaya başladığı anda çalar.
        /// </summary>
        private void PlayLevelCompleteSfx()
        {
            if (levelCompleteClip == null) return;
            if (SoundManager.Instance == null) return;

            SoundManager.Instance.PlaySfx(levelCompleteClip, levelCompleteVolume);
        }

        /// <summary>
        /// Tamamlanan ve hedef gezegenin adını ("Asteroit Kuşağı") ve görselini yazar.
        /// Panel yalnızca son gezegen dışında açıldığı için hedef gezegen her zaman vardır.
        /// </summary>
        private void FillPlanetInfo()
        {
            if (LevelManager.Instance == null) return;

            ApplyPlanetData(LevelManager.Instance.CurrentPlanetData, kaldiginGezegenTxt, kaldiginGezegenNo, kaldiginImg);
            ApplyPlanetData(LevelManager.Instance.NextPlanetData, hedefGezegenTxt, hedefGezegenNo, hedefImg);
        }

        private static void ApplyPlanetData(PlanetDataSO planetData, TMP_Text nameText, TMP_Text orderText, Image planetImage)
        {
            if (planetData == null) return;

            if (nameText != null)
            {
                nameText.text = planetData.PlanetName;
            }

            if (orderText != null)
            {
                orderText.text = string.Format(PlanetOrderFormat, planetData.PlanetId);
            }

            if (planetImage == null) return;
            if (planetData.PlanetSprite == null) return;

            planetImage.sprite = planetData.PlanetSprite;
        }
    }
}
