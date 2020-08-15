using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class MountainSpawner : DecorationSpawner
    {
        public Vector2 spawnRange = new Vector2(0.3f, 1f);
        [Range(0, 1)]
        public float spawnRate = 0.5f;
        public GameObject[] mountainPrefabs;
        public NoiseSettings noiseSettings;

        public override DecorationData Spawn(int seed, TerrainData terrainData, DecorationData inputDecorationData)
        {
            if (terrainData.TerrainSize != inputDecorationData.mapSize)
            {
                throw new InvalidOperationException($"{nameof(terrainData)}와 {nameof(inputDecorationData)}의 지형 크기가 같지 않음");
            }

            int salt = StringHash.SDBMLower(saltString);
            System.Random random = new System.Random(seed + salt);

            for (int i = 0; i < terrainData.terrainGraph.centers.Length; i++)
            {
                // 스폰 확률 적용
                if (random.NextDouble() > spawnRate)
                {
                    continue;
                }

                Center hexagonCenter = terrainData.terrainGraph.centers[i];

                // 물에는 스폰 안함
                if (hexagonCenter.isWater)
                {
                    continue;
                }

                // 스폰 고도 필터링
                if (hexagonCenter.elevation <spawnRange.x || hexagonCenter.elevation > spawnRange.y)
                {
                    continue;
                }

                // 산맥 필터링
                int numberOfLowerNeighbors = hexagonCenter.NeighborCenters.Values.Count(x => x.elevation < hexagonCenter.elevation);
                if (numberOfLowerNeighbors < 4)
                {
                    continue;
                }

                // TODO: 생성 위치 랜덤으로 퍼트리기

                // 데코 세트 중 프리팹 하나 랜덤으로 선택
                GameObject randomMountainPrefab = mountainPrefabs[random.Next(mountainPrefabs.Length)];

                // 렌더 데이터 생성
                Vector3 scale = randomMountainPrefab.transform.localScale;
                Vector3 lookDirection = Vector3.forward;

                // 데코 데이터에 저장
                inputDecorationData.decorationInfos[i] = new DecorationInfo("Mountain", DecorationInfo.Type.Mountain, false);
                inputDecorationData.renderDatas[i] = new DecorationData.RenderData(randomMountainPrefab, scale, lookDirection);
            }

            return inputDecorationData;
        }
    }
}
