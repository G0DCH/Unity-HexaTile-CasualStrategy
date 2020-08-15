using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class ForestSpawner : DecorationSpawner
    {
        public int spawnDistance = 2;
        public float spawnRate = 0.4f;
        public DecorationPrefabData[] forestPrefabDatas;
        public bool useRandomRotation;

        public override DecorationData Spawn(int seed, TerrainData terrainData, DecorationData inputDecorationData)
        {
            if (terrainData.TerrainSize != inputDecorationData.mapSize)
            {
                throw new InvalidOperationException($"{nameof(terrainData)}와 {nameof(inputDecorationData)}의 지형 크기가 같지 않음");
            }

            int mapLength = terrainData.terrainGraph.centers.Length;

            // 산 까지의 거리 정보 맵 생성
            // Flood Fill 알고리즘 적용
            int[] distanceMap = new int[mapLength];
            Queue<Center> distanceFloodFillQueue = new Queue<Center>();
            for (int i = 0; i < distanceMap.Length; i++)
            {
                DecorationInfo? spawnedDecorationInfo = inputDecorationData.decorationInfos[i];
                if (spawnedDecorationInfo.HasValue && spawnedDecorationInfo.Value.type == DecorationInfo.Type.Mountain)
                {
                    distanceMap[i] = 0;
                    distanceFloodFillQueue.Enqueue(terrainData.terrainGraph.centers[i]);
                }
                else
                {
                    distanceMap[i] = int.MaxValue;
                }
            }

            // 산에서부터 퍼져 나가면서 거리 계산
            int mapWidth = terrainData.TerrainSize.x;
            while (distanceFloodFillQueue.Count > 0)
            {
                Center currentCenter = distanceFloodFillQueue.Dequeue();
                int currentIndex = currentCenter.hexPos.ToArrayIndex(mapWidth);
                int currentDistance = distanceMap[currentIndex];

                foreach (Center neighborCenter in currentCenter.NeighborCenters.Values)
                {
                    if (neighborCenter.isWater)
                    {
                        continue;
                    }

                    int neighborIndex = neighborCenter.hexPos.ToArrayIndex(mapWidth);
                    if (distanceMap[neighborIndex] > currentDistance + 1)
                    {
                        distanceMap[neighborIndex] = currentDistance + 1;
                        distanceFloodFillQueue.Enqueue(neighborCenter);
                    }
                }
            }

            Dictionary<int, List<GameObject>> prefabTable = new Dictionary<int, List<GameObject>>();
            foreach (var prefabData in forestPrefabDatas)
            {
                foreach (string biomeName in prefabData.spawnableBiomeNames)
                {
                    int biomeId = Biome.BiomeNameToId(biomeName);
                    if (prefabTable.TryGetValue(biomeId, out List<GameObject> biomePrefabs))
                    {
                        biomePrefabs.AddRange(prefabData.prefabs);
                    }
                    else
                    {
                        prefabTable.Add(biomeId, prefabData.prefabs.ToList());
                    }
                }
            }

            int salt = StringHash.SDBMLower(saltString);
            System.Random random = new System.Random(seed + salt);
            for (int i = 0; i < mapLength; i++)
            {
                // 스폰 확률 적용
                if (random.NextDouble() > spawnRate)
                {
                    continue;
                }

                // 스폰 가능 거리 필터링
                if (distanceMap[i] > spawnDistance || distanceMap[i] == 0)
                {
                    continue;
                }

                // 바이옴에 맞는 랜덤 프리팹 선택
                int biomeId = terrainData.terrainGraph.centers[i].biomeId;
                if (prefabTable.TryGetValue(biomeId, out List<GameObject> prefabs) == false)
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
                inputDecorationData.decorationInfos[i] = new DecorationInfo("Forest", DecorationInfo.Type.Forest, true);
                inputDecorationData.renderDatas[i] = new DecorationData.RenderData(randomPrefab, scale, lookDirection);
            }

            return inputDecorationData;
        }
    }
}
