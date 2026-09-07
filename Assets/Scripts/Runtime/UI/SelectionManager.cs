using System;
using PayeGames.AsalYolculuk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Seçim panelindeki 8 bölüm butonunun durum görsellerini ve bölüm seçimini
    /// yönetir. Her buton için üç durum objesinden her zaman yalnızca biri açıktır:
    ///
    /// - Tamamlanmış bölümler        -> greenObject
    /// - Sıradaki (oynanmamış) bölüm -> unlockedLockObject (açık kilit)
    /// - Henüz gelmemiş bölümler     -> lockedLockObject (kapalı kilit)
    ///
    /// Açık olan bölümler (tamamlananlar + sıradaki) tıklanabilir; tıklanan
    /// bölümün chooseImg'i açılır, diğerlerininki kapanır ve seçim
    /// <see cref="PlanetProgress"/> üzerinden PlayerPrefs'e yazılır. Oyna
    /// butonuyla GamePlay sahnesine geçildiğinde <see cref="LevelManager"/>
    /// bu kaydı okuyup seçilen bölümü başlatır.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        [Serializable]
        private struct PlanetSlot
        {
            [Tooltip("Bölümün kendi butonu; kilitliyken interactable kapatılır.")]
            public Button button;

            [Tooltip("Bölüm tamamlandığında açılan yeşil daire.")]
            public GameObject greenObject;

            [Tooltip("Sıradaki oynanacak bölümde açılan açık kilit objesi.")]
            public GameObject unlockedLockObject;

            [Tooltip("Henüz sırası gelmemiş bölümlerde açılan kapalı kilit objesi.")]
            public GameObject lockedLockObject;

            [Tooltip("Bu bölüm seçiliyken açılan seçim işareti (chooseImg).")]
            public GameObject chooseObject;
        }

        [Header("Bölüm Butonları (1. bölümden 8.'ye sıralı)")]
        [SerializeField] private PlanetSlot[] planetSlots;

        private void Awake()
        {
            RegisterButtons();
        }

        private void OnEnable()
        {
            RefreshSlots();
        }

        /// <summary>
        /// Tüm butonların durum görsellerini ve tıklanabilirliğini kayıtlı
        /// ilerlemeye göre yeniden yazar; ardından seçimi kayıtlı bölüme alır.
        /// Panel her açıldığında çalışır, yani bölüm bitirilip menüye dönüldüğünde
        /// güncel durum görünür.
        /// </summary>
        public void RefreshSlots()
        {
            if (planetSlots == null) return;

            int completedCount = PlanetProgress.CompletedCount;
            int currentIndex = PlanetProgress.MaxUnlockedIndex;

            for (int i = 0; i < planetSlots.Length; i++)
            {
                // Tamamlanma ayrı sayılır: son bölüm bitince kilit index'i
                // ilerlemez, o bölüm de yeşil görünmelidir.
                bool isCompleted = i < completedCount;
                bool isCurrent = !isCompleted && i == currentIndex;

                ApplySlotState(planetSlots[i], isCompleted, isCurrent);
            }

            // Oyuncu hiçbir şeye tıklamadan Oyna'ya basarsa kayıtlı seçim geçerli
            // olsun diye görsel işaret de o bölüme çekilir.
            ApplySelectionVisual(PlanetProgress.SelectedIndex);
        }

        /// <summary>
        /// Bölüm butonuna basıldığında çağrılır. Kilitli bölüm seçilemez;
        /// seçim PlayerPrefs'e yazılır ve yalnızca o bölümün chooseImg'i açık kalır.
        /// </summary>
        public void SelectPlanet(int planetIndex)
        {
            if (!PlanetProgress.IsUnlocked(planetIndex)) return;

            PlanetProgress.Select(planetIndex);
            ApplySelectionVisual(planetIndex);
        }

        private void ApplySelectionVisual(int selectedIndex)
        {
            if (planetSlots == null) return;

            for (int i = 0; i < planetSlots.Length; i++)
            {
                SetActiveSafe(planetSlots[i].chooseObject, i == selectedIndex);
            }
        }

        private static void ApplySlotState(PlanetSlot slot, bool isCompleted, bool isCurrent)
        {
            SetActiveSafe(slot.greenObject, isCompleted);
            SetActiveSafe(slot.unlockedLockObject, isCurrent);
            SetActiveSafe(slot.lockedLockObject, !isCompleted && !isCurrent);

            if (slot.button == null) return;

            slot.button.interactable = isCompleted || isCurrent;
        }

        /// <summary>
        /// Tıklama davranışını bir kez bağlar; index closure ile yakalandığı için
        /// her buton kendi bölümünü seçer. Butonda AudioSource varsa tık sesi de
        /// çalınır (referans tıklamada değil, burada bir kez alınır).
        /// </summary>
        private void RegisterButtons()
        {
            if (planetSlots == null) return;

            for (int i = 0; i < planetSlots.Length; i++)
            {
                Button planetButton = planetSlots[i].button;
                if (planetButton == null) continue;

                int planetIndex = i;

                if (planetButton.TryGetComponent(out AudioSource clickSource))
                {
                    planetButton.onClick.AddListener(() => SoundManager.PlayUiSfx(clickSource));
                }

                planetButton.onClick.AddListener(() => SelectPlanet(planetIndex));
            }
        }

        private static void SetActiveSafe(GameObject target, bool isActive)
        {
            if (target == null) return;
            if (target.activeSelf == isActive) return;

            target.SetActive(isActive);
        }
    }
}
