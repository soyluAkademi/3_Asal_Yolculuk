using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Bölüm tamamlandığında sağdan sola kayarak gelen bölüm sonu paneli.
    /// Görünür konum Inspector'daki yerleşimden alınır; gizli konum canvas
    /// genişliğine göre otomatik hesaplandığı için sahnede paneli ekran dışına
    /// taşımaya gerek yoktur — panel istenen (görünür) yerinde bırakılır.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelCompletePanelView : MonoBehaviour
    {
        [Header("Gezegen Bilgisi")]
        [SerializeField] private TMP_Text kaldiginGezegenTxt;
        [SerializeField] private TMP_Text hedefGezegenTxt;
        [SerializeField] private Image kaldiginImg;
        [SerializeField] private Image hedefImg;

        [Header("Animasyon")]
        [Tooltip("Bölüm bittikten sonra panelin gelmesi için beklenen süre (patlama efekti için pay)")]
        [SerializeField] private float showDelay = 0.5f;
        [Tooltip("Panelin sağdan sola kayma süresi")]
        [SerializeField] private float slideDuration = 0.6f;
        [SerializeField] private Ease slideEase = Ease.OutBack;
        [Tooltip("CanvasGroup alpha geçiş süresi")]
        [SerializeField] private float fadeDuration = 0.35f;

        [Header("Çıkış (sola)")]
        [Tooltip("Panelin sola doğru ekrandan çıkma süresi")]
        [SerializeField] private float exitDuration = 0.5f;
        [SerializeField] private Ease exitEase = Ease.InBack;

        [Header("Konum")]
        [Tooltip("Gizli konumun canvas genişliğine ek olarak kaydırılma payı")]
        [SerializeField] private float hiddenExtraMargin = 100f;

        [Header("Ses")]
        [Tooltip("Bölüm tamamlandığında çalacak kutlama jingle'ı (örn. levelUp)")]
        [SerializeField] private AudioClip levelCompleteClip;
        [SerializeField, Range(0f, 1f)] private float levelCompleteVolume = 1f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private Vector2 exitAnchoredPosition;
        private Sequence activeSequence;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            CacheAnchoredPositions();
            ApplyHiddenState();
        }

        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            KillActiveSequence();

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Panelin Inspector'daki yerleşimi hedef konum kabul edilir; gizli konum
        /// bunun canvas genişliği kadar sağına alınır. Böylece anchor/pivot ayarı
        /// ne olursa olsun panel kesinlikle ekran dışında başlar.
        /// </summary>
        private void CacheAnchoredPositions()
        {
            shownAnchoredPosition = rectTransform.anchoredPosition;

            float offset = GetCanvasWidth() + hiddenExtraMargin;

            // Panel her zaman sağdan girer, sola doğru çıkar.
            hiddenAnchoredPosition = shownAnchoredPosition + new Vector2(offset, 0f);
            exitAnchoredPosition = shownAnchoredPosition - new Vector2(offset, 0f);
        }

        private float GetCanvasWidth()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return Screen.width;

            RectTransform canvasRect = canvas.rootCanvas.transform as RectTransform;
            if (canvasRect == null) return Screen.width;

            return canvasRect.rect.width;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Gameplay)
            {
                Hide();
                return;
            }

            if (state != GameState.LevelComplete) return;

            // Son gezegende farklı bir bitiş paneli açılacak.
            if (LevelManager.Instance != null && LevelManager.Instance.IsLastLevel) return;

            Show();
        }

        public void Show()
        {
            KillActiveSequence();
            FillPlanetInfo();

            rectTransform.anchoredPosition = hiddenAnchoredPosition;
            canvasGroup.alpha = 0f;

            // Panel gelirken timeScale değişebileceği için tween zamandan bağımsız çalışır.
            activeSequence = DOTween.Sequence()
                .AppendInterval(showDelay)
                .AppendCallback(PlayLevelCompleteSfx)
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition, slideDuration).SetEase(slideEase))
                .Join(canvasGroup.DOFade(1f, fadeDuration))
                .OnComplete(EnableInteraction)
                .SetUpdate(true);
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
        /// Tamamlanan ve hedef gezegenin adını ("2. Asteroit Kuşağı") ve görselini yazar.
        /// Panel yalnızca son gezegen dışında açıldığı için hedef gezegen her zaman vardır.
        /// </summary>
        private void FillPlanetInfo()
        {
            if (LevelManager.Instance == null) return;

            ApplyPlanetData(LevelManager.Instance.CurrentPlanetData, kaldiginGezegenTxt, kaldiginImg);
            ApplyPlanetData(LevelManager.Instance.NextPlanetData, hedefGezegenTxt, hedefImg);
        }

        private static void ApplyPlanetData(PlanetDataSO planetData, TMP_Text nameText, Image planetImage)
        {
            if (planetData == null) return;

            if (nameText != null)
            {
                nameText.text = planetData.DisplayName;
            }

            if (planetImage == null) return;
            if (planetData.PlanetSprite == null) return;

            planetImage.sprite = planetData.PlanetSprite;
        }

        /// <summary>
        /// Paneli sola doğru ekrandan çıkarır ve bitiminde sağdaki gizli konuma
        /// geri yerleştirir; böylece bir sonraki bölüm sonunda yine sağdan girer.
        /// </summary>
        public void HideToLeft(Action onComplete)
        {
            KillActiveSequence();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            activeSequence = DOTween.Sequence()
                .Append(rectTransform.DOAnchorPos(exitAnchoredPosition, exitDuration).SetEase(exitEase))
                .Join(canvasGroup.DOFade(0f, exitDuration))
                .OnComplete(() =>
                {
                    // Geri çağrı yeni bölümü başlatıp Hide() tetikleyebilir; biten
                    // sequence'in tekrar Kill edilmemesi için referans önce bırakılır.
                    activeSequence = null;

                    ApplyHiddenState();
                    onComplete?.Invoke();
                })
                .SetUpdate(true);
        }

        /// <summary>Paneli animasyonsuz olarak sağdaki gizli konuma alır.</summary>
        public void Hide()
        {
            KillActiveSequence();
            ApplyHiddenState();
        }

        private void ApplyHiddenState()
        {
            rectTransform.anchoredPosition = hiddenAnchoredPosition;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void EnableInteraction()
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void KillActiveSequence()
        {
            if (activeSequence == null) return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
