using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [CreateAssetMenu(menuName = "Tile Puzzle/Decoration Spawn Settings")]
    public class DecorationSpawnSettings : ScriptableObject
    {
        [Title("Mountain"), HideLabel]
        public MountainSpawnSetting mountainSpawnSetting;

        [Title("Forest"), HideLabel]
        public ForestSpawnSetting forestSpawnSetting;

        [Title("Vegetation")]
        [SerializeField, ListDrawerSettings(NumberOfItemsPerPage = 5)]
        private SpawnSetting[] spawnSettings;

        public IReadOnlyDictionary<int, SpawnTable> SpawnTableByBiomeId { get; private set; }

        private void OnEnable()
        {
            UpdateSpawnTable();
        }

        private void OnValidate()
        {
            UpdateSpawnTable();
        }

        private void UpdateSpawnTable()
        {
            var spawnSettingsByBiomeId = new Dictionary<int, List<SpawnSetting>>();
            foreach (SpawnSetting spawnSetting in spawnSettings)
            {
                foreach (string biomeName in spawnSetting.spawnableBiomes)
                {
                    int biomeId = Biome.BiomeNameToId(biomeName);
                    if (spawnSettingsByBiomeId.TryGetValue(biomeId, out List<SpawnSetting> spawnSettingsInBiome))
                    {
                        spawnSettingsInBiome.Add(spawnSetting);
                    }
                    else
                    {
                        spawnSettingsByBiomeId.Add(biomeId, new List<SpawnSetting> { spawnSetting });
                    }
                }
            }

            SpawnTableByBiomeId = spawnSettingsByBiomeId
                .ToDictionary(k => k.Key, v => new SpawnTable(v.Value.ToArray()));
        }

        [Serializable]
        public struct MountainSpawnSetting
        {
            public Vector2 spawnRange;
            public float spawnRate;
            public GameObject[] decorationPrefabs;
            public NoiseSettings noiseSettings;
        }

        [Serializable]
        public struct ForestSpawnSetting
        {
            public int spawnDistance;
            public float spawnRate;
            public BiomeForest[] biomeForests;
            public int tryCount;
            public bool useRandomRotation;
            public Dictionary<int, GameObject[]> BiomeDecorationTable => biomeForests.ToDictionary(k => Biome.BiomeNameToId(k.biomeName), v => v.decorationPrefabs);

            [Serializable]
            public struct BiomeForest
            {
                public string biomeName;
                public GameObject[] decorationPrefabs;
            }
        }

        [Serializable]
        public struct SpawnSetting
        {
            // Decoration
            [TabGroup("Decoration Info"), Required] public string name;
            [TabGroup("Decoration Info"), EnumToggleButtons] public Decoration.Type type;
            [TabGroup("Decoration Info")] public bool isDestructible;

            // Spawn
            [TabGroup("Spawn")] public string[] spawnableBiomes;
            [TabGroup("Spawn"), Range(0f, 1f)] public float spawnRate;

            // Render
            [TabGroup("Render")] public GameObject[] decorationPrefabs;
            [TabGroup("Render")] public bool useRandomRotation;
            [TabGroup("Render")] public Vector3 scaleVariation;

            public void Validate()
            {
                foreach (GameObject gameObject in decorationPrefabs)
                {
                    var meshFilter = gameObject.GetComponent<MeshFilter>();
                    var meshRenderer = gameObject.GetComponent<MeshRenderer>();
                    Debug.Assert(meshFilter != null && meshFilter.sharedMesh != null, $"Missing mesh, decoration: {name}");
                    Debug.Assert(meshRenderer != null && meshRenderer.sharedMaterials != null, $"Missing materials, decoration: {name}");
                }
                Debug.Assert(spawnableBiomes != null && spawnableBiomes.Length > 0, $"{nameof(spawnableBiomes)} is empty, decoration: {name}");

                spawnRate = Mathf.Clamp01(spawnRate);
            }
        }

        public struct SpawnTable
        {
            private readonly SpawnSetting[] spawnSettingTable;
            private readonly float[] weightTable;
            private readonly float randomRange;

            public SpawnTable(IEnumerable<SpawnSetting> spawnSettings)
            {
                spawnSettingTable = spawnSettings.ToArray();

                float previousWeightSum = 0;
                weightTable = new float[spawnSettingTable.Length];
                for (int i = 0; i < weightTable.Length; i++)
                {
                    weightTable[i] = spawnSettingTable[i].spawnRate + previousWeightSum;
                    previousWeightSum = weightTable[i];
                }

                randomRange = Mathf.Max(weightTable[weightTable.Length - 1], 1);
            }

            public SpawnSetting? SelectRandomDecorationSetBasedOnSpawnRate(System.Random random)
            {
                float randomValue = (float)random.NextDouble() * randomRange;
                int index = Array.BinarySearch(weightTable, randomValue);
                if (index < 0)
                {
                    index = -index - 1;
                }

                if (index < spawnSettingTable.Length)
                {
                    return spawnSettingTable[index];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}