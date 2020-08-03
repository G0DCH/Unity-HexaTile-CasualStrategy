using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public static class DecorationGenerator
    {
        public static DecorationData GenerateDecorations(int seed, TerrainData terrainData, DecorationSpawnSettings spawnSettings)
        {
            int salt = 16573549;
            seed += salt;

            DecorationData decorationData = new DecorationData(terrainData.terrainSize);

            // 산 데코
            //CalculateMountain(spawnSettings.globalSeed, spawnSettings.mountain, terrainData, ref spawnedDecorations);

            // 숲 데코

            // 나무, 꽃, 돌맹이, 기타 데코
            CalculateVegetations(seed, terrainData, spawnSettings.SpawnTableByBiomeId, decorationData);

            return decorationData;
        }

        private static void CalculateVegetations(int seed, TerrainData terrainData, 
            IReadOnlyDictionary<int, DecorationSpawnSettings.SpawnTable> spawnTableByBiome, DecorationData decorationData)
        {
            System.Random random = new System.Random(seed);

            for (int i = 0; i < terrainData.centers.Length; i++)
            {
                // 바이옴에 맞는 데코 테이블 선택
                int currentBiomeId = terrainData.centers[i].biomeId;
                if (spawnTableByBiome.TryGetValue(currentBiomeId, out var spawnTable))
                {
                    // density에 따라 다른 가중치를 부여한 랜덤으로 데코 세트 하나 선택
                    var possibleDecoSet = spawnTable.SelectRandomDecorationSetBasedOnSpawnRate(random);
                    if (possibleDecoSet.HasValue == false)
                    {
                        continue;
                    }
                    var decoSet = possibleDecoSet.Value;

                    // 데코 세트 중 프리팹 하나 랜덤으로 선택
                    var randomDecoPrefab = decoSet.decorationPrefabs[random.Next(decoSet.decorationPrefabs.Length)];

                    // 렌더 데이터 생성
                    var mesh = randomDecoPrefab.GetComponent<MeshFilter>().sharedMesh;
                    var materials = randomDecoPrefab.GetComponent<MeshRenderer>().sharedMaterials;
                    Vector3 scale = randomDecoPrefab.transform.localScale;
                    scale.Scale(decoSet.scaleVariation);
                    Vector3 lookDirection = decoSet.useRandomRotation
                        ? Quaternion.AngleAxis((float)random.NextDouble() * 360 * i, Vector3.up) * Vector3.forward
                        : Vector3.forward;

                    // 데코 데이터에 저장
                    decorationData.decorations[i] = new Decoration(decoSet.name, decoSet.type, decoSet.isDestructible);
                    decorationData.renderDatas[i] = new DecorationData.RenderData(mesh, materials, scale, lookDirection);
                }
            }
        }

        //private static void CalculateMountain(int globalSeed, DecorationSpawnSettings.NoiseBasedSpawn spawnSettings, TerrainData terrainData, ref Decoration[] spawnedDecorations)
        //{
        //    Vector2[] terrainPoints = terrainData.centers.Select(x => new Vector2(x.centerPos.x, x.centerPos.z)).ToArray();
        //    NoiseGenerator.Instance.EvaluateNoise(ref terrainPoints, out float[] noiseValues, globalSeed, spawnSettings.noiseSettings);

        //    for (int i = 0; i < spawnedDecorations.Length; i++)
        //    {
        //        Center terrainCenter = terrainData.centers[i];
        //        if (terrainCenter.isWater != false || terrainCenter.elevation < spawnSettings.spawnRange.x || terrainCenter.elevation > spawnSettings.spawnRange.y)
        //        {
        //            continue;
        //        }

        //        if (noiseValues[i] < spawnSettings.threshold)
        //        {
        //            continue;
        //        }


        //    }
        //}

        //private static void CalculateForest(Vector2 spawnRange, float forestThreshold, int globalSeed, NoiseSettings forestNoiseSettings, ref Center[] centers)
        //{
        //    Vector2[] centerPoints = centers.Select(x => new Vector2(x.centerPos.x, x.centerPos.z)).ToArray();
        //    NoiseGenerator.Instance.EvaluateNoise(ref centerPoints, out float[] noiseValues, globalSeed, forestNoiseSettings);

        //    for (int i = 0; i < centers.Length; i++)
        //    {
        //        Center center = centers[i];
        //        if (center.isWater || center.hasMountain || center.elevation < spawnRange.x || center.elevation > spawnRange.y)
        //        {
        //            continue;
        //        }

        //        if (noiseValues[i] >= forestThreshold)
        //        {
        //            center.hasForest = true;
        //        }
        //    }
        //}
    }
}
