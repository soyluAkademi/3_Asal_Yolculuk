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

        [Header("Meteor Ayarları")]
        [Tooltip("Saniyede üretilen meteor sayısı; 0.4 = 2.5 saniyede bir meteor. " +
                 "Bölüm ilerledikçe artar, bölümün temel zorluk ayarıdır.")]
        [SerializeField, Range(0.1f, 3f)] private float meteorSpawnRate = 0.4f;

        [Header("Pedagojik Öncelikli Sayılar")]
        [Tooltip("Bu seviyede öğretilmek istenen öncelikli sayılar (%70 olasılıkla seçilir)")]
        [SerializeField] private List<int> priorityNumberList = new List<int>();

        [Header("Maden Ayarları")]
        [Tooltip("Bu gezegende ödül madenleri düşer mi (ilk gezegende kapalı: oyuncu önce temel döngüyü öğrenir)")]
        [SerializeField] private bool areRewardMinesEnabled = true;
        [Tooltip("Bu gezegende ceza madenleri çıkar mı")]
        [SerializeField] private bool areCurseMinesEnabled = true;
        [Tooltip("Maden sıklığı çarpanı; Kristal Gezegeni gibi 'yoğun maden' temalı bölümlerde artırılır")]
        [SerializeField] private float mineRateMultiplier = 1f;

        public int PlanetId => planetId;
        public string PlanetName => planetName;
        public Sprite PlanetSprite => planetSprite;

        /// <summary>Arayüzde gösterilen numaralı ad: "2. Asteroit Kuşağı".</summary>
        public string DisplayName => $"{planetId}. {planetName}";
        public int MinNumber => minNumber;
        public int MaxNumber => maxNumber;
        public float MeteorSpawnRate => meteorSpawnRate;
        public IReadOnlyList<int> PriorityNumberList => priorityNumberList;
        public bool AreRewardMinesEnabled => areRewardMinesEnabled;
        public bool AreCurseMinesEnabled => areCurseMinesEnabled;
        public float MineRateMultiplier => mineRateMultiplier;
    }
}
