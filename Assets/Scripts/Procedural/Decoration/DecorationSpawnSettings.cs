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
        [SerializeField]
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
        public struct SpawnSetting
        {
            // Decoration
            public string name;
            [EnumToggleButtons]
            public Decoration.Type type;
            public bool isDestructible;

            // Spawn
            public string[] spawnableBiomes;
            [Range(0f, 1f)]
            public float spawnRate;

            // Render
            public GameObject[] decorationPrefabs;
            public bool useRandomRotation;
            public Vector3 scaleVariation;

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
            private readonly float totalSpawnRate;

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

                totalSpawnRate = weightTable[weightTable.Length - 1];
            }

            public SpawnSetting? SelectRandomDecorationSetBasedOnSpawnRate(System.Random random)
            {
                float randomValue = (float)random.NextDouble() * totalSpawnRate;
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