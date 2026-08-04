using System.Collections.Generic;
using UnityEngine;

namespace PayeGames.AsalYolculuk.Data
{
    [CreateAssetMenu(fileName = "PlanetData", menuName = "PayeGames/AsalYolculuk/Planet Data")]
    public class PlanetDataSO : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private int planetId;
        [SerializeField] private string planetName;
        [Tooltip("Harita ve bölüm sonu ekranlarında gösterilen gezegen görseli")]
        [SerializeField] private Sprite planetSprite;

        [Header("Sayı Aralığı")]
        [SerializeField] private int minNumber = 2;
        [SerializeField] private int maxNumber = 100;

        [Header("Pedagojik Öncelikli Sayılar")]
        [Tooltip("Bu seviyede öğretilmek istenen öncelikli sayılar (%70 olasılıkla seçilir)")]
        [SerializeField] private List<int> priorityNumberList = new List<int>();

        public int PlanetId => planetId;
        public string PlanetName => planetName;
        public Sprite PlanetSprite => planetSprite;

        /// <summary>Arayüzde gösterilen numaralı ad: "2. Asteroit Kuşağı".</summary>
        public string DisplayName => $"{planetId}. {planetName}";
        public int MinNumber => minNumber;
        public int MaxNumber => maxNumber;
        public IReadOnlyList<int> PriorityNumberList => priorityNumberList;
    }
}
