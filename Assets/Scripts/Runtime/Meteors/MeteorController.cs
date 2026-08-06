using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using PayeGames.AsalYolculuk.Combat;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Pooling;
using Random = UnityEngine.Random;

namespace PayeGames.AsalYolculuk.Meteors
{
    [RequireComponent(typeof(SortingGroup))]
    public class MeteorController : MonoBehaviour, IPoolable
    {
        // Her meteora benzersiz bir sorting order verilir; böylece üst üste binen
        // meteorlarda alttaki meteorun sayısı üstteki meteorun sprite'ını delmez.
        private const int SortingOrderRange = 500;
        private static int sortingOrderCounter;

        /// <summary>
        /// Bir ana meteor tüm parçalarıyla birlikte yok edildiğinde tetiklenir.
        /// Parametre: ana meteorun sayısı. Havuzdan gelen meteorlara Inspector'dan
        /// dinleyici bağlanamayacağı için static tutulur; `MeteorSpawner` dinler.
        /// </summary>
        public static event Action<int> OnMeteorCompleted;

        // Sahnede o an aktif olan meteorlar. Oyuncu öldüğünde hepsi sırayla patlatılır.
        private static readonly List<MeteorController> ActiveMeteorList = new List<MeteorController>();

        public static IReadOnlyList<MeteorController> ActiveMeteors => ActiveMeteorList;

        [Header("Hareket")]
        [SerializeField] private float minMoveSpeed = 2f;
        [SerializeField] private float maxMoveSpeed = 4f;
        [SerializeField] private float minRotationSpeed = 30f;
        [SerializeField] private float maxRotationSpeed = 90f;

        [Header("Ekran Dışı Sınır")]
        [Tooltip("Ekranın sol kenarından ne kadar uzakta despawn edileceği (ekran dışı tampon mesafesi)")]
        [SerializeField] private float despawnMargin = 1f;

        [Header("Sayı Görseli")]
        [SerializeField] private TMP_Text meteorTxt;

        [Header("Meteor Görseli")]
        [SerializeField] private SpriteRenderer meteorSpriteRenderer;

        [Header("Patlama Efekti")]
        [Tooltip("Efektin meteor boyutuna göre ölçek çarpanı")]
        [SerializeField] private float explosionEffectScale = 1f;

        [Header("Asal Çarpan Parçalanması")]
        [Tooltip("Parçaların ana meteora göre boyut oranı")]
        [SerializeField, Range(0.2f, 1f)] private float fragmentScaleRatio = 0.55f;
        [Tooltip("Parçaların ana meteorun merkezinden dikeyde ne kadar uzağa dizileceği")]
        [SerializeField] private float fragmentSpawnSpacing = 0.45f;
        [Tooltip("Parçaların dikey saçılma şiddeti (0 = düz sola gider)")]
        [SerializeField, Range(0f, 2f)] private float fragmentSpreadStrength = 0.7f;
        [Tooltip("Parçaların ana meteora göre hız çarpanı")]
        [SerializeField] private float fragmentSpeedMultiplier = 1.2f;
        [Tooltip("Parça oluştuktan sonra kaç saniye boyunca vurulamayacağı (collider bu süre boyunca kapalı kalır)")]
        [SerializeField] private float fragmentInvulnerableDuration = 1f;
        [Tooltip("Parçaların sekeceği üst/alt sınırın ekran kenarından içeride kalma payı")]
        [SerializeField] private float bounceEdgePadding = 0.3f;

        private GameObjectPool ownerPool;
        private SortingGroup sortingGroup;
        private Transform selfTransform;
        private Collider2D meteorCollider;
        private Camera gameCamera;
        private float invulnerableTimer;
        private Vector3 baseScale;
        private Vector3 moveDirection;
        private float despawnLeftOfX;
        private float despawnBelowY;
        private float despawnAboveY;
        private float bounceBottomY;
        private float bounceTopY;
        private float screenRightEdgeX;
        private bool hasEnteredScreen;
        private float moveSpeed;
        private float rotationSpeed;
        private int number;
        private bool isPrimeNumber;
        private bool isFragment;
        private MeteorSession session;
        private Sprite currentSprite;
        private GameObject explosionEffectPrefab;

        public int Number => number;
        public bool IsPrimeNumber => isPrimeNumber;
        public bool IsFragment => isFragment;

        private void Awake()
        {
            selfTransform = transform;
            gameCamera = Camera.main;
            sortingGroup = GetComponent<SortingGroup>();
            baseScale = selfTransform.localScale;
            TryGetComponent(out meteorCollider);
        }

        public void OnSpawned(GameObjectPool owner)
        {
            ownerPool = owner;
            ActiveMeteorList.Add(this);
            AssignUniqueSortingOrder();
            despawnLeftOfX = CalculateEdgeX(0f) - despawnMargin;
            screenRightEdgeX = CalculateEdgeX(1f);
            float bottomEdgeY = CalculateEdgeY(0f);
            float topEdgeY = CalculateEdgeY(1f);
            despawnBelowY = bottomEdgeY - despawnMargin;
            despawnAboveY = topEdgeY + despawnMargin;
            bounceBottomY = bottomEdgeY + bounceEdgePadding;
            bounceTopY = topEdgeY - bounceEdgePadding;
            hasEnteredScreen = false;

            // Havuzdan gelen nesne bir önceki hayatında parça olmuş olabilir; varsayılana döndür.
            selfTransform.localScale = baseScale;
            isFragment = false;
            session = null;
            moveDirection = Vector3.left;
            invulnerableTimer = 0f;
            SetVulnerable(true);

            moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

            float speed = Random.Range(minRotationSpeed, maxRotationSpeed);
            rotationSpeed = Random.value < 0.5f ? speed : -speed;
        }

        private void AssignUniqueSortingOrder()
        {
            sortingOrderCounter = (sortingOrderCounter + 1) % SortingOrderRange;
            sortingGroup.sortingOrder = sortingOrderCounter;
        }

        public void SetNumber(int assignedNumber)
        {
            number = assignedNumber;
            isPrimeNumber = PrimeMathEngine.IsPrime(number);

            if (meteorTxt != null)
            {
                meteorTxt.text = number.ToString();
            }
        }

        /// <summary>
        /// Meteorun görselini ve o görsele ait patlama efektini birlikte atar.
        /// Parçalar ana meteorun ikisini de miras alır.
        /// </summary>
        public void SetVisuals(Sprite sprite, GameObject explosionEffect)
        {
            explosionEffectPrefab = explosionEffect;

            if (meteorSpriteRenderer == null) return;
            if (sprite == null) return;

            currentSprite = sprite;
            meteorSpriteRenderer.sprite = sprite;
        }

        private float CalculateEdgeX(float viewportX)
        {
            if (gameCamera == null) return 0f;

            Vector3 edge = gameCamera.ViewportToWorldPoint(new Vector3(viewportX, 0f, gameCamera.nearClipPlane));
            return edge.x;
        }

        private float CalculateEdgeY(float viewportY)
        {
            if (gameCamera == null) return 0f;

            Vector3 edge = gameCamera.ViewportToWorldPoint(new Vector3(0f, viewportY, gameCamera.nearClipPlane));
            return edge.y;
        }

        public void OnDespawned()
        {
            ActiveMeteorList.Remove(this);
        }

        private void Update()
        {
            TickInvulnerability();

            selfTransform.position += moveDirection * (moveSpeed * Time.deltaTime);
            selfTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            BounceOnVerticalEdges();

            if (!hasEnteredScreen && selfTransform.position.x <= screenRightEdgeX)
            {
                hasEnteredScreen = true;
            }

            if (IsOutOfBounds())
            {
                // Parçası ekrandan kaçan meteor tamamlanmamış sayılır; puan verilmez.
                if (session != null)
                {
                    session.Cancel();
                }

                ownerPool.Despawn(gameObject);
            }
        }

        /// <summary>
        /// Üst/alt sınıra çarpan meteoru aynı açıyla geri yansıtır (ayna görüntüsü).
        /// Yatay yön korunduğu için sağ/sol hareket aynen devam eder.
        /// </summary>
        private void BounceOnVerticalEdges()
        {
            if (moveDirection.y == 0f) return;

            Vector3 position = selfTransform.position;

            if (position.y >= bounceTopY && moveDirection.y > 0f)
            {
                position.y = bounceTopY;
            }
            else if (position.y <= bounceBottomY && moveDirection.y < 0f)
            {
                position.y = bounceBottomY;
            }
            else
            {
                return;
            }

            moveDirection.y = -moveDirection.y;
            selfTransform.position = position;
        }

        private void TickInvulnerability()
        {
            if (invulnerableTimer <= 0f) return;

            invulnerableTimer -= Time.deltaTime;
            if (invulnerableTimer > 0f) return;

            invulnerableTimer = 0f;
            SetVulnerable(true);
        }

        private void SetVulnerable(bool isVulnerable)
        {
            if (meteorCollider == null) return;

            meteorCollider.enabled = isVulnerable;
        }

        private bool IsOutOfBounds()
        {
            Vector3 position = selfTransform.position;
            if (position.x <= despawnLeftOfX) return true;

            return position.y <= despawnBelowY || position.y >= despawnAboveY;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!hasEnteredScreen) return;
            if (!other.TryGetComponent(out Bullet bullet)) return;

            // Aynı karede ikinci bir mermi tetiklenirse meteor iki kez puan verip
            // iki kez havuza dönebilir; collider hemen kapatılarak bu engellenir.
            SetVulnerable(false);

            bullet.Despawn();
            PlayExplosionEffect();
            HandleBulletHit();
            ownerPool.Despawn(gameObject);
        }

        /// <summary>
        /// Meteor gemiye çarptığında çağrılır (çarpışmayı gemi yakalar). Puan verilmez;
        /// oturum iptal edilir ki kalan parçalar da meteoru tamamlanmış saymasın.
        /// </summary>
        public void DespawnOnShipHit()
        {
            DespawnWithExplosion();
        }

        /// <summary>
        /// Meteoru patlatıp havuza iade eder: puan verilmez, oturum iptal edilir ki
        /// kalan parçalar da meteoru tamamlanmış saymasın. Gemi çarpışmasında ve
        /// oyuncu öldüğünde sahne temizlenirken kullanılır.
        /// </summary>
        public void DespawnWithExplosion()
        {
            // Aynı karede ikinci bir çarpışma tetiklenip meteor iki kez havuza dönmesin.
            SetVulnerable(false);

            PlayExplosionEffect();

            if (session != null)
            {
                session.Cancel();
            }

            ownerPool.Despawn(gameObject);
        }

        /// <summary>
        /// Meteorun bulunduğu pozisyonda patlama efektini oluşturur. Efekt prefab'ı
        /// kendini yok ettiği için burada ömrü takip edilmez.
        /// </summary>
        private void PlayExplosionEffect()
        {
            if (explosionEffectPrefab == null) return;

            GameObject effect = Instantiate(explosionEffectPrefab, selfTransform.position, Quaternion.identity);

            // Parçalar küçüldüğü için efekt de meteorun güncel boyutuyla orantılı ölçeklenir.
            float sizeRatio = baseScale.x != 0f ? selfTransform.localScale.x / baseScale.x : 1f;
            effect.transform.localScale = Vector3.one * (explosionEffectScale * sizeRatio);
        }

        private void HandleBulletHit()
        {
            // Bileşik sayı asal çarpanlarına ayrılır; oturum parçalarla devam eder.
            if (!isPrimeNumber && TrySplitIntoPrimeFragments()) return;

            // Asal sayı bölünmez: bu dal tamamen yok edildi.
            CompleteBranch();
        }

        /// <summary>Bu dal tamamen yok edildi; oturumun son dalıysa puan verilir.</summary>
        private void CompleteBranch()
        {
            // Oturumu olmayan meteor tek vuruşta yok olan asal ana meteordur.
            if (session == null)
            {
                CompleteMeteor(number);
                return;
            }

            if (session.RegisterBranchDestroyed())
            {
                CompleteMeteor(session.RootNumber);
            }
        }

        /// <summary>Ana meteor tamamen yok edildi: puan verilir ve sayısı duyurulur.</summary>
        private void CompleteMeteor(int rootNumber)
        {
            OnMeteorCompleted?.Invoke(rootNumber);

            if (ScoreManager.Instance == null) return;

            ScoreManager.Instance.AddMeteorScore();
            ScoreManager.Instance.RegisterDiscoveredPrimes(rootNumber);
        }

        /// <returns>Meteor parçalara ayrıldıysa true; ayrılamadıysa false.</returns>
        private bool TrySplitIntoPrimeFragments()
        {
            List<int> primeFactors = PrimeMathEngine.GetPrimeFactors(number);
            if (primeFactors.Count <= 1) return false;

            // Ana meteor ilk kez bölünüyorsa oturum burada başlar; parçalar aynı oturumu paylaşır.
            MeteorSession activeSession = session ?? new MeteorSession(number);
            activeSession.RegisterSplit(primeFactors.Count);

            Vector3 origin = selfTransform.position;
            Vector3 fragmentScale = selfTransform.localScale * fragmentScaleRatio;

            for (int index = 0; index < primeFactors.Count; index++)
            {
                float spread = CalculateSpread(index, primeFactors.Count);
                Vector3 spawnPosition = origin + Vector3.up * (spread * fragmentSpawnSpacing);

                GameObject instance = ownerPool.Spawn(spawnPosition, Quaternion.identity);
                if (!instance.TryGetComponent(out MeteorController fragment))
                {
                    // Parça oluşturulamadı; açılan dal kapatılmalı, aksi halde oturum asla bitmez.
                    activeSession.Cancel();
                    continue;
                }

                fragment.SetNumber(primeFactors[index]);
                fragment.SetVisuals(currentSprite, explosionEffectPrefab);
                fragment.ApplyFragmentSetup(fragmentScale, spread, activeSession);
            }

            return true;
        }

        /// <summary>Parçaları -1 ile +1 arasında simetrik olarak dağıtır.</summary>
        private static float CalculateSpread(int index, int fragmentCount)
        {
            if (fragmentCount <= 1) return 0f;

            return Mathf.Lerp(-1f, 1f, (float)index / (fragmentCount - 1));
        }

        private void ApplyFragmentSetup(Vector3 scale, float verticalSpread, MeteorSession parentSession)
        {
            isFragment = true;
            session = parentSession;
            selfTransform.localScale = scale;
            moveDirection = new Vector3(-1f, verticalSpread * fragmentSpreadStrength, 0f).normalized;
            moveSpeed *= fragmentSpeedMultiplier;

            // Parça ekranın içinde doğar; ekrana giriş kontrolüne takılmamalı.
            hasEnteredScreen = true;

            // Parçalar dağılana kadar kısa süre vurulamaz; aksi halde ana meteoru vuran
            // mermi aynı karede parçaları da yok eder.
            if (fragmentInvulnerableDuration <= 0f) return;

            invulnerableTimer = fragmentInvulnerableDuration;
            SetVulnerable(false);
        }
    }
}
