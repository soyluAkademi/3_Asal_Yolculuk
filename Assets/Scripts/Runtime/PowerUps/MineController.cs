using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Pooling;

namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Sahnede sağdan sola akan toplanabilir maden. Görselini ve etkisini
    /// <see cref="MineDataSO"/>'dan alır. Toplamayı gemi üzerindeki
    /// <see cref="Player.MineCollector"/> tetikler — meteorlarla aynı desen:
    /// çarpışma sorumluluğu Rigidbody2D'yi taşıyan gemi tarafındadır, maden
    /// kimin topladığını bilmez.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MineController : MonoBehaviour, IPoolable
    {
        // Sahnede o an akan madenler. Madenler havuzdan geldiği için Inspector'dan
        // referans tutulamaz; bölüm sonunda toplu temizlik bu liste üzerinden yapılır.
        private static readonly List<MineController> ActiveMineList = new List<MineController>();

        [Header("Görsel")]
        [SerializeField] private SpriteRenderer iconRenderer;

        [Header("Hareket")]
        [Tooltip("Meteorlardan yavaş olmalı; oyuncu yetişip toplayabilsin")]
        [SerializeField] private float moveSpeed = 1.5f;
        [Tooltip("Dikey salınım genliği (0 = düz gider)")]
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobFrequency = 1.5f;
        [Tooltip("Kendi ekseninde dönme hızı (derece/sn)")]
        [SerializeField] private float rotationSpeed = 60f;

        [Header("Ekran Dışı Sınır")]
        [Tooltip("Ekranın sol kenarından ne kadar uzakta havuza iade edileceği")]
        [SerializeField] private float despawnMargin = 1f;

        [Header("Toplama Efekti")]
        [SerializeField] private float pickupEffectScale = 1f;

        private GameObjectPool ownerPool;
        private Transform selfTransform;
        private Transform iconTransform;
        private Collider2D mineCollider;
        private Camera gameCamera;
        private MineDataSO data;
        private float despawnLeftOfX;
        private float bobTimer;
        private float baseY;
        private bool hasBaseY;
        private bool isCollected;

        public MineDataSO Data => data;

        private void Awake()
        {
            selfTransform = transform;
            gameCamera = Camera.main;
            TryGetComponent(out mineCollider);

            if (iconRenderer != null)
            {
                iconTransform = iconRenderer.transform;
            }
        }

        public void OnSpawned(GameObjectPool owner)
        {
            ownerPool = owner;
            ActiveMineList.Add(this);
            isCollected = false;
            bobTimer = 0f;
            despawnLeftOfX = CalculateLeftEdgeX() - despawnMargin;

            // Havuz konumu OnSpawned'dan SONRA atadığı için salınımın merkezi burada
            // okunamaz; ilk Update'te alınır.
            hasBaseY = false;

            SetCollectable(true);
        }

        public void OnDespawned()
        {
            ActiveMineList.Remove(this);
            data = null;
        }

        /// <summary>
        /// Sahnedeki tüm madenleri havuza iade eder. Bölüm bittiğinde veya oyuncu
        /// öldüğünde çağrılır; aksi halde madenler ekranda donmuş olarak kalırdı.
        /// </summary>
        public static void DespawnAll()
        {
            for (int index = ActiveMineList.Count - 1; index >= 0; index--)
            {
                MineController mine = ActiveMineList[index];
                if (mine == null || mine.ownerPool == null) continue;

                mine.ownerPool.Despawn(mine.gameObject);
            }

            ActiveMineList.Clear();
        }

        /// <summary>Üretim anında spawner tarafından çağrılır; görsel de buradan gelir.</summary>
        public void SetData(MineDataSO mineData)
        {
            data = mineData;

            if (iconRenderer == null) return;
            if (mineData == null || mineData.Icon == null) return;

            iconRenderer.sprite = mineData.Icon;
        }

        private void Update()
        {
            if (!GameManager.IsGameplayActive) return;

            Vector3 position = selfTransform.position;

            if (!hasBaseY)
            {
                baseY = position.y;
                hasBaseY = true;
            }

            bobTimer += Time.deltaTime;
            position.x -= moveSpeed * Time.deltaTime;
            position.y = baseY + Mathf.Sin(bobTimer * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            selfTransform.position = position;

            if (iconTransform != null)
            {
                iconTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }

            if (position.x > despawnLeftOfX) return;

            ownerPool.Despawn(gameObject);
        }

        /// <summary>
        /// Gemi maden üzerinden geçtiğinde çağrılır. Etkiyi
        /// <see cref="PowerUpManager"/>'a devreder ve madeni havuza iade eder.
        /// </summary>
        public void Collect()
        {
            if (isCollected) return;

            isCollected = true;
            SetCollectable(false);

            PlayPickupEffect();
            PlayPickupSound();

            if (PowerUpManager.Instance != null && data != null)
            {
                PowerUpManager.Instance.Collect(data);
            }

            ownerPool.Despawn(gameObject);
        }

        private void SetCollectable(bool isCollectable)
        {
            if (mineCollider == null) return;

            mineCollider.enabled = isCollectable;
        }

        /// <summary>Toplama efekti prefab'ı kendini yok eder; ömrü burada takip edilmez.</summary>
        private void PlayPickupEffect()
        {
            if (data == null || data.PickupEffectPrefab == null) return;

            GameObject effect = Instantiate(data.PickupEffectPrefab, selfTransform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * pickupEffectScale;
        }

        private void PlayPickupSound()
        {
            if (data == null || data.PickupClip == null) return;
            if (SoundManager.Instance == null) return;

            SoundManager.Instance.PlaySfx(data.PickupClip);
        }

        private float CalculateLeftEdgeX()
        {
            if (gameCamera == null) return 0f;

            Vector3 edge = gameCamera.ViewportToWorldPoint(new Vector3(0f, 0f, gameCamera.nearClipPlane));
            return edge.x;
        }
    }
}
