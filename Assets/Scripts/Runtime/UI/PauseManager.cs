using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Duraklatma akışını yönetir: pauseBtn oyunu <see cref="GameState.Paused"/>
    /// durumuna alır (GameManager timeScale'i 0'lar), panel scale + alpha ile açılır.
    /// exitBtn veya oyunaDonBtn panel kapanışını oynatıp oyunu kaldığı yerden
    /// sürdürür. Tüm tween'ler timeScale'den bağımsız çalışır (SetUpdate(true)).
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Butonlar")]
        [SerializeField] private Button pauseBtn;
        [Tooltip("Panelin sağ üstündeki kapatma butonu")]
        [SerializeField] private Button exitBtn;
        [Tooltip("Oyuna dön butonu")]
        [SerializeField] private Button oyunaDonBtn;
        [Tooltip("Ana menüye dönüş butonu")]
        [SerializeField] private Button mainMenuBtn;

        [Header("Referanslar")]
        [Tooltip("Ana menüye dönüş akışını (puan kaydı + sahne geçişi) yürütür")]
        [SerializeField] private LevelFlowController levelFlowController;

        [Header("Panel")]
        [SerializeField] private RectTransform pausePanel;
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Açılış Animasyonu")]
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [Tooltip("Panelin açılmaya başladığı ölçek")]
        [SerializeField] private float hiddenScale = 0.7f;

        [Header("Kapanış Animasyonu")]
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField] private Ease closeEase = Ease.InBack;

        private AudioSource pauseBtnSource;
        private AudioSource exitBtnSource;
        private AudioSource oyunaDonBtnSource;
        private AudioSource mainMenuBtnSource;
        private Sequence activeSequence;

        private void Awake()
        {
            if (pausePanel != null && panelCanvasGroup == null)
            {
                pausePanel.TryGetComponent(out panelCanvasGroup);
            }

            if (pauseBtn != null)
            {
                pauseBtn.onClick.AddListener(OnPauseBtnClick);
                pauseBtn.TryGetComponent(out pauseBtnSource);
            }

            if (exitBtn != null)
            {
                exitBtn.onClick.AddListener(OnExitBtnClick);
                exitBtn.TryGetComponent(out exitBtnSource);
            }

            if (oyunaDonBtn != null)
            {
                oyunaDonBtn.onClick.AddListener(OnOyunaDonBtnClick);
                oyunaDonBtn.TryGetComponent(out oyunaDonBtnSource);
            }

            if (mainMenuBtn != null)
            {
                mainMenuBtn.onClick.AddListener(OnMainMenuBtnClick);
                mainMenuBtn.TryGetComponent(out mainMenuBtnSource);
            }

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

        /// <summary>Bölüm bittiğinde/başarısız olduğunda duraklatma butonu kapanır.</summary>
        private void HandleGameStateChanged(GameState state)
        {
            if (pauseBtn == null) return;

            pauseBtn.interactable = state == GameState.Gameplay;
        }

        private void OnPauseBtnClick()
        {
            SoundManager.PlayUiSfx(pauseBtnSource);
            Pause();
        }

        private void OnExitBtnClick()
        {
            SoundManager.PlayUiSfx(exitBtnSource);
            Resume();
        }

        private void OnOyunaDonBtnClick()
        {
            SoundManager.PlayUiSfx(oyunaDonBtnSource);
            Resume();
        }

        /// <summary>
        /// Ana menüye döner. Kalınan bölüm ve toplam puan kaydı
        /// <see cref="LevelFlowController.GoToMainMenu"/> içinde yazılır;
        /// duraklatma paneli kapanış animasyonu oynatılmaz çünkü sahne değişir.
        /// </summary>
        private void OnMainMenuBtnClick()
        {
            SoundManager.PlayUiSfx(mainMenuBtnSource);

            if (levelFlowController == null)
            {
                Debug.LogWarning("[PauseManager] levelFlowController atanmadı; ana menüye dönülemiyor.");
                return;
            }

            KillActiveSequence();

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }

            levelFlowController.GoToMainMenu();
        }

        public void Pause()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Gameplay) return;

            GameManager.Instance.SetState(GameState.Paused);
            PlayOpenAnimation();
        }

        public void Resume()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState != GameState.Paused) return;

            // Durum hemen Gameplay'e döner; oyun kaldığı yerden akarken panel
            // üzerine kapanış animasyonunu oynatır.
            GameManager.Instance.SetState(GameState.Gameplay);
            PlayCloseAnimation();
        }

        private void PlayOpenAnimation()
        {
            if (pausePanel == null) return;

            KillActiveSequence();

            pausePanel.gameObject.SetActive(true);
            pausePanel.localScale = Vector3.one * hiddenScale;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = true;
                panelCanvasGroup.interactable = true;
            }

            activeSequence = DOTween.Sequence()
                .Append(pausePanel.DOScale(1f, openDuration).SetEase(openEase))
                .SetUpdate(true);

            if (panelCanvasGroup != null)
            {
                activeSequence.Join(panelCanvasGroup.DOFade(1f, openDuration));
            }
        }

        private void PlayCloseAnimation()
        {
            if (pausePanel == null) return;

            KillActiveSequence();

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }

            activeSequence = DOTween.Sequence()
                .Append(pausePanel.DOScale(hiddenScale, closeDuration).SetEase(closeEase))
                .OnComplete(() =>
                {
                    activeSequence = null;
                    ApplyHiddenState();
                })
                .SetUpdate(true);

            if (panelCanvasGroup != null)
            {
                activeSequence.Join(panelCanvasGroup.DOFade(0f, closeDuration));
            }
        }

        private void ApplyHiddenState()
        {
            if (pausePanel == null) return;

            pausePanel.localScale = Vector3.one * hiddenScale;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }

            pausePanel.gameObject.SetActive(false);
        }

        private void KillActiveSequence()
        {
            if (activeSequence == null) return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
