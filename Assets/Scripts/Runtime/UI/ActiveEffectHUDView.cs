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

            if (slots == null || slots.Length == 0)
            {
                Debug.LogWarning("[ActiveEffectHUDView] Slots dizisi boş; gösterge çalışmaz.", this);
            }

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

        private void HandleEffectStarted(MineDataSO data)
        {
            if (data == null) return;
            if (shownEffects.Contains(data)) return;

            shownEffects.Add(data);
            RefreshSlots();
        }

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
