using System;
using DG.Tweening;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Oyuncunun canlarını tutar ve HUD'daki kalp göstergelerini yönetir.
    /// Can gidince ilgili kalp DOTween ile alpha + scale animasyonuyla söner.
    /// </summary>
    public class HealthManager : MonoBehaviour
    {
        public static HealthManager Instance { get; private set; }

        [Header("Kalp Göstergeleri")]
        [Tooltip("Soldan sağa sıralı kalp nesneleri. Can gittikçe sondaki kalpten başlayarak söner.")]
        [SerializeField] private RectTransform[] hearts;

        [Header("Sönme Animasyonu")]
        [Tooltip("Kalbin sönme süresi (saniye)")]
        [SerializeField] private float fadeDuration = 0.35f;
        [Tooltip("Sönmeden önce kalbin ne kadar büyüyeceği (nabız etkisi)")]
        [SerializeField] private float punchScale = 1.3f;
        [Tooltip("Nabız (büyüme) süresi")]
        [SerializeField] private float punchDuration = 0.15f;
        [Tooltip("Sönerken küçüleceği hedef ölçek")]
        [SerializeField] private float fadeOutScale = 0f;

        /// <summary>Parametreler: güncel can, maksimum can.</summary>
        public event Action<int, int> OnLivesChanged;

        /// <summary>Tüm canlar bittiğinde tetiklenir.</summary>
        public event Action OnAllLivesLost;

        public int CurrentLives { get; private set; }
        public int MaxLives => hearts != null ? hearts.Length : 0;
        public bool IsAlive => CurrentLives > 0;

        private CanvasGroup[] heartCanvasGroups;
        private Vector3[] heartBaseScales;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CacheHearts();
            ResetHealth();
        }

        private void OnDestroy()
        {
            KillHeartTweens();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void CacheHearts()
        {
            if (hearts == null) return;

            heartCanvasGroups = new CanvasGroup[hearts.Length];
            heartBaseScales = new Vector3[hearts.Length];

            for (int index = 0; index < hearts.Length; index++)
            {
                RectTransform heart = hearts[index];
                if (heart == null) continue;

                heart.TryGetComponent(out heartCanvasGroups[index]);
                heartBaseScales[index] = heart.localScale;
            }
        }

        /// <summary>Bir can eksiltir; son kalpten başlayarak sönme animasyonunu oynatır.</summary>
        public void LoseLife()
        {
            if (!IsAlive) return;

            CurrentLives--;

            // Kalpler soldan sağa dizili; en sağdaki (index = CurrentLives) söner.
            PlayHeartLostAnimation(CurrentLives);

            OnLivesChanged?.Invoke(CurrentLives, MaxLives);

            if (IsAlive) return;

            OnAllLivesLost?.Invoke();
        }

        /// <summary>Bölüm yeniden başladığında canları ve kalp görsellerini ilk hâline döndürür.</summary>
        public void ResetHealth()
        {
            CurrentLives = MaxLives;
            RestoreAllHearts();
            OnLivesChanged?.Invoke(CurrentLives, MaxLives);
        }

        private void PlayHeartLostAnimation(int heartIndex)
        {
            if (!IsValidHeartIndex(heartIndex)) return;

            RectTransform heart = hearts[heartIndex];
            CanvasGroup canvasGroup = heartCanvasGroups[heartIndex];
            Vector3 baseScale = heartBaseScales[heartIndex];

            heart.DOKill();

            Sequence sequence = DOTween.Sequence().SetTarget(heart).SetUpdate(true);
            sequence.Append(heart.DOScale(baseScale * punchScale, punchDuration).SetEase(Ease.OutBack));
            sequence.Append(heart.DOScale(baseScale * fadeOutScale, fadeDuration).SetEase(Ease.InBack));

            if (canvasGroup == null) return;

            canvasGroup.DOKill();
            sequence.Join(canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true));
        }

        private void RestoreAllHearts()
        {
            if (hearts == null) return;

            for (int index = 0; index < hearts.Length; index++)
            {
                RectTransform heart = hearts[index];
                if (heart == null) continue;

                heart.DOKill();
                heart.localScale = heartBaseScales[index];

                CanvasGroup canvasGroup = heartCanvasGroups[index];
                if (canvasGroup == null) continue;

                canvasGroup.DOKill();
                canvasGroup.alpha = 1f;
            }
        }

        private void KillHeartTweens()
        {
            if (hearts == null) return;

            for (int index = 0; index < hearts.Length; index++)
            {
                if (hearts[index] != null)
                {
                    hearts[index].DOKill();
                }

                if (heartCanvasGroups != null && heartCanvasGroups[index] != null)
                {
                    heartCanvasGroups[index].DOKill();
                }
            }
        }

        private bool IsValidHeartIndex(int heartIndex)
        {
            if (hearts == null) return false;
            if (heartIndex < 0 || heartIndex >= hearts.Length) return false;

            return hearts[heartIndex] != null;
        }
    }
}
