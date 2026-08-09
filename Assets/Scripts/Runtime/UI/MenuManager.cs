using DG.Tweening;
using MaskTransitions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Ana menüdeki panel akışını yönetir:
    /// - Ayarlar butonu → ayarlar paneli scale + CanvasGroup fade ile açılır,
    ///   panel içindeki çıkış butonuyla aynı animasyon tersine oynar.
    /// - Başla butonu → seçim paneli sağdan merkeze kayarken menü buton grubu
    ///   sola doğru ekrandan çıkar; geri butonu bu geçişi tersine çevirir.
    ///
    /// Panellerin sahnedeki yerleşimi "görünür konum" kabul edilir; ekran dışı
    /// konumlar root canvas genişliğine göre hesaplanır, yani tasarımcının
    /// panelleri elle ekran dışına taşımasına gerek yoktur.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [Header("Ayarlar Paneli")]
        [SerializeField] private Button ayarlarBtn;
        [SerializeField] private Button exitBtn;
        [SerializeField] private RectTransform ayarlarPanel;
        [SerializeField] private CanvasGroup ayarlarPanelCanvasGroup;

        [Header("Menü Butonları")]
        [Tooltip("Ayarlar açılırken sönümlenen, seçim panelinde sola kayan menü buton grubu")]
        [SerializeField] private RectTransform butonlarPanel;
        [SerializeField] private CanvasGroup butonlarPanelCanvasGroup;

        [Header("Seçim Paneli")]
        [SerializeField] private Button baslaBtn;
        [SerializeField] private Button backBtn;
        [SerializeField] private RectTransform selectionPanel;
        [SerializeField] private CanvasGroup selectionPanelCanvasGroup;

        [Header("Oyuna Geçiş")]
        [SerializeField] private Button oynaBtn;
        [SerializeField] private string gameplaySceneName = "GamePlay";

        [Header("Ayarlar Animasyonu")]
        [Tooltip("Ayarlar paneli kapalıyken sahip olduğu ölçek")]
        [SerializeField] private float hiddenScale = 0.7f;
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField] private Ease closeEase = Ease.InBack;

        [Header("Seçim Geçişi")]
        [SerializeField] private float slideDuration = 0.45f;
        [SerializeField] private Ease slideEase = Ease.InOutCubic;
        [Tooltip("Ekran dışı konumun canvas genişliğine ek olarak kaydırılma payı")]
        [SerializeField] private float hiddenExtraMargin = 100f;

        private Sequence ayarlarSequence;
        private Sequence selectionSequence;
        private bool isLoadingScene;

        private Vector2 butonlarShownPosition;
        private Vector2 butonlarHiddenPosition;
        private Vector2 selectionShownPosition;
        private Vector2 selectionHiddenPosition;

        private void Awake()
        {
            if (ayarlarBtn != null)
            {
                ayarlarBtn.onClick.AddListener(OpenAyarlarPanel);
            }

            if (exitBtn != null)
            {
                exitBtn.onClick.AddListener(CloseAyarlarPanel);
            }

            if (baslaBtn != null)
            {
                baslaBtn.onClick.AddListener(OpenSelectionPanel);
            }

            if (backBtn != null)
            {
                backBtn.onClick.AddListener(CloseSelectionPanel);
            }

            if (oynaBtn != null)
            {
                oynaBtn.onClick.AddListener(StartGameplay);
            }

            CacheAnchoredPositions();
            ApplyAyarlarClosedState();
            ApplySelectionClosedState();

            if (butonlarPanelCanvasGroup != null)
            {
                butonlarPanelCanvasGroup.alpha = 1f;
            }

            SetButtonsInteractable(true);
        }

        private void OnDestroy()
        {
            KillAyarlarSequence();
            KillSelectionSequence();
        }

        /// <summary>
        /// Panellerin Inspector'daki yerleşimi hedef konum kabul edilir; menü
        /// butonları canvas genişliği kadar sola, seçim paneli aynı kadar sağa
        /// alınarak ekran dışı konumlar üretilir.
        /// </summary>
        private void CacheAnchoredPositions()
        {
            float offset = GetCanvasWidth() + hiddenExtraMargin;

            if (butonlarPanel != null)
            {
                butonlarShownPosition = butonlarPanel.anchoredPosition;
                butonlarHiddenPosition = butonlarShownPosition - new Vector2(offset, 0f);
            }

            if (selectionPanel == null) return;

            selectionShownPosition = selectionPanel.anchoredPosition;
            selectionHiddenPosition = selectionShownPosition + new Vector2(offset, 0f);
        }

        private float GetCanvasWidth()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return Screen.width;

            RectTransform canvasRect = canvas.rootCanvas.transform as RectTransform;
            if (canvasRect == null) return Screen.width;

            return canvasRect.rect.width;
        }

        #region Ayarlar Paneli

        public void OpenAyarlarPanel()
        {
            if (ayarlarPanel == null || ayarlarPanelCanvasGroup == null) return;

            KillAyarlarSequence();

            ayarlarPanel.gameObject.SetActive(true);
            ayarlarPanel.localScale = Vector3.one * hiddenScale;
            ayarlarPanelCanvasGroup.alpha = 0f;

            SetButtonsInteractable(false);

            // Menü duraklatılmış olabileceği için tween timeScale'den bağımsız çalışır.
            Sequence sequence = DOTween.Sequence()
                .Append(ayarlarPanel.DOScale(1f, openDuration).SetEase(openEase))
                .Join(ayarlarPanelCanvasGroup.DOFade(1f, openDuration));

            JoinButtonsFade(sequence, 0f, openDuration);

            ayarlarSequence = sequence
                .OnComplete(() =>
                {
                    ayarlarSequence = null;
                    SetAyarlarInteractable(true);
                })
                .SetUpdate(true);
        }

        public void CloseAyarlarPanel()
        {
            if (ayarlarPanel == null || ayarlarPanelCanvasGroup == null) return;

            KillAyarlarSequence();
            SetAyarlarInteractable(false);

            Sequence sequence = DOTween.Sequence()
                .Append(ayarlarPanel.DOScale(hiddenScale, closeDuration).SetEase(closeEase))
                .Join(ayarlarPanelCanvasGroup.DOFade(0f, closeDuration));

            JoinButtonsFade(sequence, 1f, closeDuration);

            ayarlarSequence = sequence
                .OnComplete(() =>
                {
                    ayarlarSequence = null;
                    ApplyAyarlarClosedState();
                    SetButtonsInteractable(true);
                })
                .SetUpdate(true);
        }

        /// <summary>Menü buton grubunu hedef alpha'ya götüren tween'i sequence'e ekler.</summary>
        private void JoinButtonsFade(Sequence sequence, float targetAlpha, float duration)
        {
            if (butonlarPanelCanvasGroup == null) return;

            sequence.Join(butonlarPanelCanvasGroup.DOFade(targetAlpha, duration));
        }

        private void ApplyAyarlarClosedState()
        {
            if (ayarlarPanel == null || ayarlarPanelCanvasGroup == null) return;

            ayarlarPanel.localScale = Vector3.one * hiddenScale;
            ayarlarPanelCanvasGroup.alpha = 0f;
            SetAyarlarInteractable(false);
            ayarlarPanel.gameObject.SetActive(false);
        }

        private void SetAyarlarInteractable(bool isInteractable)
        {
            if (ayarlarPanelCanvasGroup == null) return;

            ayarlarPanelCanvasGroup.interactable = isInteractable;
            ayarlarPanelCanvasGroup.blocksRaycasts = isInteractable;
        }

        #endregion

        #region Seçim Paneli

        /// <summary>
        /// Seçim panelini sağdan merkeze getirir, menü butonlarını aynı anda
        /// sola doğru ekrandan çıkarır.
        /// </summary>
        public void OpenSelectionPanel()
        {
            if (selectionPanel == null || selectionPanelCanvasGroup == null) return;

            KillSelectionSequence();
            SetButtonsInteractable(false);

            selectionPanel.gameObject.SetActive(true);
            selectionPanel.anchoredPosition = selectionHiddenPosition;
            selectionPanelCanvasGroup.alpha = 1f;

            Sequence sequence = DOTween.Sequence()
                .Append(selectionPanel.DOAnchorPos(selectionShownPosition, slideDuration).SetEase(slideEase));

            JoinButtonsSlide(sequence, butonlarHiddenPosition);

            selectionSequence = sequence
                .OnComplete(() =>
                {
                    selectionSequence = null;
                    SetSelectionInteractable(true);
                })
                .SetUpdate(true);
        }

        /// <summary>Açılış geçişini tersine oynatır: panel sağa çıkar, butonlar geri gelir.</summary>
        public void CloseSelectionPanel()
        {
            if (selectionPanel == null || selectionPanelCanvasGroup == null) return;

            KillSelectionSequence();
            SetSelectionInteractable(false);

            Sequence sequence = DOTween.Sequence()
                .Append(selectionPanel.DOAnchorPos(selectionHiddenPosition, slideDuration).SetEase(slideEase));

            JoinButtonsSlide(sequence, butonlarShownPosition);

            selectionSequence = sequence
                .OnComplete(() =>
                {
                    selectionSequence = null;
                    ApplySelectionClosedState();
                    SetButtonsInteractable(true);
                })
                .SetUpdate(true);
        }

        private void JoinButtonsSlide(Sequence sequence, Vector2 targetPosition)
        {
            if (butonlarPanel == null) return;

            sequence.Join(butonlarPanel.DOAnchorPos(targetPosition, slideDuration).SetEase(slideEase));
        }

        private void ApplySelectionClosedState()
        {
            if (selectionPanel == null || selectionPanelCanvasGroup == null) return;

            selectionPanel.anchoredPosition = selectionHiddenPosition;
            SetSelectionInteractable(false);
            selectionPanel.gameObject.SetActive(false);
        }

        private void SetSelectionInteractable(bool isInteractable)
        {
            if (selectionPanelCanvasGroup == null) return;

            selectionPanelCanvasGroup.interactable = isInteractable;
            selectionPanelCanvasGroup.blocksRaycasts = isInteractable;
        }

        #endregion

        /// <summary>
        /// Seçim panelindeki oyna butonuna bağlanır; maske geçişiyle oyun
        /// sahnesini yükler. Sahnede <see cref="TransitionManager"/> yoksa
        /// doğrudan sahne yüklemesine düşer.
        /// </summary>
        public void StartGameplay()
        {
            if (isLoadingScene) return;

            isLoadingScene = true;

            KillAyarlarSequence();
            KillSelectionSequence();

            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadLevel(gameplaySceneName);
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            if (butonlarPanelCanvasGroup == null) return;

            butonlarPanelCanvasGroup.interactable = isInteractable;
            butonlarPanelCanvasGroup.blocksRaycasts = isInteractable;
        }

        private void KillAyarlarSequence()
        {
            if (ayarlarSequence == null) return;

            ayarlarSequence.Kill();
            ayarlarSequence = null;
        }

        private void KillSelectionSequence()
        {
            if (selectionSequence == null) return;

            selectionSequence.Kill();
            selectionSequence = null;
        }
    }
}
