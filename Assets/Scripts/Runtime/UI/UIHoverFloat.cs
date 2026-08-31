using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Fare butonun üzerine geldiğinde RectTransform'u verilen ofset kadar
    /// yumuşakça kaydırır; imleç ayrılınca başlangıç konumuna geri döner.
    /// Kilitli (interactable = false) butonlarda efekt çalışmaz.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UIHoverFloat : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Hareket")]
        [Tooltip("Üzerine gelindiğinde uygulanacak konum ofseti (piksel). X: sağ-sol, Y: yukarı-aşağı.")]
        [SerializeField] private Vector2 hoverOffset = new Vector2(0f, 10f);

        [Tooltip("Hareketin yumuşaklığı. Büyük değer = daha hızlı gidip gelir.")]
        [SerializeField, Range(1f, 30f)] private float moveSpeed = 10f;

        [Header("Ölçek (opsiyonel)")]
        [Tooltip("Hover sırasında uygulanacak ek ölçek. 1 = ölçek değişmez.")]
        [SerializeField] private float hoverScale = 1.05f;

        [Header("Kilit Kontrolü")]
        [Tooltip("Buton bileşeni atanırsa interactable = false iken efekt çalışmaz.")]
        [SerializeField] private Button targetButton;

        private RectTransform rectTransform;
        private Vector2 basePosition;
        private Vector3 baseScale;
        private float hoverWeight;
        private bool isPointerInside;
        private bool isHoverAllowed = true;
        private bool isCached;

        private void Awake()
        {
            CacheReferences();
        }

        /// <summary>
        /// Sahne yüklenirken Awake sırası objeden objeye değişir; başka bir
        /// bileşenin OnEnable'ı bu Awake'ten önce SetHoverAllowed çağırabildiği
        /// için referanslar tembel (lazy) olarak da hazırlanır.
        /// </summary>
        private void CacheReferences()
        {
            if (isCached) return;

            isCached = true;
            rectTransform = (RectTransform)transform;
            basePosition = rectTransform.anchoredPosition;
            baseScale = rectTransform.localScale;

            if (targetButton == null)
            {
                TryGetComponent(out targetButton);
            }
        }

        private void OnDisable()
        {
            ResetToBase();
        }

        private void Update()
        {
            float target = CanHover() && isPointerInside ? 1f : 0f;

            if (Mathf.Approximately(hoverWeight, target)) return;

            hoverWeight = Mathf.MoveTowards(hoverWeight, target, moveSpeed * Time.unscaledDeltaTime);

            float eased = Mathf.SmoothStep(0f, 1f, hoverWeight);
            rectTransform.anchoredPosition = basePosition + hoverOffset * eased;
            rectTransform.localScale = baseScale * Mathf.Lerp(1f, hoverScale, eased);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
        }

        /// <summary>
        /// Kilit durumuna göre efekti açar/kapatır. Kapatıldığında buton
        /// anında başlangıç konumuna döner.
        /// </summary>
        public void SetHoverAllowed(bool value)
        {
            CacheReferences();

            isHoverAllowed = value;
            if (!value)
            {
                ResetToBase();
            }
        }

        /// <summary>
        /// Panel kapanırsa veya konum sıfırlanmak istenirse çağrılır.
        /// </summary>
        public void ResetToBase()
        {
            CacheReferences();

            isPointerInside = false;
            hoverWeight = 0f;
            rectTransform.anchoredPosition = basePosition;
            rectTransform.localScale = baseScale;
        }

        private bool CanHover()
        {
            if (!isHoverAllowed) return false;
            if (targetButton == null) return true;
            return targetButton.interactable;
        }
    }
}
