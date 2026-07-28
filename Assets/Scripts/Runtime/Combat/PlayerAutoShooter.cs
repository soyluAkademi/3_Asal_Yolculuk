using UnityEngine;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Combat
{
    public class PlayerAutoShooter : MonoBehaviour
    {
        [Header("Ateş")]
        [SerializeField] private GameObjectPool bulletPool;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 4f;

        private float fireTimer;

        private void Update()
        {
            fireTimer += Time.deltaTime;

            float fireInterval = 1f / fireRate;
            if (fireTimer < fireInterval) return;

            fireTimer -= fireInterval;
            bulletPool.Spawn(firePoint.position, firePoint.rotation);
        }
    }
}
