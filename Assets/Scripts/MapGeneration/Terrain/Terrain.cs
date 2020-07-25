using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilePuzzle.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

namespace TilePuzzle.Procedural
{
    public class Terrain : MonoBehaviour
    {
        public Material hexagonMaterial;
        public Hexagon hexagonPrefab;
        public GameObject mountainPrefab;
        public GameObject forestPrefab;

        private bool isInitialized = false;
        private TerrainData terrainData;
        private Hexagon[] hexagons;

        public Vector2Int TerrainSize => terrainData.terrainSize;

        public void Initialize(TerrainData terrainData, TerrainRenderingSettings renderingSettings)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException($"지형이 이미 초기화 됨");
            }

            this.terrainData = terrainData ?? throw new ArgumentNullException(nameof(terrainData));

            Mesh flatHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size);
            Mesh cliffHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, renderingSettings.cliffDepth);
            var riverMeshCache = new Dictionary<HexagonMeshGenerator.VertexDirection, Mesh>();

            int width = terrainData.terrainSize.x;
            int height = terrainData.terrainSize.y;
            hexagons = new Hexagon[width * height];

            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(width, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(height, 2)));
            Color[] colorMap = new Color[textureWidth * textureHeight];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Center center = terrainData.centers[x + y * width];
                    Hexagon newHexagon = CreateNewHexagon(HexagonPos.FromArrayXY(x, y));
                    newHexagon.SetProperties(center);
                    hexagons[x + y * width] = newHexagon;

                    // mesh
                    HexagonMeshGenerator.VertexDirection riverDirection = 0;
                    for (int neighborIndex = 0; neighborIndex < center.NeighborCorners.Length; neighborIndex++)
                    {
                        Corner neighborCorner = center.NeighborCorners[neighborIndex];
                        if (neighborCorner.river > 0)
                        {
                            riverDirection |= (HexagonMeshGenerator.VertexDirection)(1 << neighborIndex);
                        }
                    }

                    Mesh mesh;
                    // 강이 있을때
                    if (center.isWater == false && riverDirection > 0)
                    {
                        if (riverMeshCache.TryGetValue(riverDirection, out mesh) == false)
                        {
                            mesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, renderingSettings.cliffDepth, renderingSettings.riverSize, riverDirection);
                            riverMeshCache.Add(riverDirection, mesh);
                        }
                    }
                    // 절벽일때 (주변에 물)
                    else if (center.isWater == false && center.NeighborCenters.Values.Any(neighborrCenter => neighborrCenter.isWater))
                    {
                        mesh = cliffHexagonMesh;
                    }
                    // 평지일때
                    else
                    {
                        mesh = flatHexagonMesh;
                    }
                    newHexagon.meshFilter.sharedMesh = mesh;

                    // decoration
                    // TODO: Spawn mountain, tree, etc...
                    if (center.hasMountain)
                    {
                        CreateNewDecoration(mountainPrefab, newHexagon.transform);
                    }
                    else if (center.hasForest)
                    {
                        CreateNewDecoration(forestPrefab, newHexagon.transform);
                    }

                    // height
                    Vector3 newPos = newHexagon.transform.position;
                    newPos.y = center.isWater ? -0.25f : 0;
                    newHexagon.transform.position = newPos;

                    // color
                    Color color;
                    if (center.isSea)
                    {
                        color = renderingSettings.seaColor;
                    }
                    else if (center.isWater)
                    {
                        color = renderingSettings.lakeColor;
                    }
                    else
                    {
                        color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
                    }
                    colorMap[x + y * textureWidth] = color;
                }
            }

            ApplyColorMap(renderingSettings, textureWidth, textureHeight, ref colorMap);

            isInitialized = true;
        }

        public Hexagon GetHexagonAt(HexagonPos hexPos)
        {
            Vector2Int arrayXY = hexPos.ToArrayXY();
            if (arrayXY.x < 0 || arrayXY.x >= TerrainSize.x || arrayXY.y < 0 || arrayXY.y >= TerrainSize.y)
            {
                throw new IndexOutOfRangeException($"지형 범위를 벗어남, hexPos: {hexPos}, arrayXY: {arrayXY}");
            }

            return hexagons[arrayXY.x + arrayXY.y * TerrainSize.x];
        }

        public IEnumerator<Hexagon> GetNeighborHexagons(HexagonPos hexPos, int range)
        {
            Vector2Int arrayXY = hexPos.ToArrayXY();
            if (arrayXY.x < 0 || arrayXY.x >= TerrainSize.x || arrayXY.y < 0 || arrayXY.y >= TerrainSize.y)
            {
                throw new IndexOutOfRangeException($"지형 범위를 벗어남, hexPos: {hexPos}, arrayXY: {arrayXY}");
            }

            if (range < 0)
            {
                throw new InvalidOperationException($"range는 0보다 커야함, range: {range}");
            }

            for (int hexZ = -range; hexZ <= range; hexZ++)
            {
                for (int hexX = -range; hexX <= range; hexX++)
                {
                    if (hexX == hexZ)
                    {
                        continue;
                    }

                    Vector2Int neighborXY = new HexagonPos(hexX, hexZ).ToArrayXY();
                    if (neighborXY.x < 0 || neighborXY.x >= TerrainSize.x || neighborXY.y < 0 || neighborXY.y >= TerrainSize.y)
                    {
                        continue;
                    }

                    yield return hexagons[neighborXY.x + neighborXY.y * TerrainSize.x];
                }
            }
        }

        private Hexagon CreateNewHexagon(HexagonPos hexPos)
        {
            Hexagon newHexagon = Instantiate(hexagonPrefab, transform);
            newHexagon.name = $"Hexagon {hexPos}";
            newHexagon.transform.position = hexPos.ToWorldPos();

            newHexagon.meshFilter = newHexagon.GetComponent<MeshFilter>();
            newHexagon.GetComponent<MeshRenderer>().sharedMaterial = hexagonMaterial;

            newHexagon.hexPos = hexPos;

            return newHexagon;
        }

        private GameObject CreateNewDecoration(GameObject decorationPrefab, Transform parent)
        {
            GameObject newDecoration = Instantiate(decorationPrefab, parent);
            newDecoration.name = decorationPrefab.name;
            return newDecoration;
        }

        private void ApplyColorMap(TerrainRenderingSettings renderingSettings, int textureWidth, int textureHeight, ref Color[] colorMap)
        {
            Texture2D colorMapTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            colorMapTexture.SetPixels(colorMap);
            colorMapTexture.Apply();
            hexagonMaterial.SetTexture("_ColorMap", colorMapTexture);
            hexagonMaterial.SetVector("_ColorMapSize", new Vector2(textureWidth, textureHeight));
            hexagonMaterial.SetInt("_EnableBrightNoise", renderingSettings.enableBrightNoise ? 1 : 0);
        }
    }
}
