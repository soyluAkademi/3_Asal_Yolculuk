using System;
using System.Collections;
using UnityEngine;
using PayeGames.AsalYolculuk.Combat;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Meteors;

namespace PayeGames.AsalYolculuk.Player
{
    /// <summary>
    /// Ölüm sekansını yönetir: gemi gizlenir, üzerinde ve yakınında arka arkaya patlamalar
    /// oynar, ardından sahnedeki meteorlar kendi patlama efektleriyle sırayla temizlenir.
    /// </summary>
    public class PlayerDeathEffect : MonoBehaviour
    {
        [Header("Patlama Efekti")]
        [Tooltip("Ölüm anında oluşturulacak partikül efekti")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [Tooltip("Arka arkaya kaç patlama oynatılacağı")]
        [SerializeField, Min(1)] private int explosionCount = 6;
        [Tooltip("Patlamalar arasındaki gecikme (saniye)")]
        [SerializeField] private float explosionInterval = 0.12f;
        [Tooltip("Patlamaların geminin merkezinden ne kadar uzağa saçılacağı (ilk patlama tam merkezde)")]
        [SerializeField] private float explosionRadius = 0.6f;
        [Tooltip("Efektin boyut çarpanı")]
        [SerializeField] private float explosionScale = 1f;

        [Header("Meteor Temizliği")]
        [Tooltip("Sahnedeki meteorları sırayla patlatan bileşen")]
        [SerializeField] private MeteorFieldClearer meteorFieldClearer;
        [Tooltip("Gemi patlaması bittikten sonra meteorların patlamaya başlaması için beklenen süre")]
        [SerializeField] private float meteorClearDelay = 0.2f;

        /// <summary>
        /// Ölüm sekansının tamamı (gemi patlaması + meteor temizliği) bittiğinde tetiklenir.
        /// Sekansın süresi sahnedeki meteor sayısına göre değiştiği için oyun sonu paneli
        /// sabit bir gecikme yerine bu event'i bekler.
        /// </summary>
        public event Action OnDeathSequenceCompleted;

        [Header("Diriliş")]
        [Tooltip("Gemi geri geldiğinde kaç saniye hasar almayacağı")]
        [SerializeField] private float respawnInvulnerableDuration = 1.5f;

        private Transform selfTransform;
        private Renderer[] shipRenderers;
        private Collider2D[] shipColliders;

        // Tasarım gereği kapalı bırakılmış bileşenler (ileride kalkan bariyeri gibi)
        // diriltmede yanlışlıkla açılmasın diye başlangıç durumları saklanır.
        private bool[] rendererInitialStates;
        private bool[] colliderInitialStates;

        private PlayerShipController shipController;
        private PlayerAutoShooter autoShooter;
        private ShipCollisionHandler collisionHandler;

        private void Awake()
        {
            selfTransform = transform;

            // Gemi patlama anında gizlenir; obje aktif kalmalı ki patlama coroutine'i devam etsin.
            shipRenderers = GetComponentsInChildren<Renderer>(true);
            shipColliders = GetComponentsInChildren<Collider2D>(true);

            CacheInitialStates();

            // Bileşenler gemide veya bir alt objesinde olabilir.
            shipController = GetComponentInChildren<PlayerShipController>(true);
            autoShooter = GetComponentInChildren<PlayerAutoShooter>(true);
            collisionHandler = GetComponentInChildren<ShipCollisionHandler>(true);
        }

        private void CacheInitialStates()
        {
            rendererInitialStates = new bool[shipRenderers.Length];
            for (int index = 0; index < shipRenderers.Length; index++)
            {
                rendererInitialStates[index] = shipRenderers[index].enabled;
            }

            colliderInitialStates = new bool[shipColliders.Length];
            for (int index = 0; index < shipColliders.Length; index++)
            {
                colliderInitialStates[index] = shipColliders[index].enabled;
            }
        }

        private void Start()
        {
            // HealthManager Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (HealthManager.Instance == null) return;

            HealthManager.Instance.OnAllLivesLost += HandleAllLivesLost;
        }

        private void OnDestroy()
        {
            if (HealthManager.Instance == null) return;

            HealthManager.Instance.OnAllLivesLost -= HandleAllLivesLost;
        }

        private void HandleAllLivesLost()
        {
            HideShip();
            StartCoroutine(PlayDeathSequence());
        }

        private IEnumerator PlayDeathSequence()
        {
            yield return PlayExplosionSequence();

            yield return new WaitForSeconds(meteorClearDelay);

            // Temizleyici bağlı değilse sekans yine tamamlanmış sayılır; aksi halde
            // oyun sonu paneli hiç açılmaz ve oyun boş ekranda kilitlenir.
            if (meteorFieldClearer != null)
            {
                yield return meteorFieldClearer.ClearRoutine();
            }

            OnDeathSequenceCompleted?.Invoke();
        }

        /// <summary>
        /// Gemiyi görünmez ve etkisiz hâle getirir. Patlama efektleri bu obje üzerinde
        /// çalıştığı için gameObject devre dışı bırakılmaz, sadece bileşenleri kapatılır.
        /// </summary>
        private void HideShip()
        {
            for (int index = 0; index < shipRenderers.Length; index++)
            {
                shipRenderers[index].enabled = false;
            }

            for (int index = 0; index < shipColliders.Length; index++)
            {
                shipColliders[index].enabled = false;
            }

            if (shipController != null)
            {
                shipController.enabled = false;
            }

            if (autoShooter != null)
            {
                autoShooter.enabled = false;
            }
        }

        /// <summary>
        /// <see cref="HideShip"/>'in tersi: bileşenler <c>Awake</c>'te kaydedilen
        /// başlangıç durumlarına döner ve gemi kısa bir süre dokunulmaz olur.
        /// Bölüm yeniden başlatılırken çağrılır.
        /// </summary>
        public void RestoreShip()
        {
            for (int index = 0; index < shipRenderers.Length; index++)
            {
                shipRenderers[index].enabled = rendererInitialStates[index];
            }

            for (int index = 0; index < shipColliders.Length; index++)
            {
                shipColliders[index].enabled = colliderInitialStates[index];
            }

            if (shipController != null)
            {
                shipController.enabled = true;
            }

            if (autoShooter != null)
            {
                autoShooter.enabled = true;
            }

            if (collisionHandler == null) return;

            collisionHandler.GrantInvulnerability(respawnInvulnerableDuration);
        }

        private IEnumerator PlayExplosionSequence()
        {
            if (explosionEffectPrefab == null) yield break;

            for (int index = 0; index < explosionCount; index++)
            {
                // İlk patlama tam geminin üzerinde, sonrakiler yakın noktalara saçılır.
                Vector3 offset = index == 0 ? Vector3.zero : (Vector3)(UnityEngine.Random.insideUnitCircle * explosionRadius);
                SpawnExplosion(offset);

                yield return new WaitForSeconds(explosionInterval);
            }
        }

        /// <summary>Efekt prefab'ı kendini yok ettiği için burada ömrü takip edilmez.</summary>
        private void SpawnExplosion(Vector3 offset)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, selfTransform.position + offset, Quaternion.identity);
            effect.transform.localScale = Vector3.one * explosionScale;
        }
    }
}
