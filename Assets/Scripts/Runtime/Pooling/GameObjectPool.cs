using UnityEngine;
using UnityEngine.Pool;

namespace PayeGames.AsalYolculuk.Pooling
{
    public class GameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int defaultCapacity = 16;
        [SerializeField] private int maxSize = 64;

        private IObjectPool<GameObject> pool;

        private void Awake()
        {
            pool = new ObjectPool<GameObject>(
                CreateInstance,
                OnGetInstance,
                OnReleaseInstance,
                OnDestroyInstance,
                collectionCheck: false,
                defaultCapacity,
                maxSize);
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        public void Despawn(GameObject instance)
        {
            pool.Release(instance);
        }

        private GameObject CreateInstance()
        {
            return Instantiate(prefab, transform);
        }

        private void OnGetInstance(GameObject instance)
        {
            instance.SetActive(true);

            if (instance.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnSpawned(this);
            }
        }

        private void OnReleaseInstance(GameObject instance)
        {
            if (instance.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnDespawned();
            }

            instance.SetActive(false);
        }

        private void OnDestroyInstance(GameObject instance)
        {
            Destroy(instance);
        }
    }
}
