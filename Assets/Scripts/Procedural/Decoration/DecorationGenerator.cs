using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public static class DecorationGenerator
    {
        /// <summary>
        /// <paramref name="terrainData"/>와 <paramref name="spawnSettings"/>을 기반으로 <see cref="DecorationData"/> 생성
        /// </summary>
        /// <param name="seed">생성 시드</param>
        /// <param name="terrainData">데코레이션을 생성하게 될 지형의 데이터</param>
        /// <param name="spawnSettings">데코레이션 생성 설정</param>
        public static DecorationData GenerateDecorationData(int seed, TerrainData terrainData, DecorationSpawnSettings spawnSettings)
        {
            int salt = 483268917;
            seed += salt;

            DecorationData decorationData = new DecorationData(terrainData.terrainSize);

            // 산 생성
            CalculateMountain(seed, spawnSettings.mountainSpawnSetting, terrainData, decorationData);

            // 숲 생성
            CalculateForest(seed, spawnSettings.forestSpawnSetting, terrainData, decorationData);

            // 나무, 꽃, 돌맹이, 기타 데코
            CalculateVegetation(seed, spawnSettings.SpawnTableByBiomeId, terrainData, decorationData);

            return decorationData;
        }

        private static void CalculateMountain(int seed, DecorationSpawnSettings.MountainSpawnSetting spawnSetting,
            TerrainData terrainData, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < terrainData.centers.Length; i++)
            {
                Center hexCenter = terrainData.centers[i];

                if (hexCenter.isWater)
                {
                    continue;
                }

                // 생성 고도 필터링
                if (hexCenter.elevation < spawnSetting.spawnRange.x || hexCenter.elevation > spawnSetting.spawnRange.y)
                {
                    continue;
                }

                // 산맥 선택
                if (hexCenter.NeighborCenters.Values.Count(neighbor => neighbor.elevation < hexCenter.elevation) < 4)
                {
                    continue;
                }

                // 스폰률 적용
                if (random.NextDouble() > spawnSetting.spawnRate)
                {
                    continue;
                }

                // TODO: 위치 섞기
                int x = i % terrainData.terrainSize.x;
                int y = i / terrainData.terrainSize.y;

                // 데코 세트 중 프리팹 하나 랜덤으로 선택
                var randomDecoPrefab = spawnSetting.decorationPrefabs[random.Next(spawnSetting.decorationPrefabs.Length)];

                // 렌더 데이터 생성
                Vector3 scale = randomDecoPrefab.transform.localScale;
                Vector3 lookDirection = Vector3.forward;

                // 데코 데이터에 저장
                decorationData.decorationInfos[i] = new Decoration("Mountain", Decoration.Type.Mountain, false);
                decorationData.renderDatas[i] = new DecorationData.RenderData(randomDecoPrefab, scale, lookDirection);
            }
        }

        private static void CalculateForest(int seed, DecorationSpawnSettings.ForestSpawnSetting spawnSetting,
            TerrainData terrainData, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            // 산 까지의 거리 구하기
            int[] distanceMap = new int[terrainData.centers.Length];
            Queue<Center> floodFill = new Queue<Center>();
            for (int i = 0; i < distanceMap.Length; i++)
            {
                if (decorationData.decorationInfos[i].HasValue && decorationData.decorationInfos[i].Value.type == Decoration.Type.Mountain)
                {
                    distanceMap[i] = 0;
                    floodFill.Enqueue(terrainData.centers[i]);
                }
                else
                {
                    distanceMap[i] = int.MaxValue;
                }
            }

            while (floodFill.Count > 0)
            {
                Center center = floodFill.Dequeue();
                Vector2Int xy = center.hexPos.ToArrayXY();
                int index = xy.x + xy.y * terrainData.terrainSize.x;
                int distance = distanceMap[index];

                foreach (Center neighbor in center.NeighborCenters.Values)
                {
                    if (neighbor.isWater)
                    {
                        continue;
                    }

                    Vector2Int neighborXY = neighbor.hexPos.ToArrayXY();
                    int neighborIndex = neighborXY.x + neighborXY.y * terrainData.terrainSize.x;

                    int newDistance = distance + 1;
                    if (newDistance < distanceMap[neighborIndex])
                    {
                        distanceMap[neighborIndex] = newDistance;
                        floodFill.Enqueue(neighbor);
                    }
                }
            }

            for (int i = 0; i < distanceMap.Length; i++)
            {
                // 거리 필터링
                if (distanceMap[i] ==  0 || distanceMap[i] > spawnSetting.spawnDistance)
                {
                    continue;
                }

                // 스폰률 적용
                if (random.NextDouble() > spawnSetting.spawnRate)
                {
                    continue;
                }

                // 바이옴에 맞는 숲 데코 선택
                if (spawnSetting.BiomeDecorationTable.TryGetValue(terrainData.centers[i].biomeId, out GameObject[] decorationPrefabs) == false)
                {
                    continue;
                }

                // 데코 세트 중 프리팹 하나 랜덤으로 선택
                var randomDecoPrefab = decorationPrefabs[random.Next(decorationPrefabs.Length)];

                // 렌더 데이터 생성
                Vector3 scale = randomDecoPrefab.transform.localScale;
                Vector3 lookDirection = spawnSetting.useRandomRotation
                        ? Quaternion.AngleAxis((float)random.NextDouble() * 360 * i, Vector3.up) * Vector3.forward
                        : Vector3.forward;

                // 데코 데이터에 저장
                decorationData.decorationInfos[i] = new Decoration("Forest", Decoration.Type.Mountain, false);
                decorationData.renderDatas[i] = new DecorationData.RenderData(randomDecoPrefab, scale, lookDirection);
            }
        }

        private static void CalculateForest2(int seed, DecorationSpawnSettings.ForestSpawnSetting spawnSetting,
            TerrainData terrainData, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < spawnSetting.tryCount; i++)
            {
                int index = random.Next(terrainData.centers.Length);
                Center center = terrainData.centers[index];

                if (center.isWater || center.isCoast)
                {
                    continue;
                }

                if (center.moisture < random.NextDouble())
                {
                    continue;
                }

                if (spawnSetting.BiomeDecorationTable.TryGetValue(center.biomeId, out GameObject[] decorationPrefabs) == false)
                {
                    continue;
                }

                int distance = spawnSetting.spawnDistance;
                for (int hexX = -distance; hexX <= distance; hexX++)
                {
                    for (int hexZ = -distance; hexZ <= distance; hexZ++)
                    {
                        HexagonPos hexOffset = new HexagonPos(hexX, hexZ);
                        if (hexOffset.HexagonDistance > distance)
                        {
                            continue;
                        }

                        Vector2Int neighborXY = (center.hexPos + hexOffset).ToArrayXY();
                        int neighborIndex = neighborXY.x + neighborXY.y * terrainData.terrainSize.x;
                        if (terrainData.centers[neighborIndex].isWater || terrainData.centers[neighborIndex].isCoast)
                        {
                            continue;
                        }

                        if (terrainData.centers[neighborIndex].biomeId != center.biomeId)
                        {
                            //continue;
                        }

                        if (decorationData.decorationInfos[neighborIndex].HasValue)
                        {
                            continue;
                        }

                        if (random.NextDouble() > Mathf.Pow(spawnSetting.spawnRate, Mathf.Max(hexOffset.HexagonDistance, 0)))
                        {
                            continue;
                        }

                        // 데코 세트 중 프리팹 하나 랜덤으로 선택
                        var randomDecoPrefab = decorationPrefabs[random.Next(decorationPrefabs.Length)];

                        // 렌더 데이터 생성
                        Vector3 scale = randomDecoPrefab.transform.localScale;
                        Vector3 lookDirection = Vector3.forward;

                        // 데코 데이터에 저장
                        decorationData.decorationInfos[neighborIndex] = new Decoration("Forest", Decoration.Type.Mountain, false);
                        decorationData.renderDatas[neighborIndex] = new DecorationData.RenderData(randomDecoPrefab, scale, lookDirection);
                    }
                }
            }
        }

        private static void CalculateForest3(int seed, DecorationSpawnSettings.ForestSpawnSetting spawnSetting,
            TerrainData terrainData, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < terrainData.centers.Length; i++)
            {
                if (terrainData.centers[i].isWater || terrainData.centers[i].isCoast)
                {
                    continue;
                }

                float moisture = terrainData.centers[i].moisture;
                if (random.NextDouble() > moisture / 2f)
                {
                    continue;
                }

                if (spawnSetting.BiomeDecorationTable.TryGetValue(terrainData.centers[i].biomeId, out GameObject[] decorationPrefabs) == false)
                {
                    continue;
                }

                // 데코 세트 중 프리팹 하나 랜덤으로 선택
                var randomDecoPrefab = decorationPrefabs[random.Next(decorationPrefabs.Length)];

                // 렌더 데이터 생성
                Vector3 scale = randomDecoPrefab.transform.localScale;
                Vector3 lookDirection = spawnSetting.useRandomRotation
                        ? Quaternion.AngleAxis((float)random.NextDouble() * 360 * i, Vector3.up) * Vector3.forward
                        : Vector3.forward;

                // 데코 데이터에 저장
                decorationData.decorationInfos[i] = new Decoration("Forest", Decoration.Type.Mountain, false);
                decorationData.renderDatas[i] = new DecorationData.RenderData(randomDecoPrefab, scale, lookDirection);
            }
        }

        private static void CalculateVegetation(int seed, IReadOnlyDictionary<int, DecorationSpawnSettings.SpawnTable> spawnTableByBiome,
            TerrainData terrainData, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < terrainData.centers.Length; i++)
            {
                // 물, 해안에는 생성 안함
                if (terrainData.centers[i].isWater || terrainData.centers[i].isCoast)
                {
                    continue;
                }

                // 이미 다른 데코가 있는지 검사
                if (decorationData.decorationInfos[i].HasValue
                    && (decorationData.decorationInfos[i].Value.type == Decoration.Type.Mountain || decorationData.decorationInfos[i].Value.type == Decoration.Type.Forest))
                {
                    continue;
                }

                // 바이옴에 맞는 데코 테이블 선택
                int currentBiomeId = terrainData.centers[i].biomeId;
                if (spawnTableByBiome.TryGetValue(currentBiomeId, out var spawnTable))
                {
                    // 스폰확률 따라 다른 가중치를 부여한 랜덤으로 데코 세트 하나 선택
                    var possibleDecoSet = spawnTable.SelectRandomDecorationSetBasedOnSpawnRate(random);
                    if (possibleDecoSet.HasValue == false)
                    {
                        continue;
                    }
                    var decoSet = possibleDecoSet.Value;

                    // 데코 세트 중 프리팹 하나 랜덤으로 선택
                    var randomDecoPrefab = decoSet.decorationPrefabs[random.Next(decoSet.decorationPrefabs.Length)];

                    // 렌더 데이터 생성
                    Vector3 scale = randomDecoPrefab.transform.localScale;
                    Vector3 lookDirection = decoSet.useRandomRotation
                        ? Quaternion.AngleAxis((float)random.NextDouble() * 360 * i, Vector3.up) * Vector3.forward
                        : Vector3.forward;

                    // 데코 데이터에 저장
                    decorationData.decorationInfos[i] = new Decoration(decoSet.name, decoSet.type, decoSet.isDestructible);
                    decorationData.renderDatas[i] = new DecorationData.RenderData(randomDecoPrefab, scale, lookDirection);
                }
            }
        }
    }
}
