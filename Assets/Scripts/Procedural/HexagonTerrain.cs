#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class HexagonTerrain : MonoBehaviour
    {
        public RenderOption renderOption;

        private HexagonTileObject[] tileObjects;
        private float[] fogOfWarMap;

        [Title("Object pooling settings")]
        public HexagonTileObject tilePrefab;
        public int tilePoolCapacity;
        public Transform tileHolder;
        private ObjectPool<HexagonTileObject> tileObjectPool;

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

            fogOfWarMap = null;
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

        public void SetFog(HexagonPos hexPos, bool value)
        {
            int index = hexPos.ToArrayIndex(TerrainSize.x);
            if (index < 0 || index >= fogOfWarMap.Length)
            {
                throw new IndexOutOfRangeException(nameof(hexPos));
            }
            fogOfWarMap[index] = value ? 1f : 0f;
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

            for (int y = 0; y <= TerrainSize.y; y++)
            {
                for (int x = 0; x <= TerrainSize.x; x++)
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
                        hasRiver = tileCenter.NeighborCorners.Any(x => x.river > 0),
                        biome = terrainData.biomeTable.biomeDictionary[tileCenter.biomeId]
                    };

                    // 타일의 종류에 따라 다른 메시와 메티리얼을 선택
                    Mesh selectedMesh;
                    Material selectedMaterial;

                    // 물 타일의 경우
                    if (newTileObject.TileInfo.isWater)
                    {
                        selectedMesh = flatTileMesh;
                        selectedMaterial = newTileObject.TileInfo.isSea ? renderOption.seaMaterial : renderOption.lakeMaterial;
                    }
                    // 육지 타일의 경우
                    else
                    {
                        // 주변에 강이 있을 경우
                        if (newTileObject.TileInfo.hasRiver)
                        {
                            // 강이 흐르는 방향을 계산
                            HexagonMeshGenerator.VertexDirection riverDirection = 0;
                            for (int neighborIndex = 0; neighborIndex < tileCenter.NeighborCorners.Length; neighborIndex++)
                            {
                                Corner neighborCorner = tileCenter.NeighborCorners[neighborIndex];
                                if (neighborCorner.river > 0)
                                {
                                    riverDirection |= (HexagonMeshGenerator.VertexDirection)(0x1 << neighborIndex);
                                }
                            }

                            // 캐시된 메시가 없으면 새로 생성
                            if (riverTileMeshCache.TryGetValue(riverDirection, out selectedMesh) == false)
                            {
                                selectedMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize, renderOption.cliffDepth,
                                    renderOption.riverSize, riverDirection);
                                riverTileMeshCache.Add(riverDirection, selectedMesh);
                            }
                        }
                        // 주변 타일과 고도차이가 있는 경우 (주변 타일이 물)
                        else if (tileCenter.NeighborCenters.Values.Any(x => x.isWater))
                        {
                            selectedMesh = cliffTileMesh;
                        }
                        // 일반적인 육지 타일의 경우
                        else
                        {
                            selectedMesh = flatTileMesh;
                        }

                        selectedMaterial = renderOption.landMaterial;
                    }

                    newTileObject.TileMesh = selectedMesh;
                    newTileObject.TileMaterial = selectedMaterial;

                    // 타일의 높이를 설정
                    Vector3 newPos = newTileObject.transform.position;
                    newPos.y = newTileObject.TileInfo.isWater ? -renderOption.waterDepth : 0;
                    newTileObject.transform.position = newPos;

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
            Color[] tileColors = new Color[tileObjects.Length];
            for (int i = 0; i < tileObjects.Length; i++)
            {
                tileColors[i] = tileObjects[i].TileInfo.biome.color;
            }

            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(TerrainSize.y, 2)));
            Texture2D terrainColorTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            terrainColorTexture.SetPixels(tileColors);
            terrainColorTexture.Apply();

            renderOption.landMaterial.SetTexture("_ColorMap", terrainColorTexture);
        }

        public struct RenderOption
        {
            [Title("Land")]
            [Required]
            public Material landMaterial;
            [Min(0.1f)] public float cliffDepth;
            public bool enableBrightNoise;

            [Title("Water")]
            [Required]
            public Material seaMaterial;
            [Required]
            public Material lakeMaterial;
            [PropertyRange(0, nameof(cliffDepth))]
            public float waterDepth;

            [Title("River")]
            [PropertyRange(0.05f, 1f)]
            public float riverSize;

            public RenderOption Default => new RenderOption
            {
                cliffDepth = 1.5f,
                enableBrightNoise = true,

                waterDepth = 0.5f,

                riverSize = 0.2f,
            };
        }
    }
}
