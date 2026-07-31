using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Gemiye çarpan meteoru yakalar: bir can eksiltir ve meteoru havuza iade eder.
    /// Çarpışma sonrası kısa bir dokunulmazlık penceresi vardır; böylece üst üste gelen
    /// meteorlar tek karede birden fazla can götürmez.
    /// Ölümcül olmayan hasarlarda hasar efekti oynar; son canda patlama sekansını
    /// <see cref="PlayerDeathEffect"/> devralır.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShipCollisionHandler : MonoBehaviour
    {
        [Header("Hasar")]
        [Tooltip("Çarpışmadan sonra kaç saniye boyunca hasar alınmayacağı")]
        [SerializeField] private float invulnerableDuration = 1.5f;

        [Header("Hasar Efekti")]
        [Tooltip("Ölümcül olmayan hasarlarda (1. ve 2. çarpışma) geminin üzerinde oynayacak efekt")]
        [SerializeField] private GameObject damageEffectPrefab;
        [Tooltip("Efektin boyut çarpanı")]
        [SerializeField] private float damageEffectScale = 1f;

        [Header("Kamera Sarsıntısı")]
        [Tooltip("Hasar alındığında kameranın sarsılma süresi (saniye)")]
        [SerializeField] private float shakeDuration = 0.25f;
        [Tooltip("Hasar alındığında kameranın sarsılma şiddeti")]
        [SerializeField] private float shakeStrength = 0.35f;

        private Transform selfTransform;
        private float invulnerableTimer;

        private void Awake()
        {
            selfTransform = transform;
        }

        public bool IsInvulnerable => invulnerableTimer > 0f;

        private void Update()
        {
            if (invulnerableTimer <= 0f) return;

            invulnerableTimer = Mathf.Max(0f, invulnerableTimer - Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsInvulnerable) return;
            if (!GameManager.IsGameplayActive) return;
            if (!other.TryGetComponent(out MeteorController meteor)) return;

            invulnerableTimer = invulnerableDuration;

            meteor.DespawnOnShipHit();
            TakeDamage();
        }

        private void TakeDamage()
        {
            if (HealthManager.Instance == null) return;

            HealthManager.Instance.LoseLife();
            ShakeCamera();

            // Son can gittiyse gemi patlama sekansı devreye girer; hasar efekti oynatılmaz.
            if (!HealthManager.Instance.IsAlive) return;

            PlayDamageEffect();
        }

        /// <summary>
        /// Hasar efektini geminin altında oluşturur; böylece gemi hareket ederken
        /// efekt de gemiyle birlikte gider. Efekt prefab'ı kendini yok eder.
        /// </summary>
        private void PlayDamageEffect()
        {
            if (damageEffectPrefab == null) return;

            GameObject effect = Instantiate(damageEffectPrefab, selfTransform);
            effect.transform.localPosition = Vector3.zero;
            effect.transform.localScale = Vector3.one * damageEffectScale;
        }

        private void ShakeCamera()
        {
            if (CameraShake.Instance == null) return;

            CameraShake.Instance.Shake(shakeDuration, shakeStrength);
        }
    }
}
