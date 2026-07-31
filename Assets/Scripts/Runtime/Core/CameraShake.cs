using DG.Tweening;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Core
{
    /// <summary>
    /// Kamerayı DOTween ile sarsar. Hasar, patlama gibi anlarda çağrılır.
    /// Kameraya eklenir; erişim static Instance üzerindendir.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("Varsayılan Sarsıntı")]
        [Tooltip("Sarsıntı süresi (saniye)")]
        [SerializeField] private float defaultDuration = 0.3f;
        [Tooltip("Sarsıntı şiddeti (dünya birimi)")]
        [SerializeField] private float defaultStrength = 0.35f;
        [Tooltip("Süre boyunca kaç kez titreyeceği")]
        [SerializeField] private int vibrato = 14;
        [Tooltip("Titreme yönlerinin rastgeleliği (derece)")]
        [SerializeField, Range(0f, 180f)] private float randomness = 90f;

        private Transform selfTransform;
        private Vector3 basePosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            selfTransform = transform;
            basePosition = selfTransform.localPosition;
        }

        private void OnDestroy()
        {
            if (selfTransform != null)
            {
                selfTransform.DOKill();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Shake()
        {
            Shake(defaultDuration, defaultStrength);
        }

        public void Shake(float duration, float strength)
        {
            // Üst üste gelen sarsıntılarda kamera kaymasın diye önceki tween iptal edilip
            // pozisyon başa alınır.
            selfTransform.DOKill();
            selfTransform.localPosition = basePosition;

            // Z ekseni sarsılmaz; 2D kamerada derinlik değişimi istenmez.
            Vector3 shakeStrength = new Vector3(strength, strength, 0f);

            selfTransform
                .DOShakePosition(duration, shakeStrength, vibrato, randomness, fadeOut: true)
                .OnComplete(ResetPosition);
        }

        private void ResetPosition()
        {
            selfTransform.localPosition = basePosition;
        }
    }
}
