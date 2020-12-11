#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class HexagonTerrain : MonoBehaviour
    {
        public RenderOption renderOption = RenderOption.Default;

        private HexagonTileObject[] tileObjects;
        private bool isFogOfWarChanged = false;

        [Title("Object pooling settings")]
        [Required]
        public HexagonTileObject tilePrefab;
        [Min(100)]
        public int tilePoolCapacity = 500;
        public Transform tileHolder;
        private ObjectPool<HexagonTileObject> tileObjectPool;

        [Title("Debug settings")]
        public bool drawRiver = false;
        private Corner[] corners;

        public Vector2Int TerrainSize { get; private set; }

        #region Unity 메시지
        protected virtual void Awake()
        {
            if (tileHolder == null)
            {
                tileHolder = new GameObject("Tile holder").transform;
            }
            tileObjectPool = new ObjectPool<HexagonTileObject>(tilePrefab, tilePoolCapacity, tileHolder);
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            if (isFogOfWarChanged)
            {
                UpdateFogOfWars();
            }

            if (Input.GetKey(KeyCode.Space))
            {
                var randomTile = tileObjects[UnityEngine.Random.Range(0, tileObjects.Length)];
                SetTileVisibility(randomTile.TileInfo.hexPos, false);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (drawRiver)
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    if (corners[i].river > 0 && corners[i].downslope.river > 0)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(corners[i].cornerPos, corners[i].downslope.cornerPos);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 헥사곤 지형과 데코레이션을 생성, 이전에 생성된 지형이 있으면 제거
        /// </summary>
        public void BuildTerrain(TerrainData terrainData, DecorationData decorationData)
        {
            if (terrainData is null)
            {
                throw new ArgumentNullException(nameof(terrainData));
            }

            if (decorationData is null)
            {
                throw new ArgumentNullException(nameof(decorationData));
            }

            if (terrainData.TerrainSize != decorationData.mapSize)
            {
                throw new InvalidOperationException($"terrainData와 decorationData의 맵 크기가 다름, {terrainData.TerrainSize}, {decorationData.mapSize}");
            }

            ClearTerrain();
            TerrainSize = terrainData.TerrainSize;
            tileObjects = SpawnTerrainTiles(terrainData, decorationData);
            UpdateTileColors();
            UpdateFogOfWars();
            UpdateTileHeights(terrainData);
            UpdateWaterMap(terrainData);

            // Debug
            corners = terrainData.terrainGraph.corners;
        }

        /// <summary>
        /// 생성된 지형을 지움
        /// </summary>
        public void ClearTerrain()
        {
            TerrainSize = Vector2Int.zero;
            if (tileObjects != null)
            {
                foreach (HexagonTileObject tileObject in tileObjects)
                {
                    tileObjectPool.Push(tileObject);
                }
            }
            tileObjects = null;
        }

        public HexagonTileObject GetHexagonTile(HexagonPos hexPos)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= tileObjects.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }
            return tileObjects[index];
        }

        [Obsolete("Use GetHexagonTile instead")]
        public TileInfo GetTerrainInfo(HexagonPos hexPos)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= tileObjects.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }
            return tileObjects[index].TileInfo;
        }

        [Obsolete("Use GetHexagonTile instead")]
        public DecorationInfo? GetDecorationInfo(HexagonPos hexPos)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= tileObjects.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }

            if (tileObjects[index].DecorationInfo != null)
            {
                return tileObjects[index].DecorationInfo;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 범위 내에 있는 모든 <see cref="HexagonTileObject"/>들을 반환
        /// </summary>
        /// <param name="centerHexPos">중심 위치</param>
        /// <param name="range">탐색의 시작 ~ 끝 범위</param>
        public IEnumerable<HexagonTileObject> GetHexagonTiles(HexagonPos centerHexPos, RangeInt range)
        {
            foreach (int index in CalculateIndexesInRange(centerHexPos, range))
            {
                yield return tileObjects[index];
            }
        }

        /// <summary>
        /// 범위 내에 있는 모든 <see cref="TileInfo"/>들을 반환
        /// </summary>
        /// <param name="centerHexPos">중심 위치</param>
        /// <param name="range">탐색의 시작 ~ 끝 범위</param>
        [Obsolete("Use GetHexagonTiles instead")]
        public IEnumerable<TileInfo> GetTerrainInfos(HexagonPos centerHexPos, RangeInt range)
        {
            foreach (int index in CalculateIndexesInRange(centerHexPos, range))
            {
                yield return tileObjects[index].TileInfo;
            }
        }

        /// <summary>
        /// 범위 내에 있는 모든 <see cref="DecorationInfo"/>들을 반환
        /// </summary>
        /// <param name="centerHexPos">중심 위치</param>
        /// <param name="range">탐색의 시작 ~ 끝 범위</param>
        [Obsolete("Use GetHexagonTiles instead")]
        public IEnumerable<DecorationInfo?> GetDecorationInfos(HexagonPos centerHexPos, RangeInt range)
        {
            foreach (int index in CalculateIndexesInRange(centerHexPos, range))
            {
                if (tileObjects[index].DecorationInfo != null)
                {
                    yield return tileObjects[index].DecorationInfo;
                }
                else
                {
                    yield return null;
                }
            }
        }

        public void SetTileVisibility(HexagonPos hexPos, bool isVisible)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= tileObjects.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }
            tileObjects[index].IsVisible = isVisible;
            isFogOfWarChanged = true;
        }

        public void DestroyDecoration(HexagonPos hexPos)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= tileObjects.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }
            tileObjects[index].DestroyDecoration();
        }

        /// <summary>
        /// 인덱스가 array의 범위를 벗어나지 않음을 보장
        /// </summary>
        /// <param name="centerHexPos"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        private IEnumerable<int> CalculateIndexesInRange(HexagonPos centerHexPos, RangeInt range)
        {
            if (range.start < 0 || range.length < 0)
            {
                throw new InvalidOperationException($"잘못된 범위, range: {range}");
            }

            for (int hexZ = -range.end; hexZ <= range.end; hexZ++)
            {
                for (int hexX = -range.end; hexX <= range.end; hexX++)
                {
                    HexagonPos neighborHexOffset = new HexagonPos(hexX, hexZ);
                    int offsetDistance = neighborHexOffset.HexagonDistance;
                    if (offsetDistance < range.start || offsetDistance > range.end)
                    {
                        continue;
                    }

                    Vector2Int neighborXY = (centerHexPos + neighborHexOffset).ToArrayXY();
                    if (neighborXY.x < 0 || neighborXY.x >= TerrainSize.x || neighborXY.y < 0 || neighborXY.y >= TerrainSize.y)
                    {
                        continue;
                    }

                    int neighborIndex = neighborXY.x + neighborXY.y * TerrainSize.x;
                    yield return neighborIndex;
                }
            }
        }

        private HexagonTileObject[] SpawnTerrainTiles(TerrainData terrainData, DecorationData decorationData)
        {
            var spawnedTileObjects = new HexagonTileObject[TerrainSize.x * TerrainSize.y];

            // 재사용할 메시를 캐싱
            Mesh flatTileMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize);
            Mesh cliffTileMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize, renderOption.cliffDepth);
            var riverTileMeshCache = new Dictionary<HexagonMeshGenerator.VertexDirection, Mesh>();

            for (int y = 0; y < TerrainSize.y; y++)
            {
                for (int x = 0; x < TerrainSize.x; x++)
                {
                    int tileIndex = x + y * TerrainSize.x;
                    Center tileCenter = terrainData.terrainGraph.centers[tileIndex];
                    HexagonPos tileHexPos = HexagonPos.FromArrayXY(x, y);

                    // 새 타일 오브젝트 생성
                    HexagonTileObject newTileObject = tileObjectPool.Pop();
                    newTileObject.transform.position = tileHexPos.ToWorldPos(0);
                    newTileObject.name = $"Hexagon tile ({tileHexPos})";
                    spawnedTileObjects[tileIndex] = newTileObject;

                    // TileInfo 설정
                    newTileObject.TileInfo = new TileInfo
                    {
                        hexPos = tileHexPos,
                        isWater = tileCenter.isWater,
                        isSea = tileCenter.isSea,
                        isCoast = tileCenter.isCoast,
                        hasRiver = tileCenter.NeighborCorners.Any(corner => corner.river > 0),
                        biome = terrainData.biomeTable.biomeDictionary[tileCenter.biomeId]
                    };

                    // 타일의 종류에 따라 다른 메시와 메티리얼을 선택

                    // 물 타일의 경우
                    if (newTileObject.TileInfo.isWater)
                    {
                        newTileObject.water.Mesh = flatTileMesh;
                        newTileObject.water.Material = renderOption.waterMaterial;
                    }
                    // 육지 타일의 경우
                    else
                    {
                        Mesh selectedLandMesh;

                        // 주변에 강이 있을 경우
                        if (newTileObject.TileInfo.hasRiver)
                        {
                            // 강이 흐르는 방향을 계산
                            var rivers = new HexagonMeshGenerator.VertexDirection[6];
                            for (int neighborIndex = 0; neighborIndex < tileCenter.NeighborCorners.Length; neighborIndex++)
                            {
                                Corner neighborCorner = tileCenter.NeighborCorners[neighborIndex];
                                if (neighborCorner.river > 0)
                                {
                                    rivers[neighborIndex] |= (HexagonMeshGenerator.VertexDirection)(1 << neighborIndex);

                                    if (neighborCorner.downslope.river > 0)
                                    {
                                        int rightNeighborIndex = Modulo(neighborIndex + 1, tileCenter.NeighborCorners.Length);
                                        int leftNeighborIndex = Modulo(neighborIndex - 1, tileCenter.NeighborCorners.Length);
                                        if (neighborCorner.downslope == tileCenter.NeighborCorners[rightNeighborIndex])
                                        {
                                            rivers[neighborIndex] |= (HexagonMeshGenerator.VertexDirection)(1 << rightNeighborIndex);
                                        }
                                        else if (neighborCorner.downslope == tileCenter.NeighborCorners[leftNeighborIndex])
                                        {
                                            rivers[neighborIndex] |= (HexagonMeshGenerator.VertexDirection)(1 << leftNeighborIndex);
                                        }
                                    }
                                }
                            }

                            // 캐시된 메시가 없으면 새로 생성
                            selectedLandMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize, renderOption.cliffDepth,
                                    renderOption.riverSize, rivers.ToArray());
                            //if (riverTileMeshCache.TryGetValue(riverDirection, out selectedMesh) == false)
                            //{
                            //    riverTileMeshCache.Add(riverDirection, selectedMesh);
                            //}

                            newTileObject.water.Mesh = cliffTileMesh;
                            newTileObject.water.Material = renderOption.waterMaterial;
                        }
                        // 주변 타일과 고도차이가 있는 경우 (주변 타일이 물)
                        //else if (tileCenter.NeighborCenters.Values.Any(corner => corner.isWater))
                        //{
                        //    selectedLandMesh = cliffTileMesh;
                        //}
                        // 일반적인 육지 타일의 경우
                        else
                        {
                            selectedLandMesh = cliffTileMesh;
                        } 

                        newTileObject.land.Mesh = selectedLandMesh;
                        newTileObject.land.Material = renderOption.landMaterial;
                    }

                    // 타일의 전장의 안개 설정
                    newTileObject.IsVisible = true;

                    // 타일에 데코레이션이 있으면 데코레이션 생성
                    if (decorationData.decorationInfos[tileIndex].HasValue)
                    {
                        // 데코레이션 오브젝트 생성
                        DecorationData.RenderData renderData = decorationData.renderDatas[tileIndex].Value;
                        GameObject newDecorationObject = SpawnDecorationObject(renderData);

                        // 타일에 데코레이션 할당
                        DecorationInfo decorationInfo = decorationData.decorationInfos[tileIndex].Value;
                        newTileObject.SetDecoration(newDecorationObject, decorationInfo);
                    }
                }
            }

            return spawnedTileObjects;
        }

        private GameObject SpawnDecorationObject(DecorationData.RenderData decorationRenderData)
        {
            GameObject decorationObject = Instantiate(decorationRenderData.prefab);
            decorationObject.transform.LookAt(decorationObject.transform.position + decorationRenderData.lookDirection);
            decorationObject.transform.localScale = decorationRenderData.scale;
            return decorationObject;
        }

        private void UpdateTileColors()
        {
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.y, 2)));

            Color[] tileColors = new Color[textureWidth * textureHeight];
            for (int y = 0; y < TerrainSize.y; y++)
            {
                for (int x = 0; x < TerrainSize.x; x++)
                {
                    int textureIndex = x + y * textureWidth;
                    int tileIndex = x + y * TerrainSize.x;
                    tileColors[textureIndex] = tileObjects[tileIndex].TileInfo.biome.color;
                }
            }

            Texture2D terrainColorTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            terrainColorTexture.SetPixels(tileColors);
            terrainColorTexture.Apply();

            renderOption.landMaterial.SetVector("_MapSize", new Vector2(textureWidth, textureHeight));
            renderOption.landMaterial.SetTexture("_ColorMap", terrainColorTexture);
        }

        private void UpdateFogOfWars()
        {
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.y, 2)));

            Color[] fogMask = new Color[textureWidth * textureHeight];
            for (int y = 0; y < TerrainSize.y; y++)
            {
                for (int x = 0; x < TerrainSize.x; x++)
                {
                    int textureIndex = x + y * textureWidth;
                    int tileIndex = x + y * TerrainSize.x;
                    fogMask[textureIndex] = new Color(tileObjects[tileIndex].IsVisible ? 1f : 0f, 0f, 0f, 0f);
                }
            }

            Texture2D forMaskTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            forMaskTexture.SetPixels(fogMask);
            forMaskTexture.Apply();

            renderOption.landMaterial.SetVector("_MapSize", new Vector2(textureWidth, textureHeight));
            renderOption.landMaterial.SetTexture("_FogMask", forMaskTexture);
        }

        private void UpdateTileHeights(TerrainData terrainData)
        {
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.y, 2)));
            float stepSize = 1f / renderOption.numberOfStep;
            Color[] heightMapColors = new Color[textureWidth * textureHeight];

            for (int y = 0; y < TerrainSize.y; y++)
            {
                for (int x = 0; x < TerrainSize.x; x++)
                {
                    int tileIndex = x + y * TerrainSize.x;
                    int textureIndex = x + y * textureWidth;

                    float level = Mathf.FloorToInt(terrainData.terrainGraph.centers[tileIndex].elevation / stepSize);
                    float normalizedLevel = Mathf.InverseLerp(0, renderOption.numberOfStep - 1, level);
                    float quantinzedHeight = Mathf.Lerp(renderOption.heightRange.x, renderOption.heightRange.y, normalizedLevel);

                    tileObjects[tileIndex].land.transform.localPosition = new Vector3(0, quantinzedHeight, 0);

                    heightMapColors[textureIndex] = Color.Lerp(Color.black, Color.white, quantinzedHeight);
                }
            }

            Texture2D heightMapTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            heightMapTexture.SetPixels(heightMapColors);
            heightMapTexture.Apply();

            renderOption.waterMaterial.SetVector("_MapSize", new Vector2(textureWidth, textureHeight));
            renderOption.waterMaterial.SetTexture("_HeightMap", heightMapTexture);
        }

        private void UpdateWaterMap(TerrainData terrainData)
        {
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.y, 2)));
            Color[] waterMapColors = new Color[textureWidth * textureHeight];

            var floodFillQueue = new Queue<Center>();
            foreach (Center center in terrainData.terrainGraph.centers)
            {
                if (center.isWater == false && center.NeighborCenters.Values.Any(x => x.isWater))
                {
                    floodFillQueue.Enqueue(center);
                }
            }

            var distanceMap = new int[TerrainSize.x * TerrainSize.y];
            while (floodFillQueue.Count > 0)
            {
                Center currentCenter = floodFillQueue.Dequeue();
                int currentIndex = currentCenter.hexPos.ToArrayIndex(TerrainSize.x);

                foreach (Center neighborCenter in currentCenter.NeighborCenters.Values.Where(x => x.isWater))
                {
                    int neighborIndex = neighborCenter.hexPos.ToArrayIndex(TerrainSize.x);
                    if (distanceMap[neighborIndex] == 0)
                    {
                        distanceMap[neighborIndex] = distanceMap[currentIndex] + 1;
                        floodFillQueue.Enqueue(neighborCenter);
                    }
                }
            }

            for (int y = 0; y < TerrainSize.y; y++)
            {
                for (int x = 0; x < TerrainSize.x; x++)
                {
                    int tileIndex = x + y * TerrainSize.x;
                    int textureIndex = x + y * textureWidth;

                    TileInfo tileInfo = tileObjects[tileIndex].TileInfo;
                    waterMapColors[textureIndex] = new Color(
                        tileInfo.isWater == false && tileInfo.hasRiver ? 1f : 0f,
                        tileInfo.IsLake ? 1f : 0f,
                        tileInfo.isSea ? 1f : 0f,
                        distanceMap[tileIndex] / 20f);
                }
            }

            Texture2D waterMapTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            waterMapTexture.SetPixels(waterMapColors);
            waterMapTexture.Apply();

            renderOption.waterMaterial.SetVector("_MapSize", new Vector2(textureWidth, textureHeight));
            renderOption.waterMaterial.SetTexture("_WaterMap", waterMapTexture);
        }

        private static int Modulo(int x, int m)
        {
            int remainder = x % m;
            return remainder < 0 ? remainder + m : remainder;
        }

        [Serializable]
        public struct RenderOption
        {
            [Title("Land")]
            [Required]
            public Material landMaterial;
            [Min(0.1f)] public float cliffDepth;
            public bool enableBrightNoise;

            [Title("Water")]
            [Required]
            public Material waterMaterial;

            [Title("River")]
            [PropertyRange(0.05f, 1f)]
            public float riverSize;

            [Title("Height")]
            public bool enableHeight;
            [Range(0, 20)]
            public int numberOfStep;
            [MinMaxSlider(0, 2, true)]
            public Vector2 heightRange;

            public static RenderOption Default => new RenderOption
            {
                cliffDepth = 1.5f,
                enableBrightNoise = true,

                riverSize = 0.2f,

                enableHeight = true,
                numberOfStep = 4,
                heightRange = new Vector2(0, 1),
            };
        }
    }
}
