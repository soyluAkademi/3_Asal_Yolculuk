using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using PayeGames.AsalYolculuk.Core;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.EditorTools
{
    public static class PlanetDataGenerator
    {
        private const string OutputFolder = "Assets/Scripts/Runtime/ScriptableObjects/Planets";

        /// <summary>
        /// GDD kuralı: bir meteor en fazla 5 asal çarpana ayrılabilir.
        /// Öncelikli listeye bunu aşan sayı girerse üretim uyarı verir.
        /// </summary>
        private const int MaxPrimeFactorCount = 5;

        // Öncelikli sayı listeleri (GDD'nin %70 pedagojik sayı kuralı bunları kullanır).
        // Gezegen temasına göre kurgulanmıştır; hepsi kendi aralığı içinde ve
        // en fazla 5 asal çarpanlıdır (GeneratePlanetData bunu doğrular).

        // Temel asal çarpan mantığı: küçük asallar + 2-3 çarpanlı tanıdık bileşikler.
        private static readonly int[] Planet1Numbers =
        {
            4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
            23, 24, 25, 27, 30, 32, 36, 45, 48,
        };

        // Meteor yoğunluğu artar: aynı mantık, daha fazla çarpan adımı.
        private static readonly int[] Planet2Numbers =
        {
            24, 27, 28, 29, 30, 32, 36, 37, 40, 42, 44, 45, 48, 50, 52, 53, 54,
            56, 60, 63, 66, 70, 71, 72, 75, 80, 81, 84, 88, 89, 90,
        };

        // Lav Gezegeni: asal ağırlıklı (20 asal / 12 bileşik), bileşikler kolay yarı-asal.
        private static readonly int[] Planet3Numbers =
        {
            11, 13, 17, 19, 23, 26, 29, 31, 33, 34, 37, 39, 41, 43, 47, 51, 53,
            57, 58, 59, 61, 62, 67, 69, 71, 73, 74, 79, 83, 85, 87, 89,
        };

        // Buz Gezegeni: daha büyük sayılar, daha fazla parçalama adımı (4-5 çarpan ağırlıklı).
        private static readonly int[] Planet4Numbers =
        {
            48, 53, 54, 56, 60, 63, 68, 72, 75, 79, 80, 81, 84, 88, 90, 97, 98,
            99, 100, 104, 108, 110, 112, 113, 117, 120,
        };

        // Fırtına Gezegeni: geniş aralık, çok kademeli bölünen sayılar.
        private static readonly int[] Planet5Numbers =
        {
            60, 61, 63, 66, 70, 75, 78, 84, 89, 90, 98, 102, 105, 110, 114, 117,
            120, 126, 127, 130, 132, 135, 138, 140, 150, 154, 156, 162, 165, 168,
            170, 173, 174, 180,
        };

        // Kara Delik Sınırı: "büyük ara çarpanlara ayrılma" teması — ağırlıklı olarak
        // iki büyük asalın çarpımı (yarı-asal) ve asal kareler.
        private static readonly int[] Planet6Numbers =
        {
            77, 82, 85, 86, 87, 91, 93, 94, 95, 106, 111, 115, 118, 119, 121, 122,
            123, 129, 133, 134, 141, 142, 143, 145, 146, 149, 155, 158, 159, 161,
            166, 169, 177, 181, 183, 185, 187, 194, 197,
        };

        // Kristal Gezegeni: karmaşık sayılar, çarpan yapısı çeşitlenir.
        private static readonly int[] Planet7Numbers =
        {
            102, 103, 105, 110, 114, 117, 126, 130, 132, 135, 138, 140, 149, 150,
            154, 156, 165, 170, 174, 182, 186, 189, 190, 195, 198, 199, 200, 204,
            210, 222, 227, 231, 234, 238,
        };

        // Asal Krallığı: final — yarı-asallar, 5 çarpanlı büyük bileşikler ve büyük asallar bir arada.
        private static readonly int[] Planet8Numbers =
        {
            154, 165, 168, 180, 182, 190, 195, 198, 200, 203, 209, 210, 211, 221,
            231, 238, 247, 253, 255, 257, 264, 266, 270, 273, 286, 299, 307, 310,
            312, 322, 323, 330, 349, 357, 374, 378, 385, 390, 399, 401, 405, 418,
            429, 442, 455, 456, 462, 483, 494, 500, 506, 510, 527, 533,
        };

        // Maden kademeli açılımı: 1. gezegende hiç maden yok (oyuncu önce temel
        // döngüyü öğrenir), 2.'de yalnızca ödüller, 3.'ten itibaren cezalar da girer.
        // Kristal Gezegeni GDD'de "yoğun maden kullanımı" olarak tanımlı, Asal
        // Krallığı ise tüm mekaniklerin birleştiği final bölgesi.
        private static readonly MineSettings NoMines = new MineSettings(false, false, 1f);
        private static readonly MineSettings RewardsOnly = new MineSettings(true, false, 1f);
        private static readonly MineSettings AllMines = new MineSettings(true, true, 1f);
        private static readonly MineSettings DenseMines = new MineSettings(true, true, 2f);
        private static readonly MineSettings FinalMines = new MineSettings(true, true, 1.5f);

        // Meteor üretim hızı (saniyedeki meteor sayısı). Artış sona doğru bilinçli
        // olarak yavaşlar: ileri gezegenlerde sayılar da büyüdüğü ve daha çok parçaya
        // ayrıldığı için sıklık aynı tempoda artarsa final bölüm oynanamaz hale gelir.
        private static readonly PlanetDefinition[] Planets =
        {
            new PlanetDefinition(1, "Başlangıç Gezegeni", 4, 48, 0.4f, Planet1Numbers, NoMines),
            new PlanetDefinition(2, "Asteroit Kuşağı", 24, 90, 0.55f, Planet2Numbers, RewardsOnly),
            new PlanetDefinition(3, "Lav Gezegeni", 11, 89, 0.7f, Planet3Numbers, AllMines),
            new PlanetDefinition(4, "Buz Gezegeni", 48, 120, 0.82f, Planet4Numbers, AllMines),
            new PlanetDefinition(5, "Fırtına Gezegeni", 60, 180, 0.92f, Planet5Numbers, AllMines),
            new PlanetDefinition(6, "Kara Delik Sınırı", 72, 200, 1.02f, Planet6Numbers, AllMines),
            new PlanetDefinition(7, "Kristal Gezegeni", 100, 240, 1.1f, Planet7Numbers, DenseMines),
            new PlanetDefinition(8, "Asal Krallığı", 150, 540, 1.2f, Planet8Numbers, FinalMines),
        };

        [MenuItem("Tools/Paye Games/Asal Yolculuk/Generate Planet Data")]
        private static void GeneratePlanetData()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
            }

            foreach (PlanetDefinition definition in Planets)
            {
                ValidateNumbers(definition);
                CreateOrUpdateAsset(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Öncelikli sayıları GDD kurallarına göre denetler: sayı kendi gezegeninin
        /// aralığında olmalı ve en fazla <see cref="MaxPrimeFactorCount"/> asal çarpanı
        /// bulunmalı. Üretimi durdurmaz; ihlalleri tek bir uyarıda toplar.
        /// </summary>
        private static void ValidateNumbers(PlanetDefinition definition)
        {
            StringBuilder issues = null;

            foreach (int number in definition.PriorityNumbers)
            {
                if (number < definition.MinNumber || number > definition.MaxNumber)
                {
                    AppendIssue(ref issues, $"{number} aralık dışı " +
                        $"[{definition.MinNumber}-{definition.MaxNumber}]");
                    continue;
                }

                int factorCount = PrimeMathEngine.GetPrimeFactors(number).Count;
                if (factorCount > MaxPrimeFactorCount)
                {
                    AppendIssue(ref issues, $"{number} sayısının {factorCount} asal " +
                        $"çarpanı var (üst sınır {MaxPrimeFactorCount})");
                }
            }

            if (issues == null) return;

            Debug.LogWarning($"[PlanetDataGenerator] Planet_{definition.PlanetId} " +
                $"({definition.PlanetName}) öncelikli sayı uyarıları: {issues}");
        }

        private static void AppendIssue(ref StringBuilder issues, string message)
        {
            if (issues == null)
            {
                issues = new StringBuilder();
            }
            else
            {
                issues.Append(", ");
            }

            issues.Append(message);
        }

        private static void CreateOrUpdateAsset(PlanetDefinition definition)
        {
            string assetPath = $"{OutputFolder}/Planet_{definition.PlanetId}.asset";
            PlanetDataSO planetData = AssetDatabase.LoadAssetAtPath<PlanetDataSO>(assetPath);

            if (planetData == null)
            {
                planetData = ScriptableObject.CreateInstance<PlanetDataSO>();
                AssetDatabase.CreateAsset(planetData, assetPath);
            }

            SerializedObject serializedObject = new SerializedObject(planetData);
            serializedObject.FindProperty("planetId").intValue = definition.PlanetId;
            serializedObject.FindProperty("planetName").stringValue = definition.PlanetName;
            serializedObject.FindProperty("minNumber").intValue = definition.MinNumber;
            serializedObject.FindProperty("maxNumber").intValue = definition.MaxNumber;
            serializedObject.FindProperty("meteorSpawnRate").floatValue = definition.MeteorSpawnRate;
            WritePriorityNumbers(serializedObject, definition.PriorityNumbers);
            WriteMineSettings(serializedObject, definition.Mines);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(planetData);
        }

        /// <summary>
        /// Öncelikli sayı listesini asset'e yazar. Liste **tamamen değiştirilir** —
        /// bu araç sayıların tek kaynağıdır, Inspector'dan yapılan elle düzenlemeler
        /// yeniden üretimde kaybolur. Kalıcı değişiklik için yukarıdaki dizileri düzenle.
        /// </summary>
        private static void WritePriorityNumbers(SerializedObject serializedObject, int[] numbers)
        {
            SerializedProperty listProperty = serializedObject.FindProperty("priorityNumberList");
            listProperty.arraySize = numbers.Length;

            for (int index = 0; index < numbers.Length; index++)
            {
                listProperty.GetArrayElementAtIndex(index).intValue = numbers[index];
            }
        }

        /// <summary>
        /// Gezegenin maden ayarlarını asset'e yazar. Sayı listesi gibi bu alanlar da
        /// tamamen değiştirilir; kalıcı değişiklik yukarıdaki tanımlardan yapılır.
        /// </summary>
        private static void WriteMineSettings(SerializedObject serializedObject, MineSettings mines)
        {
            serializedObject.FindProperty("areRewardMinesEnabled").boolValue = mines.AreRewardsEnabled;
            serializedObject.FindProperty("areCurseMinesEnabled").boolValue = mines.AreCursesEnabled;
            serializedObject.FindProperty("mineRateMultiplier").floatValue = mines.RateMultiplier;
        }

        private readonly struct MineSettings
        {
            public MineSettings(bool areRewardsEnabled, bool areCursesEnabled, float rateMultiplier)
            {
                AreRewardsEnabled = areRewardsEnabled;
                AreCursesEnabled = areCursesEnabled;
                RateMultiplier = rateMultiplier;
            }

            public bool AreRewardsEnabled { get; }
            public bool AreCursesEnabled { get; }
            public float RateMultiplier { get; }
        }

        private readonly struct PlanetDefinition
        {
            public PlanetDefinition(int planetId, string planetName, int minNumber,
                int maxNumber, float meteorSpawnRate, int[] priorityNumbers, MineSettings mines)
            {
                PlanetId = planetId;
                PlanetName = planetName;
                MinNumber = minNumber;
                MaxNumber = maxNumber;
                MeteorSpawnRate = meteorSpawnRate;
                PriorityNumbers = priorityNumbers;
                Mines = mines;
            }

            public int PlanetId { get; }
            public string PlanetName { get; }
            public int MinNumber { get; }
            public int MaxNumber { get; }
            public float MeteorSpawnRate { get; }
            public int[] PriorityNumbers { get; }
            public MineSettings Mines { get; }
        }
    }
}
