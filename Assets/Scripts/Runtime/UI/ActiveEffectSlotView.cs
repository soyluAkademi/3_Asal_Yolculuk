using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Aktif etki göstergesindeki tek bir kutucuk: madenin ikonu ve geri sayımı.
    /// Hangi etkiyi göstereceğine <see cref="ActiveEffectHUDView"/> karar verir;
    /// bu sınıf yalnızca kendisine verileni çizer ve giriş/çıkış geçişini yönetir.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ActiveEffectSlotView : MonoBehaviour
    {
        // Geri sayım yazıları önceden hazırlanır; Update içinde string üretilmez.
        private const int MaxCachedSeconds = 60;
        private static readonly string[] SecondLabels = BuildSecondLabels();

        // Geçiş süreleri tüm slot'larda aynı; kutucuk başına ayarlanacak bir şey yok.
        private const float FadeInDuration = 0.25f;
        private const float FadeOutDuration = 0.2f;

        // Slot düzeni her zaman aynı: ikon objenin kendisinde, geri sayım altında.
        // Bu yüzden referanslar Inspector'a açılmaz, çalışma anında bulunur.
        private Image iconImage;
        private TMP_Text timeTxt;
        private CanvasGroup canvasGroup;
        private Tween fadeTween;

        // Aynı saniye tekrar yazılmasın; TMP_Text'e her karede atama yapmak boşuna iş.
        private int lastShownSeconds = -1;
        private bool isVisible;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
        }

        private void ResolveReferences()
        {
            if (iconImage == null)
            {
                TryGetComponent(out iconImage);
            }

            if (canvasGroup == null)
            {
                TryGetComponent(out canvasGroup);
            }

            if (timeTxt == null)
            {
                timeTxt = GetComponentInChildren<TMP_Text>(includeInactive: true);
            }
        }

        /// <summary>
        /// Slotu görünür yapar. Zaten görünürse yalnızca içeriği tazelenir —
        /// yandaki bir etki bittiğinde slot'lar yeniden dağıtıldığı için, ekranda
        /// duran kutucuk boşuna yeniden solup açılmasın.
        /// </summary>
        public void Show(Sprite icon, Color timeColor)
        {
            // Önce aktif edilir: slot sahnede kapalı başladıysa Awake henüz
            // çalışmamıştır ve referanslar boş olurdu.
            gameObject.SetActive(true);
            ResolveReferences();

            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            if (timeTxt != null)
            {
                timeTxt.color = timeColor;
            }

            if (isVisible) return;

            isVisible = true;
            lastShownSeconds = -1;

            fadeTween?.Kill();

            // CanvasGroup yoksa geçiş yapılamaz ama kutucuk yine de görünmeli.
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;
            fadeTween = canvasGroup.DOFade(1f, FadeInDuration).SetUpdate(true);
        }

        /// <summary>Solarak kaybolur; animasyon bitince obje kapatılır.</summary>
        public void Hide()
        {
            if (!isVisible && !gameObject.activeSelf) return;

            isVisible = false;
            fadeTween?.Kill();

            // Kapalı objede veya CanvasGroup yokken tween çalışmaz; doğrudan gizle.
            if (canvasGroup == null || !gameObject.activeInHierarchy)
            {
                HideImmediate();
                return;
            }

            fadeTween = canvasGroup.DOFade(0f, FadeOutDuration)
                .SetUpdate(true)
                .OnComplete(HideImmediate);
        }

        /// <summary>
        /// Animasyonsuz gizler. Bölüm başında tüm slot'ları sıfırlarken kullanılır;
        /// oyuncu açılışta yedi kutucuğun tek tek solmasını görmemeli.
        /// </summary>
        public void HideImmediate()
        {
            ResolveReferences();

            fadeTween?.Kill();
            fadeTween = null;
            isVisible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(false);
        }

        /// <summary>Kalan süreyi yukarı yuvarlayarak yazar: 5.3 sn → "6s".</summary>
        public void SetRemainingTime(float remainingTime)
        {
            if (timeTxt == null) return;

            int seconds = Mathf.Max(0, Mathf.CeilToInt(remainingTime));
            if (seconds == lastShownSeconds) return;

            lastShownSeconds = seconds;
            timeTxt.text = seconds < SecondLabels.Length
                ? SecondLabels[seconds]
                : $"{seconds}s";
        }

        private static string[] BuildSecondLabels()
        {
            string[] labels = new string[MaxCachedSeconds + 1];

            for (int second = 0; second < labels.Length; second++)
            {
                labels[second] = $"{second}s";
            }

            return labels;
        }
    }
}
