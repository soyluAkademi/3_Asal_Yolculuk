using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;
using PayeGames.AsalYolculuk.Player;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Üç can da bittiğinde sağdan sola kayarak gelen oyun sonu paneli.
    /// Görünür konum Inspector'daki yerleşimden alınır; gizli konum canvas
    /// genişliğine göre otomatik hesaplandığı için paneli sahnede ekran dışına
    /// taşımaya gerek yoktur — panel istenen (görünür) yerinde bırakılır.
    ///
    /// Panel, ölüm sekansı (gemi patlaması + meteor temizliği) tamamen bitince açılır.
    /// Sekansın süresi sahnedeki meteor sayısına göre değiştiğinden zamanlama sabit bir
    /// gecikmeden değil, <see cref="PlayerDeathEffect.OnDeathSequenceCompleted"/>
    /// event'inden alınır.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class GameOverPanelView : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Gemideki PlayerDeathEffect. Bağlanmazsa panel LevelFailed durumundan " +
                 "fallbackShowDelay kadar sonra açılır (patlamaların üstüne binebilir).")]
        [SerializeField] private PlayerDeathEffect playerDeathEffect;

        [Header("Gezegen Bilgisi")]
        [Tooltip("Kaldığın gezegenin numaralı adı (örn. \"1. Başlangıç Gezegeni\")")]
        [SerializeField] private TMP_Text kaldiginGezegenTxt;
        [Tooltip("Kaldığın gezegenin görseli")]
        [SerializeField] private Image gezegenImg;

        [Header("Gizlenecek Nesneler")]
        [Tooltip("Panel gelirken sahneden kaybolacak nesneler. Oyun yeniden başlarken geri gelirler.")]
        [SerializeField] private GameObject[] hideOnGameOver;

        [Header("Animasyon")]
        [Tooltip("Ölüm sekansı bittikten sonra panelin gelmesi için beklenen süre")]
        [SerializeField] private float showDelay = 0.3f;
        [Tooltip("Panelin sağdan sola kayma süresi")]
        [SerializeField] private float slideDuration = 0.6f;
        [SerializeField] private Ease slideEase = Ease.OutBack;
        [Tooltip("CanvasGroup alpha geçiş süresi")]
        [SerializeField] private float fadeDuration = 0.35f;

        [Header("Konum")]
        [Tooltip("Gizli konumun canvas genişliğine ek olarak kaydırılma payı")]
        [SerializeField] private float hiddenExtraMargin = 100f;

        [Header("Güvenlik Ağı")]
        [Tooltip("playerDeathEffect bağlı değilken LevelFailed sonrası beklenecek süre")]
        [SerializeField] private float fallbackShowDelay = 3f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private Sequence activeSequence;
        private Coroutine fallbackRoutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            CacheAnchoredPositions();
            ApplyHiddenState();
        }

        private void Start()
        {
            // Manager ve gemi bileşenleri Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (playerDeathEffect != null)
            {
                playerDeathEffect.OnDeathSequenceCompleted += Show;
            }

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            KillActiveSequence();
            StopFallbackRoutine();

            if (playerDeathEffect != null)
            {
                playerDeathEffect.OnDeathSequenceCompleted -= Show;
            }

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        /// <summary>
        /// Panelin Inspector'daki yerleşimi hedef konum kabul edilir; gizli konum
        /// bunun canvas genişliği kadar sağına alınır. Böylece anchor/pivot ayarı
        /// ne olursa olsun panel kesinlikle ekran dışında başlar ve sağdan girer.
        /// </summary>
        private void CacheAnchoredPositions()
        {
            shownAnchoredPosition = rectTransform.anchoredPosition;

            float offset = GetCanvasWidth() + hiddenExtraMargin;

            hiddenAnchoredPosition = shownAnchoredPosition + new Vector2(offset, 0f);
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

            if (state != GameState.LevelFailed) return;
            if (playerDeathEffect != null) return;

            // Ölüm sekansını haber verecek bileşen yoksa panel sabit gecikmeyle açılır.
            StopFallbackRoutine();
            fallbackRoutine = StartCoroutine(ShowAfterFallbackDelay());
        }

        private IEnumerator ShowAfterFallbackDelay()
        {
            yield return new WaitForSecondsRealtime(fallbackShowDelay);

            fallbackRoutine = null;
            Show();
        }

        /// <summary>Paneli soldan sağa kaydırarak açar ve sahnedeki fazlalıkları gizler.</summary>
        public void Show()
        {
            KillActiveSequence();
            FillPlanetInfo();
            SetSceneObjectsVisible(false);

            rectTransform.anchoredPosition = hiddenAnchoredPosition;
            canvasGroup.alpha = 0f;

            // Panel gelirken timeScale değişebileceği için tween zamandan bağımsız çalışır.
            activeSequence = DOTween.Sequence()
                .AppendInterval(showDelay)
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition, slideDuration).SetEase(slideEase))
                .Join(canvasGroup.DOFade(1f, fadeDuration))
                .OnComplete(EnableInteraction)
                .SetUpdate(true);
        }

        /// <summary>
        /// Kaybedilen bölümün gezegen adını ve görselini yazar. Bölüm başarısız olduğunda
        /// seviye ilerlemediği için kaynak her zaman <see cref="LevelManager.CurrentPlanetData"/>'dır.
        /// </summary>
        private void FillPlanetInfo()
        {
            if (LevelManager.Instance == null) return;

            PlanetDataSO planetData = LevelManager.Instance.CurrentPlanetData;
            if (planetData == null) return;

            if (kaldiginGezegenTxt != null)
            {
                kaldiginGezegenTxt.text = planetData.DisplayName;
            }

            // Sprite atanmamış gezegende paneldeki mevcut görsel bozulmadan bırakılır.
            if (gezegenImg == null) return;
            if (planetData.PlanetSprite == null) return;

            gezegenImg.sprite = planetData.PlanetSprite;
        }

        /// <summary>Paneli animasyonsuz olarak gizli konuma alır ve gizlenen nesneleri geri getirir.</summary>
        public void Hide()
        {
            KillActiveSequence();
            StopFallbackRoutine();
            ApplyHiddenState();
            SetSceneObjectsVisible(true);
        }

        private void SetSceneObjectsVisible(bool isVisible)
        {
            if (hideOnGameOver == null) return;

            for (int index = 0; index < hideOnGameOver.Length; index++)
            {
                GameObject target = hideOnGameOver[index];
                if (target == null) continue;

                target.SetActive(isVisible);
            }
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

        private void StopFallbackRoutine()
        {
            if (fallbackRoutine == null) return;

            StopCoroutine(fallbackRoutine);
            fallbackRoutine = null;
        }
    }
}
