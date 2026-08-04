using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;

namespace PayeGames.AsalYolculuk.Meteors
{
    /// <summary>
    /// Sahnedeki meteorları kendi patlama efektleriyle soldan sağa doğru sırayla
    /// temizler. Hem bölüm tamamlandığında (kendi kendine, durum değişimini dinleyerek)
    /// hem de ölüm sekansında (<see cref="Player.PlayerDeathEffect"/> tarafından
    /// çağrılarak) kullanılır.
    /// </summary>
    public class MeteorFieldClearer : MonoBehaviour
    {
        // Havadaki mermiler temizlik sürerken yeni parçalar doğurabildiği için sahne
        // birden fazla turda süpürülür. Sonsuz döngüye karşı üst sınır.
        private const int MaxClearPassCount = 12;

        [Header("Temizlik")]
        [Tooltip("Meteorların arka arkaya patlama aralığı (saniye)")]
        [SerializeField] private float clearInterval = 0.15f;
        [Tooltip("Bölüm tamamlandığında temizliğin başlaması için beklenen süre")]
        [SerializeField] private float levelCompleteDelay = 0.3f;

        private readonly List<MeteorController> clearBuffer = new List<MeteorController>();
        private Coroutine activeRoutine;

        private void Start()
        {
            // GameManager Awake'inde kurulduğu için abonelik Start'ta yapılır.
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            // Ölüm sekansındaki temizliği PlayerDeathEffect kendi zamanlamasıyla yürütür.
            if (state != GameState.LevelComplete) return;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(ClearAfterDelay(levelCompleteDelay));
        }

        private IEnumerator ClearAfterDelay(float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            yield return ClearRoutine();

            activeRoutine = null;
        }

        /// <summary>
        /// Sahne tamamen boşalana kadar meteorları sırayla patlatır. Bir tur sürerken
        /// vurulan meteorlardan yeni parçalar doğabildiği için tur tekrarlanır.
        /// </summary>
        public IEnumerator ClearRoutine()
        {
            int passCount = 0;

            while (MeteorController.ActiveMeteors.Count > 0 && passCount < MaxClearPassCount)
            {
                yield return ClearSinglePass();
                passCount++;
            }
        }

        /// <summary>
        /// O anda sahnede olan meteorları soldan sağa doğru tek tek patlatır. Liste
        /// kopyalanır; çünkü meteorlar patladıkça aktif meteor listesinden çıkar.
        /// </summary>
        private IEnumerator ClearSinglePass()
        {
            clearBuffer.Clear();
            clearBuffer.AddRange(MeteorController.ActiveMeteors);
            clearBuffer.Sort(CompareByHorizontalPosition);

            for (int index = 0; index < clearBuffer.Count; index++)
            {
                MeteorController meteor = clearBuffer[index];
                if (meteor == null) continue;
                if (!meteor.gameObject.activeInHierarchy) continue;

                meteor.DespawnWithExplosion();

                yield return new WaitForSeconds(clearInterval);
            }

            clearBuffer.Clear();
        }

        private static int CompareByHorizontalPosition(MeteorController first, MeteorController second)
        {
            if (first == null || second == null) return 0;

            return first.transform.position.x.CompareTo(second.transform.position.x);
        }
    }
}
