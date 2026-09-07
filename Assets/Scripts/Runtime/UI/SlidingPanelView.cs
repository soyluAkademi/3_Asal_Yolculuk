using System;
using DG.Tweening;
using UnityEngine;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Ekranın sağından girip soluna doğru çıkan panellerin ortak davranışı.
    /// Görünür konum Inspector'daki yerleşimden alınır; gizli ve çıkış konumları
    /// canvas genişliğine göre hesaplandığı için panelleri sahnede ekran dışına
    /// taşımaya gerek yoktur — panel istenen (görünür) yerinde bırakılır.
    /// Panel sola çıktıktan sonra tekrar sağdaki gizli konuma alınır; böylece
    /// her açılışta yine sağdan girer.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class SlidingPanelView : MonoBehaviour
    {
        [Header("Giriş (sağdan)")]
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

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private Vector2 exitAnchoredPosition;
        private Sequence activeSequence;

        protected virtual void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            CacheAnchoredPositions();
            ApplyHiddenState();
        }

        protected virtual void OnDestroy()
        {
            KillActiveSequence();
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

        /// <summary>Paneli sağdaki gizli konumdan görünür konuma kaydırır.</summary>
        public void ShowFromRight()
        {
            ShowFromRight(0f, null);
        }

        /// <summary>
        /// Paneli gecikmeli olarak sağdan getirir. <paramref name="onSlideStart"/>
        /// gecikme bittiğinde, panel kaymaya başladığı anda çağrılır (ses vb. için).
        /// </summary>
        public void ShowFromRight(float delay, Action onSlideStart)
        {
            KillActiveSequence();
            ApplyHiddenState();

            // Panel gelirken timeScale değişebileceği için tween zamandan bağımsız çalışır.
            activeSequence = DOTween.Sequence()
                .AppendInterval(delay)
                .AppendCallback(() => onSlideStart?.Invoke())
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition, slideDuration).SetEase(slideEase))
                .Join(canvasGroup.DOFade(1f, fadeDuration))
                .OnComplete(EnableInteraction)
                .SetUpdate(true);
        }

        /// <summary>
        /// Paneli sola doğru ekrandan çıkarır ve bitiminde sağdaki gizli konuma
        /// geri yerleştirir; böylece bir sonraki açılışta yine sağdan girer.
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
                    // Geri çağrı yeni bir animasyon başlatabilir; biten sequence'in
                    // tekrar Kill edilmemesi için referans önce bırakılır.
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

        protected void KillActiveSequence()
        {
            if (activeSequence == null) return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
