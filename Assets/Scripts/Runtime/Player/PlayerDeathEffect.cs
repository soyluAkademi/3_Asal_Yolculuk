using System.Collections;
using System.Collections.Generic;
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
        [Tooltip("Gemi patlaması bittikten sonra meteorların patlamaya başlaması için beklenen süre")]
        [SerializeField] private float meteorClearDelay = 0.2f;
        [Tooltip("Meteorların arka arkaya patlama aralığı (saniye)")]
        [SerializeField] private float meteorClearInterval = 0.15f;

        private Transform selfTransform;
        private Renderer[] shipRenderers;
        private Collider2D[] shipColliders;
        private readonly List<MeteorController> meteorClearBuffer = new List<MeteorController>();

        private void Awake()
        {
            selfTransform = transform;

            // Gemi patlama anında gizlenir; obje aktif kalmalı ki patlama coroutine'i devam etsin.
            shipRenderers = GetComponentsInChildren<Renderer>(true);
            shipColliders = GetComponentsInChildren<Collider2D>(true);
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

            yield return ClearMeteorsSequentially();
        }

        /// <summary>
        /// Sahnedeki meteorları soldan sağa doğru, kendi patlama efektleriyle tek tek yok eder.
        /// Liste kopyalanır; çünkü meteorlar patladıkça aktif meteor listesinden çıkar.
        /// </summary>
        private IEnumerator ClearMeteorsSequentially()
        {
            meteorClearBuffer.Clear();
            meteorClearBuffer.AddRange(MeteorController.ActiveMeteors);
            meteorClearBuffer.Sort(CompareByHorizontalPosition);

            for (int index = 0; index < meteorClearBuffer.Count; index++)
            {
                MeteorController meteor = meteorClearBuffer[index];
                if (meteor == null) continue;
                if (!meteor.gameObject.activeInHierarchy) continue;

                meteor.DespawnWithExplosion();

                yield return new WaitForSeconds(meteorClearInterval);
            }

            meteorClearBuffer.Clear();
        }

        private static int CompareByHorizontalPosition(MeteorController first, MeteorController second)
        {
            if (first == null || second == null) return 0;

            return first.transform.position.x.CompareTo(second.transform.position.x);
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

            // Ateşleyici gemide veya bir alt objesinde olabilir.
            PlayerShipController shipController = GetComponentInChildren<PlayerShipController>(true);
            if (shipController != null)
            {
                shipController.enabled = false;
            }

            PlayerAutoShooter autoShooter = GetComponentInChildren<PlayerAutoShooter>(true);
            if (autoShooter != null)
            {
                autoShooter.enabled = false;
            }
        }

        private IEnumerator PlayExplosionSequence()
        {
            if (explosionEffectPrefab == null) yield break;

            for (int index = 0; index < explosionCount; index++)
            {
                // İlk patlama tam geminin üzerinde, sonrakiler yakın noktalara saçılır.
                Vector3 offset = index == 0 ? Vector3.zero : (Vector3)(Random.insideUnitCircle * explosionRadius);
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
