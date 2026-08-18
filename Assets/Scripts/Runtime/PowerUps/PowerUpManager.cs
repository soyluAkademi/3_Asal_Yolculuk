using System;
using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.Combat;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Meteors;
using PayeGames.AsalYolculuk.Player;
using PayeGames.AsalYolculuk.PowerUps.Effects;

namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Toplanan madenlerin etkilerini oluşturur, süreleri boyunca besler ve süre
    /// dolunca geri alır. GDD kuralı gereği aynı anda birden fazla etki aktif olabilir;
    /// aynı türden ikinci maden toplanırsa etki üst üste binmez, süresi tazelenir.
    /// </summary>
    public class PowerUpManager : MonoBehaviour
    {
        public static PowerUpManager Instance { get; private set; }

        [Header("Etki Hedefleri")]
        [Tooltip("Plazma Yakıtı atış noktalarını bunun üzerinden açar")]
        [SerializeField] private PlayerAutoShooter playerShooter;
        [Tooltip("Koruma Kalkanı bariyeri bunun üzerinden açılır")]
        [SerializeField] private ShipCollisionHandler shipCollisionHandler;
        [Tooltip("Meteor Çağırıcı üretim sıklığını bunun üzerinden artırır")]
        [SerializeField] private MeteorSpawner meteorSpawner;
        [Tooltip("Yerçekimi Taşı gemi hızını ve aşağı çekimi bunun üzerinden değiştirir")]
        [SerializeField] private PlayerShipController playerShipController;

        /// <summary>Süreli bir etki başladığında tetiklenir; HUD göstergesi dinler.</summary>
        public event Action<MineDataSO> OnEffectStarted;

        /// <summary>Süreli bir etki bittiğinde veya iptal edildiğinde tetiklenir.</summary>
        public event Action<MineDataSO> OnEffectEnded;

        /// <summary>Anlık etkiler için (toplama animasyonu/ses geri bildirimi).</summary>
        public event Action<MineDataSO> OnInstantEffectApplied;

        private readonly List<ActiveEffectEntry> activeEffects = new List<ActiveEffectEntry>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        // Manager'ların Awake'i garanti çalışmış olsun diye abonelik Start'ta yapılır.
        private void Start()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Bölüm bittiğinde veya kaybedildiğinde tüm etkiler temizlenir; aksi halde
        /// kalkan/yavaşlatma gibi etkiler yeni bölüme sarkardı.
        /// </summary>
        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.Gameplay) return;
            if (state == GameState.Paused) return;

            ClearAllEffects();
        }

        /// <summary>Maden toplandığında <see cref="MineController"/> tarafından çağrılır.</summary>
        public void Collect(MineDataSO data)
        {
            if (data == null) return;

            IActiveEffect effect = CreateEffect(data);
            if (effect == null)
            {
                Debug.LogWarning($"[PowerUpManager] {data.EffectType} için etki sınıfı henüz yok.");
                return;
            }

            if (effect.Duration <= 0f)
            {
                effect.ApplyEffect();
                OnInstantEffectApplied?.Invoke(data);
                return;
            }

            StartTimedEffect(data, effect);
        }

        /// <summary>
        /// Süreli etkiyi başlatır. Aynı tür zaten aktifse etki yeniden uygulanmaz,
        /// yalnızca kalan süre tazelenir — iki kalkan toplamak çift bariyer yaratmasın.
        /// </summary>
        private void StartTimedEffect(MineDataSO data, IActiveEffect effect)
        {
            int existingIndex = IndexOf(data.EffectType);
            if (existingIndex >= 0)
            {
                activeEffects[existingIndex].RemainingTime = effect.Duration;
                return;
            }

            effect.ApplyEffect();
            activeEffects.Add(new ActiveEffectEntry(data, effect));
            OnEffectStarted?.Invoke(data);
        }

        private void Update()
        {
            if (activeEffects.Count == 0) return;
            if (!GameManager.IsGameplayActive) return;

            float deltaTime = Time.deltaTime;

            // Süresi biten etki listeden çıkarıldığı için sondan başa gezilir.
            for (int index = activeEffects.Count - 1; index >= 0; index--)
            {
                ActiveEffectEntry entry = activeEffects[index];
                entry.Effect.TickEffect(deltaTime);
                entry.RemainingTime -= deltaTime;

                if (entry.RemainingTime > 0f) continue;

                activeEffects.RemoveAt(index);
                entry.Effect.RemoveEffect();
                OnEffectEnded?.Invoke(entry.Data);
            }
        }

        public bool IsEffectActive(EffectType type)
        {
            return IndexOf(type) >= 0;
        }

        /// <summary>Etki aktif değilse 0 döner. HUD geri sayımı bunu okur.</summary>
        public float GetRemainingTime(EffectType type)
        {
            int index = IndexOf(type);
            return index >= 0 ? activeEffects[index].RemainingTime : 0f;
        }

        private int IndexOf(EffectType type)
        {
            for (int index = 0; index < activeEffects.Count; index++)
            {
                if (activeEffects[index].Effect.Type == type) return index;
            }

            return -1;
        }

        /// <summary>Tüm süreli etkileri geri alır (bölüm sonu, ölüm, yeniden başlatma).</summary>
        public void ClearAllEffects()
        {
            if (activeEffects.Count == 0) return;

            for (int index = activeEffects.Count - 1; index >= 0; index--)
            {
                ActiveEffectEntry entry = activeEffects[index];
                entry.Effect.RemoveEffect();
                OnEffectEnded?.Invoke(entry.Data);
            }

            activeEffects.Clear();
        }

        /// <summary>
        /// Maden türünü etki nesnesine çevirir. Yeni bir maden eklendiğinde tek
        /// değiştirilecek yer burasıdır; henüz yazılmamış türler null döner.
        /// Gemiye/sahneye dokunan etkiler ihtiyaç duydukları bileşeni buradan alır;
        /// böylece etki sınıfları singleton aramak zorunda kalmaz.
        /// </summary>
        private IActiveEffect CreateEffect(MineDataSO data)
        {
            return data.EffectType switch
            {
                EffectType.StarFragment => new StarFragmentEffect(data),
                EffectType.GoldCrystal => new GoldCrystalEffect(data),
                EffectType.PlasmaFuel => CreatePlasmaFuelEffect(data),
                EffectType.TimeCrystal => new TimeCrystalEffect(data),
                EffectType.Shield => CreateShieldEffect(data),
                EffectType.MeteorCaller => CreateMeteorCallerEffect(data),
                EffectType.GravityStone => CreateGravityStoneEffect(data),
                _ => null,
            };
        }

        private IActiveEffect CreateGravityStoneEffect(MineDataSO data)
        {
            if (playerShipController == null)
            {
                Debug.LogWarning("[PowerUpManager] Yerçekimi Taşı çalışmadı: " +
                    "Player Ship Controller referansı Inspector'dan bağlanmamış.");
                return null;
            }

            return new GravityStoneEffect(data, playerShipController);
        }

        private IActiveEffect CreateMeteorCallerEffect(MineDataSO data)
        {
            if (meteorSpawner == null)
            {
                Debug.LogWarning("[PowerUpManager] Meteor Çağırıcı çalışmadı: " +
                    "Meteor Spawner referansı Inspector'dan bağlanmamış.");
                return null;
            }

            return new MeteorCallerEffect(data, meteorSpawner);
        }

        private IActiveEffect CreateShieldEffect(MineDataSO data)
        {
            if (shipCollisionHandler == null)
            {
                Debug.LogWarning("[PowerUpManager] Koruma Kalkanı çalışmadı: " +
                    "Ship Collision Handler referansı Inspector'dan bağlanmamış.");
                return null;
            }

            return new ShieldEffect(data, shipCollisionHandler);
        }

        private IActiveEffect CreatePlasmaFuelEffect(MineDataSO data)
        {
            if (playerShooter == null)
            {
                Debug.LogWarning("[PowerUpManager] Plazma Yakıtı çalışmadı: " +
                    "Player Shooter referansı Inspector'dan bağlanmamış.");
                return null;
            }

            return new PlasmaFuelEffect(data, playerShooter);
        }

        /// <summary>Aktif bir etki ve kalan süresi. Süre değiştiği için sınıf (referans tipi).</summary>
        private class ActiveEffectEntry
        {
            public ActiveEffectEntry(MineDataSO data, IActiveEffect effect)
            {
                Data = data;
                Effect = effect;
                RemainingTime = effect.Duration;
            }

            public MineDataSO Data { get; }
            public IActiveEffect Effect { get; }
            public float RemainingTime { get; set; }
        }
    }
}
