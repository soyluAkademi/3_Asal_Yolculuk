using System.IO;
using UnityEditor;
using UnityEngine;
using PayeGames.AsalYolculuk.Data;

namespace PayeGames.AsalYolculuk.EditorTools
{
    public static class PlanetDataGenerator
    {
        private const string OutputFolder = "Assets/Scripts/Runtime/ScriptableObjects";

        private static readonly PlanetDefinition[] Planets =
        {
            new PlanetDefinition(1, "Başlangıç Gezegeni", 4, 48),
            new PlanetDefinition(2, "Asteroit Kuşağı", 24, 90),
            new PlanetDefinition(3, "Lav Gezegeni", 11, 89),
            new PlanetDefinition(4, "Buz Gezegeni", 48, 120),
            new PlanetDefinition(5, "Fırtına Gezegeni", 60, 180),
            new PlanetDefinition(6, "Kara Delik Sınırı", 72, 200),
            new PlanetDefinition(7, "Kristal Gezegeni", 100, 240),
            new PlanetDefinition(8, "Asal Krallığı", 150, 540),
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
                CreateOrUpdateAsset(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(planetData);
        }

        private readonly struct PlanetDefinition
        {
            public PlanetDefinition(int planetId, string planetName, int minNumber, int maxNumber)
            {
                PlanetId = planetId;
                PlanetName = planetName;
                MinNumber = minNumber;
                MaxNumber = maxNumber;
            }

            public int PlanetId { get; }
            public string PlanetName { get; }
            public int MinNumber { get; }
            public int MaxNumber { get; }
        }
    }
}
