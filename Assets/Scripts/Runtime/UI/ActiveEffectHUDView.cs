using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.PowerUps;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Aktif süreli etkileri ikon + geri sayım olarak gösterir. Mantık
    /// <see cref="PowerUpManager"/>'da; bu sınıf yalnızca durumu çizer.
    /// Anlık etkiler (Yıldız Parçası) burada görünmez — gösterilecek süreleri yoktur.
    /// </summary>
    public class ActiveEffectHUDView : MonoBehaviour
    {
        // Süreli maden sayısı: 8 madenin 7'sinin süresi var (Yıldız Parçası anlık).
        // Teorik olarak hepsi aynı anda aktif olabilir.
        private const int RequiredSlotCount = 7;

        [Header("Kutucuklar")]
        [Tooltip("Soldan sağa sıralı slot'lar. Süreli maden sayısı kadar olmalı (7)")]
        [SerializeField] private ActiveEffectSlotView[] slots;

        [Header("Geri Sayım Rengi")]
        [SerializeField] private Color rewardTimeColor = Color.white;
        [SerializeField] private Color curseTimeColor = new Color(1f, 0.45f, 0.45f);

        // Gösterilen etkiler, toplanma sırasında. Slot'lara bu sırayla dağıtılır.
        private readonly List<MineDataSO> shownEffects = new List<MineDataSO>();

        // Manager'ın Awake'i garanti çalışmış olsun diye abonelik Start'ta yapılır.
        private void Start()
        {
            HideAllSlots();

            ValidateSlots();

            if (PowerUpManager.Instance == null)
            {
                Debug.LogWarning("[ActiveEffectHUDView] Sahnede PowerUpManager yok; " +
                    "gösterge hiçbir etkiyi duyamaz.", this);
                return;
            }

            PowerUpManager.Instance.OnEffectStarted += HandleEffectStarted;
            PowerUpManager.Instance.OnEffectEnded += HandleEffectEnded;
        }

        private void OnDestroy()
        {
            if (PowerUpManager.Instance == null) return;

            PowerUpManager.Instance.OnEffectStarted -= HandleEffectStarted;
            PowerUpManager.Instance.OnEffectEnded -= HandleEffectEnded;
        }

        /// <summary>
        /// Slot dizisini denetler. Eksik veya boş bırakılmış slot, etkiyi sessizce
        /// yutar: ilk etki görünür, ikinci ve sonrakiler kaybolur. Bu, "bazı madenler
        /// HUD'da çıkmıyor" şeklinde teşhisi zor bir belirti üretir.
        /// </summary>
        private void ValidateSlots()
        {
            if (slots == null || slots.Length == 0)
            {
                Debug.LogWarning("[ActiveEffectHUDView] Slots dizisi boş; gösterge çalışmaz.", this);
                return;
            }

            int emptyCount = 0;
            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] == null) emptyCount++;
            }

            if (emptyCount > 0)
            {
                Debug.LogWarning($"[ActiveEffectHUDView] Slots dizisinde {emptyCount} boş " +
                    "eleman var; o sıraya düşen etkiler gösterilemez.", this);
            }

            if (slots.Length >= RequiredSlotCount) return;

            Debug.LogWarning($"[ActiveEffectHUDView] Slot sayısı {slots.Length}; " +
                $"aynı anda {RequiredSlotCount} süreli etki aktif olabilir. " +
                "Fazlası gösterilemez.", this);
        }

        private void HandleEffectStarted(MineDataSO data)
        {
            if (data == null) return;
            if (shownEffects.Contains(data)) return;

            shownEffects.Add(data);

            if (shownEffects.Count > SlotCount)
            {
                Debug.LogWarning($"[ActiveEffectHUDView] '{data.DisplayName}' gösterilemedi: " +
                    $"{shownEffects.Count} etki aktif ama yalnızca {SlotCount} slot var.", this);
            }

            RefreshSlots();
        }

        private int SlotCount => slots != null ? slots.Length : 0;

        private void HandleEffectEnded(MineDataSO data)
        {
            if (!shownEffects.Remove(data)) return;

            RefreshSlots();
        }

        /// <summary>
        /// Etkileri baştan slot'lara dağıtır. Ortadaki etki bittiğinde boşluk
        /// kalmaması için tüm dizi yeniden yazılır; sıralamayı Layout Group toparlar.
        /// </summary>
        private void RefreshSlots()
        {
            if (slots == null) return;

            for (int index = 0; index < slots.Length; index++)
            {
                ActiveEffectSlotView slot = slots[index];
                if (slot == null) continue;

                if (index >= shownEffects.Count)
                {
                    slot.Hide();
                    continue;
                }

                MineDataSO data = shownEffects[index];
                slot.Show(data.Icon, data.IsReward ? rewardTimeColor : curseTimeColor);
            }
        }

        private void HideAllSlots()
        {
            if (slots == null) return;

            for (int index = 0; index < slots.Length; index++)
            {
                if (slots[index] == null) continue;

                // Bölüm açılışında yedi kutucuğun tek tek solması görülmemeli.
                slots[index].HideImmediate();
            }
        }

        private void Update()
        {
            if (shownEffects.Count == 0) return;
            if (PowerUpManager.Instance == null) return;

            int slotCount = slots != null ? slots.Length : 0;
            int visibleCount = Mathf.Min(shownEffects.Count, slotCount);

            for (int index = 0; index < visibleCount; index++)
            {
                ActiveEffectSlotView slot = slots[index];
                if (slot == null) continue;

                float remainingTime = PowerUpManager.Instance.GetRemainingTime(shownEffects[index].EffectType);
                slot.SetRemainingTime(remainingTime);
            }
        }
    }
}
