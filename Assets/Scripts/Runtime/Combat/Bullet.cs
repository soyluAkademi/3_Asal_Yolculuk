using UnityEngine;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.Combat
{
    public class Bullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;

        private GameObjectPool ownerPool;
        private Transform selfTransform;
        private float lifetimeTimer;

        private void Awake()
        {
            selfTransform = transform;
        }

        public void OnSpawned(GameObjectPool owner)
        {
            ownerPool = owner;
            lifetimeTimer = 0f;
        }

        public void OnDespawned()
        {
        }

        private void Update()
        {
            selfTransform.position += selfTransform.right * speed * Time.deltaTime;

            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= lifetime)
            {
                ownerPool.Despawn(gameObject);
            }
        }
    }
}
