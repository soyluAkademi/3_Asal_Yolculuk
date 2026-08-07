using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [Tooltip("Yolculuk boyunca keşfedilen farklı asal sayısının yazılacağı metin")]
        [SerializeField] private TMP_Text asalSayisiTxt;
        [Tooltip("Tüm oynayışlar boyunca biriken toplam puanın yazılacağı metin")]
        [SerializeField] private TMP_Text toplamPuanTxt;

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

        [Header("Kutlama Efekti")]
        [Tooltip("Gezegenler açılırken sahnede patlatılacak partikül prefab'ı (örn. confetti)")]
        [SerializeField] private GameObject celebrationEffectPrefab;
        [Tooltip("Sekans boyunca kaç efekt çıkacağı")]
        [SerializeField, Min(1)] private int celebrationCount = 10;
        [Tooltip("Efektlerin doğacağı kamera; boş bırakılırsa Camera.main kullanılır")]
        [SerializeField] private Camera celebrationCamera;
        [Tooltip("Ekran kenarlarından bırakılacak pay (viewport oranı); efektler bu alanın dışına düşmez")]
        [SerializeField, Range(0f, 0.45f)] private float celebrationEdgePadding = 0.15f;
        [Tooltip("Efektin kameradan uzaklığı; 2D kurulumda kamera düzleminin biraz önü")]
        [SerializeField] private float celebrationCameraDistance = 10f;
        [Tooltip("Efektin boyut çarpanı")]
        [SerializeField] private float celebrationScale = 1f;
        [Tooltip("Efektin kaç saniye sonra yok edileceği; 0 = prefab kendini yok eder")]
        [SerializeField] private float celebrationLifetime = 0f;

        [Header("Özet Panelleri")]
        [Tooltip("Keşfedilen asal sayısı paneli; gezegen sekansından sonra ilk açılır")]
        [SerializeField] private CanvasGroup asalSayiPanel;
        [Tooltip("Toplam puan paneli; asal panelinden sonra açılır")]
        [SerializeField] private CanvasGroup puanPanel;
        [Tooltip("Son çizgi dolduktan sonra ilk panelin açılması için beklenen süre")]
        [SerializeField] private float summaryStartDelay = 0.25f;
        [SerializeField] private float summaryPanelDuration = 0.45f;
        [SerializeField] private Ease summaryPanelEase = Ease.OutBack;
        [Tooltip("Paneller arasındaki bekleme süresi")]
        [SerializeField] private float summaryPanelInterval = 0.15f;
        [Tooltip("Panelin belirmeye başladığı ölçek oranı (0 = yoktan büyür)")]
        [SerializeField, Range(0f, 0.9f)] private float summaryPanelStartScale = 0f;

        [Header("Başla Butonu")]
        [Tooltip("Puan panelinden sonra açılan buton; açıldıktan sonra nabız gibi büyüyüp küçülür")]
        [SerializeField] private CanvasGroup baslaBtn;
        [Tooltip("Butonun büyüyeceği ölçek oranı (1.5 = %50 büyür)")]
        [SerializeField] private float pulseScale = 1.5f;
        [Tooltip("Tek yönlü büyüme süresi; gidiş-dönüş bunun iki katı sürer")]
        [SerializeField] private float pulseDuration = 0.6f;
        [SerializeField] private Ease pulseEase = Ease.InOutSine;

        [Header("Ana Menü Geçişi")]
        [Tooltip("Butona basıldığında yüklenecek sahne; Build Settings'e ekli olmalı")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Sayaç Animasyonu")]
        [Tooltip("Metnin 0'dan hedef değere sayma süresi")]
        [SerializeField] private float counterDuration = 0.8f;
        [Tooltip("Sona doğru yavaşlayan bir ease sayacı daha okunaklı yapar")]
        [SerializeField] private Ease counterEase = Ease.OutCubic;

        private RectTransform rectTransform;
        private CanvasGroup panelCanvasGroup;
        private Vector2 shownAnchoredPosition;
        private Vector2 hiddenAnchoredPosition;
        private CanvasGroup[] planetCanvasGroups;
        private Transform[] planetTransforms;
        private Vector3[] planetBaseScales;
        private Image[] lineImages;
        // Açılış sırası: asal paneli → puan paneli → başla butonu. Buton da aynı
        // scale + fade adımını kullandığı için diziye dahil edilir; sayaç metni
        // olmadığı için sayma adımı onda atlanır.
        private const int BaslaBtnIndex = 2;

        // Üç dizi de aynı index ile eşleşir: panel, içindeki sayaç metni, hedef değer.
        private CanvasGroup[] summaryPanels;
        private Vector3[] summaryBaseScales;
        private TMP_Text[] summaryTexts;
        private readonly int[] summaryTargetValues = new int[3];
        private Tween pulseTween;
        private Sequence activeSequence;
        private Sequence panelSequence;
        private Coroutine celebrationRoutine;
        private float celebrationSpreadDuration;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            panelCanvasGroup = GetComponent<CanvasGroup>();

            // Efekt kamerası runtime'da aranmasın diye burada bir kez çözülür.
            if (celebrationCamera == null)
            {
                celebrationCamera = Camera.main;
            }

            CacheAnchoredPositions();
            CachePlanets();
            CacheLines();
            CacheSummaryPanels();

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
            KillPulse();
            StopCelebration();

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

            CacheSummaryValues();

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
        /// Başla butonuna bağlanır: yolculuğu kapatıp ana menüye döner.
        ///
        /// Sahne değiştiği için sahnedeki tüm manager'lar (puan, can, seviye, keşif)
        /// yok olur; oyun tekrar başladığında her şey baştan kurulur. Kalıcı olması
        /// gereken tek veri toplam puandır, o da diske yazılır.
        /// </summary>
        public void GoToMainMenu()
        {
            // Buton tekrar tekrar tıklanıp sahne iki kez yüklenmesin.
            if (baslaBtn != null)
            {
                baslaBtn.interactable = false;
                baslaBtn.blocksRaycasts = false;
            }

            KillPulse();
            KillActiveSequence();
            KillPanelSequence();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SaveTotalScore();
            }

            // Bölüm duraklatılmışken bitiş paneline gelinmiş olabilir; menü donmasın.
            Time.timeScale = 1f;

            if (string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                Debug.LogError("[BitisManager] Ana menü sahne adı boş; geçiş yapılamadı.", this);
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Özet panellerini açılış sırasına göre toplar. Slotlardan biri boş
        /// bırakılırsa o adım sekansta atlanır.
        /// </summary>
        private void CacheSummaryPanels()
        {
            summaryPanels = new[] { asalSayiPanel, puanPanel, baslaBtn };
            // Başla butonunda sayaç yok; index hizası bozulmasın diye null bırakılır.
            summaryTexts = new TMP_Text[] { asalSayisiTxt, toplamPuanTxt, null };
            summaryBaseScales = new Vector3[summaryPanels.Length];

            for (int index = 0; index < summaryPanels.Length; index++)
            {
                CanvasGroup panel = summaryPanels[index];
                summaryBaseScales[index] = panel == null ? Vector3.one : ResolveBaseScale(panel.transform);
            }
        }

        /// <summary>
        /// Sayaçların sayacağı hedef değerleri okur. Metinler burada yazılmaz;
        /// panel açıldığında 0'dan bu değerlere doğru sayılır.
        /// </summary>
        private void CacheSummaryValues()
        {
            if (ScoreManager.Instance == null) return;

            summaryTargetValues[0] = ScoreManager.Instance.DiscoveredPrimeCount;
            summaryTargetValues[1] = ScoreManager.Instance.TotalScore;
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

            // Efektler ilk gezegenle birlikte başlar ve gezegen/çizgi sekansı boyunca sürer.
            activeSequence.AppendCallback(StartCelebration);

            int planetCount = planetTransforms != null ? planetTransforms.Length : 0;
            for (int index = 0; index < planetCount; index++)
            {
                AppendPlanetStep(index);
                AppendLineStep(index);
            }

            // Gezegen fazının süresi; efektler bu süreye eşit aralıklarla yayılır.
            float planetPhaseDuration = Mathf.Max(0f, activeSequence.Duration(false) - startDelay);
            celebrationSpreadDuration = planetPhaseDuration;

            AppendSummaryPanelSteps();

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

        /// <summary>
        /// Gezegen/çizgi sekansı bittikten sonra özet panellerini sırayla açar:
        /// her panel aynı anda hem büyür hem görünür hâle gelir.
        /// </summary>
        private void AppendSummaryPanelSteps()
        {
            if (summaryPanels == null) return;

            bool isFirstPanel = true;

            for (int index = 0; index < summaryPanels.Length; index++)
            {
                CanvasGroup panel = summaryPanels[index];
                if (panel == null) continue;

                // Gecikme yalnızca gerçekten açılacak paneller arasında uygulanır;
                // boş bırakılan slot sekansta boşluk bırakmaz.
                float delay = isFirstPanel ? summaryStartDelay : summaryPanelInterval;
                if (delay > 0f)
                {
                    activeSequence.AppendInterval(delay);
                }

                isFirstPanel = false;

                activeSequence.Append(panel.transform.DOScale(summaryBaseScales[index], summaryPanelDuration).SetEase(summaryPanelEase));
                activeSequence.Join(panel.DOFade(1f, summaryPanelDuration));

                // Sayaç sekansa eklenmez, panel açılınca bağımsız başlatılır; böylece
                // sekans sayma bitmeden bir sonraki adıma (butona) geçebilir.
                int counterIndex = index;
                activeSequence.AppendCallback(() => StartCounter(counterIndex));

                if (index != BaslaBtnIndex) continue;

                // Buton yerine oturur oturmaz nabız başlar; sekans bu noktada biter.
                activeSequence.AppendCallback(StartPulse);
            }
        }

        /// <summary>
        /// Kutlama efektlerini sırayla patlatmaya başlar. Tümü aynı anda değil,
        /// gezegen sekansı boyunca eşit aralıklarla ve her seferinde ekranın
        /// rastgele bir noktasında çıkar.
        /// </summary>
        private void StartCelebration()
        {
            StopCelebration();

            if (celebrationEffectPrefab == null) return;
            if (celebrationCamera == null) return;

            celebrationRoutine = StartCoroutine(CelebrationRoutine());
        }

        private IEnumerator CelebrationRoutine()
        {
            // Süre gezegen fazına yayılır; faz hesaplanamadıysa efektler arka arkaya çıkar.
            float interval = celebrationCount > 0 ? celebrationSpreadDuration / celebrationCount : 0f;

            for (int index = 0; index < celebrationCount; index++)
            {
                SpawnCelebrationEffect();

                if (interval <= 0f) continue;

                // Bölüm sonunda timeScale 0 olabildiği için gerçek zaman kullanılır.
                yield return new WaitForSecondsRealtime(interval);
            }

            celebrationRoutine = null;
        }

        private void SpawnCelebrationEffect()
        {
            Vector3 viewportPoint = new Vector3(
                Random.Range(celebrationEdgePadding, 1f - celebrationEdgePadding),
                Random.Range(celebrationEdgePadding, 1f - celebrationEdgePadding),
                celebrationCameraDistance);

            Vector3 worldPosition = celebrationCamera.ViewportToWorldPoint(viewportPoint);

            GameObject effect = Instantiate(celebrationEffectPrefab, worldPosition, celebrationEffectPrefab.transform.rotation);
            effect.transform.localScale = celebrationEffectPrefab.transform.localScale * celebrationScale;

            if (celebrationLifetime <= 0f) return;

            Destroy(effect, celebrationLifetime);
        }

        private void StopCelebration()
        {
            if (celebrationRoutine == null) return;

            StopCoroutine(celebrationRoutine);
            celebrationRoutine = null;
        }

        /// <summary>
        /// Başla butonunu sürekli büyütüp küçültür. Yoyo döngüsü olduğu için buton
        /// her zaman kendi temel ölçeğine geri döner.
        /// </summary>
        private void StartPulse()
        {
            KillPulse();

            if (baslaBtn == null) return;

            Vector3 baseScale = summaryBaseScales[BaslaBtnIndex];
            baslaBtn.transform.localScale = baseScale;

            pulseTween = baslaBtn.transform
                .DOScale(baseScale * pulseScale, pulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(pulseEase)
                .SetUpdate(true);

            // Tıklama ancak buton tamamen belirdikten sonra açılır.
            baslaBtn.interactable = true;
            baslaBtn.blocksRaycasts = true;
        }

        private void KillPulse()
        {
            if (pulseTween == null) return;

            pulseTween.Kill();
            pulseTween = null;
        }

        /// <summary>
        /// Panel yerleştiği anda metni 0'dan hedef değere saymaya başlar. Sekanstan
        /// bağımsız çalışır: sayma sürerken sekans bir sonraki adıma geçebilir.
        /// Hedef 0 ise tween oluşturulmaz; metin zaten 0 gösteriyor.
        /// </summary>
        private void StartCounter(int index)
        {
            if (summaryTexts == null) return;

            TMP_Text label = summaryTexts[index];
            if (label == null) return;

            int targetValue = summaryTargetValues[index];
            if (targetValue <= 0) return;

            // DOTween ara değeri kendi tutamadığı için yerel bir değişken üzerinden
            // okunup yazılır; SetTarget sayesinde tween metinle birlikte kill edilir.
            int displayedValue = 0;

            DOTween.To(
                    () => displayedValue,
                    value =>
                    {
                        displayedValue = value;
                        label.text = value.ToString();
                    },
                    targetValue,
                    counterDuration)
                .SetEase(counterEase)
                .SetTarget(label)
                .SetUpdate(true);
        }

        /// <summary>Tüm gezegenleri, çizgileri ve özet panellerini animasyon öncesi hâline döndürür.</summary>
        public void ResetToHiddenState()
        {
            StopCelebration();

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

            KillPulse();

            int summaryCount = summaryPanels != null ? summaryPanels.Length : 0;
            for (int index = 0; index < summaryCount; index++)
            {
                CanvasGroup panel = summaryPanels[index];
                if (panel == null) continue;

                panel.DOKill();
                panel.transform.DOKill();
                panel.alpha = 0f;
                panel.transform.localScale = summaryBaseScales[index] * summaryPanelStartScale;
            }

            // Buton belirmeden tıklanabilir kalmasın.
            if (baslaBtn != null)
            {
                baslaBtn.interactable = false;
                baslaBtn.blocksRaycasts = false;
            }

            ResetCounterTexts();
        }

        /// <summary>Sayaç metinlerini başlangıç değeri olan 0'a döndürür.</summary>
        private void ResetCounterTexts()
        {
            int textCount = summaryTexts != null ? summaryTexts.Length : 0;
            for (int index = 0; index < textCount; index++)
            {
                TMP_Text label = summaryTexts[index];
                if (label == null) continue;

                label.DOKill();
                label.text = "0";
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
