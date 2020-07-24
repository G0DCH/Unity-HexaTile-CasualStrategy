using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilePuzzle.Rendering;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [ExecuteInEditMode]
    public class TerrainPreview : MonoBehaviour
    {
        // 지형 생성 설정 값
        [Title("Generate Settings")]
        public Vector2Int terrainSize = new Vector2Int(30, 30);
        public NoiseSettings terrainShapeNoiseSettings;
        public FalloffSettings terrainShapeFalloffSettings;
        // falloff settings
        public float landRatio = 0.6f;
        public float lakeThreshold = 0.3f;
        public float peakMultiplier = 1.1f;
        public int riverSeed;
        public Vector2 riverSpawnRange = new Vector2(0.3f, 0.9f);
        public float riverSpawnMultiplier = 1f;
        [Required]
        public BiomeTableSettings biomeTableSettings;

        [Title("Preview Settings")]
        public bool autoUpdatePreview;
        [EnumToggleButtons]
        public PreviewMode previewMode;
        [Required]
        public Hexagon hexagonPrefab;
        [Required]
        public Material hexagonMaterial;
        [Required]
        public Transform hexagonHolder;

        private bool settingUpdated;
        private TerrainGenerateSettings generateSettings;
        private Hexagon[] hexagons;
        private Vector2Int previousHexagonMapSize;

        public enum PreviewMode
        {
            Water, Height, Moisture, Temperature, Biome
        }

        // 프리뷰 모드 (높이맵, 습도맵 보기 등)

        private void Update()
        {
            // 설정값이 변경되면 미리보기 지형 업데이트
            if (settingUpdated)
            {
                settingUpdated = false;

                UpdateTerrainGenerateSettings();
                UpdateTerrainPreview();
            }
        }

        private void OnValidate()
        {
            if (autoUpdatePreview)
            {
                settingUpdated = true;
            }
        }

        public void SaveGenerateSettings()
        {
            // TODO: 지형 생성 설정값 저장
        }

        private void UpdateTerrainGenerateSettings()
        {
            if (generateSettings == null)
            {
                generateSettings = ScriptableObject.CreateInstance<TerrainGenerateSettings>();
            }

            generateSettings.terrainSize = terrainSize;
            generateSettings.terrainShapeNoiseSettings = terrainShapeNoiseSettings;
            generateSettings.terrainShapeFalloffSettings = terrainShapeFalloffSettings;
            generateSettings.landRatio = landRatio;
            generateSettings.lakeThreshold = lakeThreshold;
            generateSettings.peakMultiplier = peakMultiplier;
            generateSettings.riverSeed = riverSeed;
            generateSettings.riverSpawnRange = riverSpawnRange;
            generateSettings.riverSpawnMultiplier = riverSpawnMultiplier;
            generateSettings.biomeTableSettings = biomeTableSettings;
    }

        [Button]
        private void UpdateTerrainPreview()
        {
            TerrainData terrainData = TerrainGenerator.GenerateTerrain(generateSettings);
            CreateHexagonMap(generateSettings.terrainSize);
            UpdateHexagonMeshes(terrainData);
            UpdateHexagonHeight(terrainData);
            UpdateHexagonColorMap(terrainData);
        }

        private void CreateHexagonMap(Vector2Int mapSize)
        {
            if (hexagons != null && previousHexagonMapSize == mapSize)
            {
                return;
            }
            previousHexagonMapSize = mapSize;

            DestroyAllHexagons();

            int width = mapSize.x;
            int height = mapSize.y;
            hexagons = new Hexagon[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    hexagons[x + y * width] = CreateNewHexagon(HexagonPos.FromArrayXY(x, y));
                }
            }
        }

        private void UpdateHexagonMeshes(TerrainData terrainData)
        {
            Mesh planeHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size);
            Mesh cliffHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, 1);

            for (int i = 0; i < hexagons.Length; i++)
            {
                Center center = terrainData.centers[i];
                HexagonMeshGenerator.VertexDirection riverDirection = 0;
                for (int cornerIndex = 0; cornerIndex < center.NeighborCorners.Length; cornerIndex++)
                {
                    Corner neighborCorner = center.NeighborCorners[cornerIndex];
                    if (neighborCorner.river > 0)
                    {
                        riverDirection |= (HexagonMeshGenerator.VertexDirection)(1 << cornerIndex);
                    }
                }

                Mesh hexagonMesh;
                //if (center.isSea)
                //{
                //    hexagonMesh = null;
                //}
                if (riverDirection > 0 && center.isWater == false)
                {
                    hexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, 1, 0.3f, riverDirection);
                }
                else if (center.isWater == false && center.NeighborCenters.Values.Any(neighborCenter => neighborCenter.isWater))
                {
                    hexagonMesh = cliffHexagonMesh;
                }
                else
                {
                    hexagonMesh = planeHexagonMesh;
                }

                hexagons[i].meshFilter.sharedMesh = hexagonMesh;
            }
        }

        private void UpdateHexagonHeight(TerrainData terrainData)
        {
            for (int i = 0; i < hexagons.Length; i++)
            {
                Center center = terrainData.centers[i];
                if (center.isWater)
                {
                    Vector3 newPosition = hexagons[i].transform.position;
                    newPosition.y = -0.25f;
                    hexagons[i].transform.position = newPosition;
                }
            }
        }

        private void UpdateHexagonColorMap(TerrainData terrainData)
        {
            int mapWidth = terrainData.terrainSize.x;
            int mapHeight = terrainData.terrainSize.y;
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(mapWidth, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(mapHeight, 2)));
            Texture2D colorMapTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };

            Color[] colorMap = new Color[textureWidth * textureHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    Center center = terrainData.centers[x + y * mapWidth];
                    Color color;
                    switch (previewMode)
                    {
                        case PreviewMode.Water:
                            color = center.isWater ? Color.blue : Color.black;
                            break;
                        case PreviewMode.Height:
                            color = Color.Lerp(Color.black, Color.white, center.elevation);
                            break;
                        case PreviewMode.Moisture:
                            color = Color.Lerp(Color.black, Color.blue, center.moisture);
                            break;
                        case PreviewMode.Temperature:
                            color = Color.HSVToRGB(Mathf.Lerp(0, 0.66666f, center.Temperature), 1, 1);
                            break;
                        case PreviewMode.Biome:
                            color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
                            break;
                        default:
                            color = Color.magenta;
                            break;
                    }
                    colorMap[x + y * textureWidth] = color;
                }
            }
            colorMapTexture.SetPixels(colorMap);
            colorMapTexture.Apply();

            hexagonMaterial.SetTexture("_ColorMap", colorMapTexture);
            hexagonMaterial.SetVector("_ColorMapSize", new Vector2(textureWidth, textureHeight));
            hexagonMaterial.SetInt("_EnableBrightNoise", previewMode == PreviewMode.Biome ? 1 : 0);
        }

        private Hexagon CreateNewHexagon(HexagonPos hexPos)
        {
            Hexagon newHexagon = Instantiate(hexagonPrefab, hexagonHolder);
            newHexagon.hexPos = hexPos;
            newHexagon.name = $"Hexagon {newHexagon.hexPos}";
            newHexagon.transform.position = newHexagon.hexPos.ToWorldPos();
            newHexagon.GetComponent<MeshRenderer>().sharedMaterial = hexagonMaterial;

            return newHexagon;
        }

        [Button]
        private void DestroyAllHexagons()
        {
            foreach (GameObject hexagon in GameObject.FindGameObjectsWithTag("Hexagon"))
            {
                DestroyImmediate(hexagon);
            }
            hexagons = null;
        }
    }
}
