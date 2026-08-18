using UnityEngine;

namespace PayeGames.AsalYolculuk.PowerUps
{
    /// <summary>
    /// Bir maden türünün verisi: görseli, süresi, miktarı ve toplama geri bildirimi.
    /// Davranış <see cref="IActiveEffect"/> uygulayan sınıflarda, veri burada durur.
    /// </summary>
    [CreateAssetMenu(fileName = "MineData", menuName = "PayeGames/AsalYolculuk/Mine Data")]
    public class MineDataSO : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private EffectType effectType;
        [Tooltip("HUD'da ve geliştirici araçlarında gösterilen ad (ör. Yıldız Parçası)")]
        [SerializeField] private string displayName;
        [Tooltip("Sahnedeki maden görseli; HUD göstergesinde de aynı ikon kullanılır")]
        [SerializeField] private Sprite icon;

        [Header("Etki")]
        [Tooltip("Etkinin süresi (saniye). 0 = anlık etki, HUD'da geri sayım çıkmaz")]
        [SerializeField] private float duration = 10f;
        [Tooltip("Sayı bazlı madenlerin miktarı (Yıldız Parçası +10 puan, Altın Kristal meteor başına puan, Plazma Yakıtı 3 atış noktası)")]
        [SerializeField] private int amount = 10;
        [Tooltip("Çarpan bazlı madenlerin katsayısı (Plazma Yakıtı ateş hızı ×2, Zaman Kristali meteor hızı ×0.5)")]
        [SerializeField] private float multiplier = 2f;

        [Header("Toplama Geri Bildirimi")]
        [SerializeField] private GameObject pickupEffectPrefab;
        [SerializeField] private AudioClip pickupClip;

        public EffectType EffectType => effectType;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public float Duration => duration;
        public int Amount => amount;
        public float Multiplier => multiplier;
        public GameObject PickupEffectPrefab => pickupEffectPrefab;
        public AudioClip PickupClip => pickupClip;

        /// <summary>Ödül mü ceza mı; karar <see cref="EffectTypeExtensions"/>'da.</summary>
        public bool IsReward => effectType.IsReward();
    }
}
