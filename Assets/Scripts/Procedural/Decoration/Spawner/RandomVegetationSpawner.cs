using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [CreateAssetMenu(menuName = "Tile Puzzle/Decoration Spawner/Random Vegetation Spawner")]
    public class RandomVegetationSpawner : DecorationSpawner
    {
        public DecorationPrefabData[] prefabDatas;
        public float spawnRate;
        public bool useRandomRotation;
        // TODO: 물 위에도 생성 할 수 있는 옵션

        public override DecorationData Spawn(int seed, TerrainData terrainData, DecorationData inputDecorationData)
        {
            if (terrainData.TerrainSize != inputDecorationData.mapSize)
            {
                throw new InvalidOperationException($"{nameof(terrainData)}와 {nameof(inputDecorationData)}의 지형 크기가 같지 않음");
            }

            int mapLength = terrainData.terrainGraph.centers.Length;

            var prefabsByBiome = new Dictionary<int, List<GameObject>>();
            foreach (var prefabData in prefabDatas)
            {
                foreach (string biomeName in prefabData.spawnableBiomeNames)
                {
                    int biomeId = Biome.BiomeNameToId(biomeName);
                    if (prefabsByBiome.TryGetValue(biomeId, out List<GameObject> biomePrefabs))
                    {
                        biomePrefabs.AddRange(prefabData.prefabs);
                    }
                    else
                    {
                        prefabsByBiome.Add(biomeId, prefabData.prefabs.ToList());
                    }
                }
            }

            int salt = StringHash.SDBMLower(saltString);
            System.Random random = new System.Random(seed + salt);
            for (int i = 0; i < mapLength; i++)
            {
                if (random.NextDouble() > spawnRate)
                {
                    continue;
                }

                Center hexagonCenter = terrainData.terrainGraph.centers[i];
                if (hexagonCenter.isWater)
                {
                    continue;
                }

                if (inputDecorationData.decorationInfos[i].HasValue)
                {
                    continue;
                }

                // 바이옴에 맞는 랜덤 프리팹 선택
                int biomeId = terrainData.terrainGraph.centers[i].biomeId;
                if (prefabsByBiome.TryGetValue(biomeId, out List<GameObject> prefabs) == false)
                {
                    continue;
                }
                GameObject randomPrefab = prefabs[random.Next(prefabs.Count)];

                // 렌더 데이터 생성
                Vector3 scale = randomPrefab.transform.localScale;
                Vector3 lookDirection = useRandomRotation
                        ? Quaternion.AngleAxis((float)random.NextDouble() * 360 * i, Vector3.up) * Vector3.forward
                        : Vector3.forward;

                // 데코 데이터에 저장
                inputDecorationData.decorationInfos[i] = new DecorationInfo("Vegetation", DecorationInfo.Type.Vegetation, true);
                inputDecorationData.renderDatas[i] = new DecorationData.RenderData(randomPrefab, scale, lookDirection);
            }

            return inputDecorationData;
        }
    }
}
