using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PayeGames.AsalYolculuk.UI
{
    /// <summary>
    /// Detay listesindeki tek bir satır (asalDetayHolder). Sıra numarasını,
    /// meteorun sayısını ve sayının asal çarpanlara ayrılmış hâlini yazar.
    /// Çarpanlar TMP rich text ile renklendirilir: her asal sayı her satırda
    /// aynı renkte çıkar, böylece tekrar eden çarpanlar gözle ayırt edilir.
    /// </summary>
    public class AsalDetayHolderView : MonoBehaviour
    {
        // Renk paleti asal sayıların sırasına göre atanır: 2, 3, 5, 7, 11, ...
        private static readonly int[] PrimeOrder = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47 };

        // Satır metni her seferinde yeni string ayırmamak için ortak tampon üzerinde kurulur.
        private static readonly StringBuilder Builder = new StringBuilder(64);

        [Header("Metinler")]
        [SerializeField] private TMP_Text siraTxt;
        [SerializeField] private TMP_Text asalSayiTxt;
        [SerializeField] private TMP_Text carpanlaraAyirTxt;

        [Header("Satır Zemini")]
        [Tooltip("Satırın arka plan görseli (asalHolderImg)")]
        [SerializeField] private Image asalHolderImg;
        [Tooltip("Tek sıra numaralı satırların rengi — #122045")]
        [SerializeField] private Color oddRowColor = new Color(0.071f, 0.125f, 0.271f);
        [Tooltip("Çift sıra numaralı satırların rengi — #0E1938")]
        [SerializeField] private Color evenRowColor = new Color(0.055f, 0.098f, 0.220f);

        [Header("Biçim")]
        [Tooltip("Çarpanlar büyükten küçüğe sıralansın (154 = 11 × 7 × 2). Kapalıyken küçükten büyüğe: 154 = 2 × 7 × 11")]
        [SerializeField] private bool descendingFactors;
        [SerializeField] private string equalsSeparator = " = ";
        [SerializeField] private string factorSeparator = " × ";
        [Tooltip("Meteorun kendisi asalken çarpan alanına yazılacak metin")]
        [SerializeField] private string primeLabel = "ASAL";

        [Header("Asal Renkleri")]
        [Tooltip("Sırasıyla 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 sayılarının renkleri")]
        [SerializeField]
        private Color[] factorColors =
        {
            new Color(1f, 0.541f, 0.239f),        // 2  — turuncu
            new Color(1f, 0.361f, 0.478f),        // 3  — kırmızı
            new Color(0.239f, 0.863f, 0.592f),    // 5  — yeşil
            new Color(0.710f, 0.482f, 1f),        // 7  — mor
            new Color(0.208f, 0.839f, 1f),        // 11 — camgöbeği
            new Color(0.608f, 0.882f, 0.365f),    // 13 — fıstık yeşili
            new Color(0.435f, 0.659f, 1f),        // 17 — açık mavi
            new Color(1f, 0.435f, 0.847f),        // 19 — pembe
            new Color(0.369f, 0.918f, 0.831f),    // 23 — turkuaz
            new Color(0.941f, 0.651f, 1f)         // 29 — lila
        };

        [Tooltip("Eşittir ve çarpı işaretlerinin rengi")]
        [SerializeField] private Color operatorColor = new Color(0.369f, 0.467f, 0.671f);
        [Tooltip("Sol taraftaki sayının (154 = ...) rengi")]
        [SerializeField] private Color numberColor = new Color(0.780f, 0.847f, 0.961f);
        [Tooltip("ASAL etiketinin rengi")]
        [SerializeField] private Color primeLabelColor = new Color(0.239f, 0.863f, 0.592f);

        private string[] factorColorHexList;
        private string operatorHex;
        private string numberHex;
        private string primeLabelHex;

        private void Awake()
        {
            CacheColorHexValues();
        }

        /// <summary>
        /// Renkler her satır kurulduğunda yeniden hex'e çevrilmesin diye bir kez
        /// önbelleğe alınır.
        /// </summary>
        private void CacheColorHexValues()
        {
            factorColorHexList = new string[factorColors.Length];

            for (int index = 0; index < factorColors.Length; index++)
            {
                factorColorHexList[index] = ColorUtility.ToHtmlStringRGB(factorColors[index]);
            }

            operatorHex = ColorUtility.ToHtmlStringRGB(operatorColor);
            numberHex = ColorUtility.ToHtmlStringRGB(numberColor);
            primeLabelHex = ColorUtility.ToHtmlStringRGB(primeLabelColor);
        }

        /// <summary>Satırı verilen meteor kaydına göre doldurur.</summary>
        public void Bind(AsalDetayManager.MeteorDetay detail)
        {
            if (factorColorHexList == null)
            {
                CacheColorHexValues();
            }

            ApplyRowColor(detail.Order);

            if (siraTxt != null)
            {
                siraTxt.text = detail.Order.ToString();
            }

            if (asalSayiTxt != null)
            {
                asalSayiTxt.text = detail.Number.ToString();
            }

            if (carpanlaraAyirTxt == null) return;

            carpanlaraAyirTxt.richText = true;
            carpanlaraAyirTxt.text = BuildFactorText(detail);
        }

        /// <summary>
        /// Satırlar dönüşümlü iki zemin rengiyle boyanır; uzun listede gözün
        /// satır kaymasını önler.
        /// </summary>
        private void ApplyRowColor(int order)
        {
            if (asalHolderImg == null) return;

            bool isOddRow = order % 2 != 0;
            asalHolderImg.color = isOddRow ? oddRowColor : evenRowColor;
        }

        /// <summary>
        /// Asal meteorlarda "37 = 37" anlamsız kalacağı için yalnızca ASAL etiketi
        /// yazılır; bileşik meteorlarda renkli çarpanlar dizilir.
        /// </summary>
        private string BuildFactorText(AsalDetayManager.MeteorDetay detail)
        {
            Builder.Clear();

            if (detail.IsPrime || detail.PrimeFactors == null || detail.PrimeFactors.Count < 2)
            {
                AppendColored(primeLabelHex, primeLabel);
                return Builder.ToString();
            }

            AppendColored(numberHex, detail.Number.ToString());
            AppendColored(operatorHex, equalsSeparator);

            int factorCount = detail.PrimeFactors.Count;

            for (int step = 0; step < factorCount; step++)
            {
                // Çarpanlar artan sırada tutulur; büyükten küçüğe istendiğinde
                // liste kopyalanmadan tersten okunur.
                int index = descendingFactors ? factorCount - 1 - step : step;
                int factor = detail.PrimeFactors[index];

                if (step > 0)
                {
                    AppendColored(operatorHex, factorSeparator);
                }

                AppendColored(GetFactorHex(factor), factor.ToString());
            }

            return Builder.ToString();
        }

        private void AppendColored(string colorHex, string value)
        {
            Builder.Append("<color=#").Append(colorHex).Append('>').Append(value).Append("</color>");
        }

        /// <summary>
        /// Rengi asalın büyüklük sırasına göre seçer (2 turuncu, 3 kırmızı, ...).
        /// Palette karşılığı olmayan büyük asallar renkleri döngüsel kullanır.
        /// </summary>
        private string GetFactorHex(int factor)
        {
            if (factorColorHexList.Length == 0) return numberHex;

            int primeIndex = System.Array.IndexOf(PrimeOrder, factor);
            if (primeIndex < 0)
            {
                primeIndex = Mathf.Abs(factor);
            }

            return factorColorHexList[primeIndex % factorColorHexList.Length];
        }
    }
}
