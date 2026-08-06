using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Bitiş panelini (son gezegen tamamlandığında) ekrana getirir ve içindeki
    /// yolculuk özeti animasyonunu oynatır: gezegenler sırayla büyüyerek belirir,
    /// her gezegenden sonra kendisiyle bir sonraki arasındaki çizgi dolar
    /// (1. gezegen → 1. çizgi → 2. gezegen → 2. çizgi ...).
    ///
    /// Gezegen ve çizgi nesneleri hiyerarşi sırasına göre eşleştirilir; çizgi
    /// sayısı gezegen sayısından bir eksiktir (son gezegenden sonra çizgi yoktur).
    ///
    /// Panelin kendisine <see cref="LevelCompletePanelView"/> eklenmez; o script
    /// bölüm sonu paneline aittir ve son gezegende bilinçli olarak açılmaz.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class BitisManager : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Gezegen nesnelerinin parent'ı; her çocuğunda CanvasGroup olmalı")]
        [SerializeField] private Transform gezegenler;
        [Tooltip("Çizgi nesnelerinin parent'ı; her çocuğunda Filled tipinde Image olmalı")]
        [SerializeField] private Transform lines;

        [Header("Panel Girişi")]
        [Tooltip("Son bölüm bittikten sonra panelin gelmesi için beklenen süre")]
        [SerializeField] private float showDelay = 0.8f;
        [Tooltip("Panelin sağdan sola kayma süresi")]
        [SerializeField] private float slideDuration = 0.6f;
        [SerializeField] private Ease slideEase = Ease.OutBack;
        [SerializeField] private float fadeDuration = 0.35f;
        [Tooltip("Gizli konumun canvas genişliğine ek olarak sağa kaydırılma payı")]
        [SerializeField] private float hiddenExtraMargin = 100f;

        [Header("Gezegen Animasyonu")]
        [Tooltip("Panel yerleştikten sonra gezegen sekansının başlaması için beklenen süre")]
        [SerializeField] private float startDelay = 0.3f;
        [SerializeField] private float planetDuration = 0.4f;
        [SerializeField] private Ease planetEase = Ease.OutBack;
        [Tooltip("Gezegenin belirmeye başladığı ölçek oranı (0 = yoktan büyür)")]
        [SerializeField, Range(0f, 0.9f)] private float planetStartScale = 0f;

        [Header("Çizgi Animasyonu")]
        [SerializeField] private float lineDuration = 0.35f;
        [SerializeField] private Ease lineEase = Ease.Linear;
        [Tooltip("Gezegen belirdikten sonra çizginin dolmaya başlaması için beklenen süre")]
        [SerializeField] private float planetToLineDelay = 0.05f;

        private RectTransform rectTransform;
        private CanvasGroup panelCanvasGroup;
        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private CanvasGroup[] planetCanvasGroups;
        private Transform[] planetTransforms;
        private Vector3[] planetBaseScales;
        private Image[] lineImages;
        private Sequence activeSequence;
        private Sequence panelSequence;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            panelCanvasGroup = GetComponent<CanvasGroup>();

            CacheAnchoredPositions();
            CachePlanets();
            CacheLines();

            // Panel ve içindekiler sahnede görünür bırakılır; gizleme runtime'da yapılır.
            ApplyPanelHiddenState();
            ResetToHiddenState();
        }

        private void Start()
        {
            // GameManager Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            KillActiveSequence();
            KillPanelSequence();

            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state != GameState.LevelComplete) return;

            // Bitiş paneli yalnızca son gezegen tamamlandığında açılır; diğer
            // bölümlerde LevelCompletePanelView devreye girer.
            if (LevelManager.Instance == null) return;
            if (!LevelManager.Instance.IsLastLevel) return;

            ShowPanel();
        }

        /// <summary>
        /// Paneli sağdan sola kaydırarak getirir ve yerleşir yerleşmez gezegen
        /// sekansını başlatır.
        /// </summary>
        public void ShowPanel()
        {
            KillPanelSequence();
            ResetToHiddenState();

            rectTransform.anchoredPosition = hiddenAnchoredPosition;
            panelCanvasGroup.alpha = 0f;

            panelSequence = DOTween.Sequence()
                .AppendInterval(showDelay)
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition, slideDuration).SetEase(slideEase))
                .Join(panelCanvasGroup.DOFade(1f, fadeDuration))
                .OnComplete(() =>
                {
                    panelSequence = null;
                    EnableInteraction();
                    PlaySequence();
                })
                .SetUpdate(true);
        }

        /// <summary>
        /// Panelin Inspector'daki yerleşimi hedef konum kabul edilir; gizli konum
        /// bunun canvas genişliği kadar sağına alınır. Böylece anchor/pivot ayarı
        /// ne olursa olsun panel ekran dışında başlar.
        /// </summary>
        private void CacheAnchoredPositions()
        {
            shownAnchoredPosition = rectTransform.anchoredPosition;
            hiddenAnchoredPosition = shownAnchoredPosition + new Vector2(GetCanvasWidth() + hiddenExtraMargin, 0f);
        }

        private float GetCanvasWidth()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return Screen.width;

            RectTransform canvasRect = canvas.rootCanvas.transform as RectTransform;
            if (canvasRect == null) return Screen.width;

            return canvasRect.rect.width;
        }

        private void ApplyPanelHiddenState()
        {
            rectTransform.anchoredPosition = hiddenAnchoredPosition;
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        private void EnableInteraction()
        {
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        private void KillPanelSequence()
        {
            if (panelSequence == null) return;

            panelSequence.Kill();
            panelSequence = null;
        }

        private void CachePlanets()
        {
            if (gezegenler == null) return;

            int count = gezegenler.childCount;
            planetTransforms = new Transform[count];
            planetCanvasGroups = new CanvasGroup[count];
            planetBaseScales = new Vector3[count];

            for (int index = 0; index < count; index++)
            {
                Transform planet = gezegenler.GetChild(index);
                planetTransforms[index] = planet;
                planetBaseScales[index] = ResolveBaseScale(planet);
                planet.TryGetComponent(out planetCanvasGroups[index]);
            }
        }

        /// <summary>
        /// Gezegenin hedef ölçeği. Sahnede ölçek sıfır bırakılmışsa tween 0'dan 0'a
        /// gideceği için gezegen hiç büyümez; bu durumda birim ölçeğe düşülür.
        /// </summary>
        private static Vector3 ResolveBaseScale(Transform planet)
        {
            Vector3 scale = planet.localScale;

            return scale.sqrMagnitude > 0.0001f ? scale : Vector3.one;
        }

        private void CacheLines()
        {
            if (lines == null) return;

            int count = lines.childCount;
            lineImages = new Image[count];

            for (int index = 0; index < count; index++)
            {
                lines.GetChild(index).TryGetComponent(out lineImages[index]);
            }
        }

        /// <summary>
        /// Bitiş paneli ekrana yerleştikten sonra çağrılır. Sekans baştan kurulur;
        /// panel tekrar açılırsa animasyon yeniden oynar.
        /// </summary>
        public void PlaySequence()
        {
            KillActiveSequence();
            ResetToHiddenState();

            activeSequence = DOTween.Sequence().SetUpdate(true);
            activeSequence.AppendInterval(startDelay);

            int planetCount = planetTransforms != null ? planetTransforms.Length : 0;
            for (int index = 0; index < planetCount; index++)
            {
                AppendPlanetStep(index);
                AppendLineStep(index);
            }

            activeSequence.OnComplete(() => activeSequence = null);
        }

        /// <summary>Gezegen aynı anda hem büyür hem görünür hâle gelir.</summary>
        private void AppendPlanetStep(int index)
        {
            Transform planet = planetTransforms[index];
            if (planet == null) return;

            activeSequence.Append(planet.DOScale(planetBaseScales[index], planetDuration).SetEase(planetEase));

            CanvasGroup canvasGroup = planetCanvasGroups[index];
            if (canvasGroup == null) return;

            activeSequence.Join(canvasGroup.DOFade(1f, planetDuration));
        }

        /// <summary>
        /// Gezegenin ardından gelen çizgiyi doldurur. Son gezegenin çizgisi
        /// olmadığı için index taşarsa adım atlanır.
        /// </summary>
        private void AppendLineStep(int index)
        {
            if (lineImages == null) return;
            if (index >= lineImages.Length) return;

            Image line = lineImages[index];
            if (line == null) return;

            if (planetToLineDelay > 0f)
            {
                activeSequence.AppendInterval(planetToLineDelay);
            }

            activeSequence.Append(line.DOFillAmount(1f, lineDuration).SetEase(lineEase));
        }

        /// <summary>Tüm gezegenleri ve çizgileri animasyon öncesi hâline döndürür.</summary>
        public void ResetToHiddenState()
        {
            int planetCount = planetTransforms != null ? planetTransforms.Length : 0;
            for (int index = 0; index < planetCount; index++)
            {
                Transform planet = planetTransforms[index];
                if (planet == null) continue;

                planet.DOKill();
                planet.localScale = planetBaseScales[index] * planetStartScale;

                CanvasGroup canvasGroup = planetCanvasGroups[index];
                if (canvasGroup == null) continue;

                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
            }

            int lineCount = lineImages != null ? lineImages.Length : 0;
            for (int index = 0; index < lineCount; index++)
            {
                Image line = lineImages[index];
                if (line == null) continue;

                line.DOKill();
                line.fillAmount = 0f;
            }
        }

        private void KillActiveSequence()
        {
            if (activeSequence == null) return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
