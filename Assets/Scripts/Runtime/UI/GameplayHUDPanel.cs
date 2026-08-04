using DG.Tweening;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Oynanış sırasında görünen HUD panelleri (puan, can) için görünürlük yönetimi.
    /// Bölüm bittiğinde kaybolur, oynanışa dönüldüğünde geri gelir. Aynı bileşen
    /// hem scorePanel hem healthPanel üzerinde kullanılır.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class GameplayHUDPanel : MonoBehaviour
    {
        [Header("Gizlenme")]
        [Tooltip("Bölüm bittikten sonra panelin kaybolmaya başlaması için beklenen süre")]
        [SerializeField] private float hideDelay = 0.1f;
        [SerializeField] private float hideDuration = 0.4f;
        [Tooltip("Panelin gizlenirken kayacağı yön ve mesafe (0,0 = sadece solma)")]
        [SerializeField] private Vector2 hideOffset = Vector2.zero;
        [SerializeField] private Ease hideEase = Ease.InBack;

        [Header("Geri Gelme")]
        [SerializeField] private float showDuration = 0.35f;
        [SerializeField] private Ease showEase = Ease.OutBack;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Vector2 shownAnchoredPosition;
        private Sequence activeSequence;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            // Inspector'daki yerleşim panelin görünür konumu kabul edilir.
            shownAnchoredPosition = rectTransform.anchoredPosition;
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

        private void HandleGameStateChanged(GameState state)
        {
            // Duraklatmada HUD yerinde kalmalı; sadece bölüm bittiğinde kaybolur.
            if (state == GameState.Paused) return;

            if (state == GameState.Gameplay)
            {
                Show();
                return;
            }

            Hide();
        }

        public void Hide()
        {
            KillActiveSequence();

            activeSequence = DOTween.Sequence()
                .AppendInterval(hideDelay)
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition + hideOffset, hideDuration).SetEase(hideEase))
                .Join(canvasGroup.DOFade(0f, hideDuration))
                .OnComplete(DisableInteraction)
                .SetUpdate(true);
        }

        public void Show()
        {
            KillActiveSequence();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            activeSequence = DOTween.Sequence()
                .Append(rectTransform.DOAnchorPos(shownAnchoredPosition, showDuration).SetEase(showEase))
                .Join(canvasGroup.DOFade(1f, showDuration))
                .SetUpdate(true);
        }

        private void DisableInteraction()
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void KillActiveSequence()
        {
            if (activeSequence == null) return;

            activeSequence.Kill();
            activeSequence = null;
        }
    }
}
