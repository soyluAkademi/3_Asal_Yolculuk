using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Combat
{
    public class PlayerAutoShooter : MonoBehaviour
    {
        [Header("Ateş")]
        [SerializeField] private GameObjectPool bulletPool;
        [Tooltip("Element 0 normal atış noktasıdır; sonrakiler yalnızca çoklu atış " +
            "etkisi (Plazma Yakıtı) aktifken kullanılır")]
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private float fireRate = 4f;

        [Header("Ses")]
        [Tooltip("Tek noktadan ateş edilirken çalınacak klip")]
        [SerializeField] private AudioClip fireClip;
        [Tooltip("Çoklu atış (Plazma Yakıtı) aktifken çalınacak klip; boşsa tekli atış klibi kullanılır")]
        [SerializeField] private AudioClip multiFireClip;
        [Tooltip("Atış sesi saniyede birkaç kez tekrarlandığı için varsayılan olarak kısık çalınır")]
        [Range(0f, 1f)]
        [SerializeField] private float fireVolume = 0.5f;

        private float fireTimer;

        // Normalde yalnızca merkez nokta ateş eder; Plazma Yakıtı bunu artırır.
        private int activeFirePointCount = 1;

        /// <summary>
        /// Aynı anda ateş edecek nokta sayısını ayarlar (Plazma Yakıtı: 3).
        /// Dizide o kadar nokta yoksa var olan sayıyla sınırlanır.
        /// </summary>
        public void SetActiveFirePointCount(int count)
        {
            if (firePoints == null || firePoints.Length == 0) return;

            activeFirePointCount = Mathf.Clamp(count, 1, firePoints.Length);
        }

        public void ResetFirePointCount()
        {
            activeFirePointCount = 1;
        }

        private void Update()
        {
            if (!GameManager.IsGameplayActive) return;

            fireTimer += Time.deltaTime;

            float fireInterval = 1f / fireRate;
            if (fireTimer < fireInterval) return;

            fireTimer -= fireInterval;
            Fire();
        }

        /// <summary>
        /// Aktif noktaların her birinden bir mermi çıkarır. Merminin yönü noktanın
        /// kendi rotasyonundan gelir; yelpaze açısı Inspector'da nokta döndürülerek
        /// ayarlanır, kodda açı hesabı yapılmaz.
        /// </summary>
        private void Fire()
        {
            if (bulletPool == null) return;
            if (firePoints == null) return;

            int firedCount = 0;

            for (int index = 0; index < activeFirePointCount && index < firePoints.Length; index++)
            {
                Transform firePoint = firePoints[index];
                if (firePoint == null) continue;

                bulletPool.Spawn(firePoint.position, firePoint.rotation);
                firedCount++;
            }

            // Çoklu atışta her nokta için ayrı ses çalmak sesi üst üste bindirir;
            // salvo başına tek çağrı yapılır.
            if (firedCount > 0) PlayFireSfx(firedCount);
        }

        private void PlayFireSfx(int firedCount)
        {
            if (SoundManager.Instance == null) return;

            AudioClip clip = firedCount > 1 && multiFireClip != null
                ? multiFireClip
                : fireClip;

            if (clip == null) return;

            SoundManager.Instance.PlaySfx(clip, fireVolume);
        }
    }
}
