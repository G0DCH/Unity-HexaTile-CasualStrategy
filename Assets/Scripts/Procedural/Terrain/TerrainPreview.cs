using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TilePuzzle.Procedural
{
    [ExecuteInEditMode]
    public class TerrainPreview : MonoBehaviour
    {
        [Title("Generate Settings", Bold = true, TitleAlignment = TitleAlignments.Centered)]
        public int globalSeed;
        [Min(10)]
        public Vector2Int terrainSize = new Vector2Int(20, 20);

        [Title("Island Shape")]
        [ProgressBar(0, 1, 0.4f, 0.9f, 0.2f)]
        public float landRatio = 0.6f;
        [Range(0.1f, 1f)]
        public float lakeThreshold = 0.3f;
        [Range(0f, 2f)]
        public float peakMultiplier = 1.1f;
        [FoldoutGroup("Island Shape Noise", GroupName = "Noise Settings"), HideLabel]
        public NoiseSettings terrainShapeNoiseSettings;
        [FoldoutGroup("Island Shape Falloff", GroupName = "Falloff Settings"), HideLabel]
        public FalloffSettings terrainShapeFalloffSettings;

        [Title("River")]
        public int riverSeed;
        [MinMaxSlider(0, 1, true)]
        public Vector2 riverSpawnRange = new Vector2(0.3f, 0.9f);
        public float riverSpawnMultiplier = 1;

        [Title("Biome")]
        [Required]
        public BiomeTableSettings biomeTableSettings;


        [Title("Rendering Settings", Bold = true, TitleAlignment = TitleAlignments.Centered)]
        [Required] public HexagonObject hexagonObjectPrefab;

        [Title("Land")]
        [Required]
        public Material landMaterial;
        [Min(0.1f)] public float cliffDepth = 1.5f;
        public bool enableBrightNoise = true;

        [Title("Water")]
        [Required]
        public Material seaMaterial;
        [Required]
        public Material lakeMaterial;
        [PropertyRange(0, nameof(cliffDepth))]
        public float waterDepth = 0.3f;

        [Title("River")]
        [PropertyRange(0.05f, 1f)]
        public float riverSize = 0.2f;


        [Title("Decoration Settings", Bold = true, TitleAlignment = TitleAlignments.Centered)]
        public DecorationSpawnSettings decorationSpawnSettings;


        [Title("Preview Settings", Bold = true, TitleAlignment = TitleAlignments.Centered)]
        public bool autoUpdatePreview;
        public bool previewDecoration;
        [EnumToggleButtons]
        public PreviewMode previewMode;


        [Title("Export Settings", Bold = true, TitleAlignment = TitleAlignments.Centered)]
        [FolderPath]
        public string exportPath = "Assets/Settings";

        private bool settingUpdated;
        private TerrainGenerateSettings generateSettings;
        private HexagonObject[] spawnedHexagonObjects;
        private Vector2Int previousHexagonMapSize;

        public enum PreviewMode
        {
            Water, Height, Moisture, Temperature, Biome, Combine
        }

        private void Update()
        {
            // 설정값이 변경되면 미리보기 지형 업데이트
            if (settingUpdated)
            {
                settingUpdated = false;

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

        [Button]
        private void UpdateTerrainPreview()
        {
            UpdateGenerateSettings();

            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(globalSeed, generateSettings);
            SpawnHexagons(terrainData);
            UpdateHexagonColors(terrainData);

            CleanUpDecorations();
            if (previewDecoration)
            {
                DecorationData decorationData = DecorationGenerator.GenerateDecorationData(globalSeed, terrainData, decorationSpawnSettings);
                SpawnDecorations(terrainData.terrainGraph.size, decorationData.renderDatas);
            }
        }

        [Button]
        private void ExportGenerateSettings()
        {
            UpdateGenerateSettings();
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(generateSettings, $"{exportPath}/{nameof(TerrainGenerateSettings)}_{DateTime.Now.Ticks}.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = generateSettings;
#endif
            generateSettings = null;
            if (autoUpdatePreview)
            {
                UpdateTerrainPreview();
            }
        }

        private void UpdateGenerateSettings()
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

        private void SpawnHexagons(TerrainData terrainData)
        {
            int width = terrainData.terrainGraph.size.x;
            int height = terrainData.terrainGraph.size.y;
            if (previousHexagonMapSize != terrainData.terrainGraph.size)
            {
                CleanUpHexagons();
                spawnedHexagonObjects = new HexagonObject[width * height];
                previousHexagonMapSize = terrainData.terrainGraph.size;
            }

            Mesh flatHexagonMesh = HexagonMeshGenerator.BuildMesh(HexagonObject.Size);
            Mesh cliffHexagonMesh = HexagonMeshGenerator.BuildMesh(HexagonObject.Size, cliffDepth);
            var riverMeshCache = new Dictionary<HexagonMeshGenerator.VertexDirection, Mesh>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Center center = terrainData.terrainGraph.centers[x + y * width];
                    if (spawnedHexagonObjects[x + y * width] == null)
                    {
                        spawnedHexagonObjects[x + y * width] = CreateNewHexagon(hexagonObjectPrefab, HexagonPos.FromArrayXY(x, y));
                    }
                    HexagonObject currentHexagonObject = spawnedHexagonObjects[x + y * width];

                    if (center.isWater)
                    {
                        currentHexagonObject.SetMesh(flatHexagonMesh);
                        currentHexagonObject.SetMaterial(center.isSea ? seaMaterial : lakeMaterial);
                    }
                    else
                    {
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
                                mesh = HexagonMeshGenerator.BuildMesh(HexagonObject.Size, cliffDepth, riverSize, riverDirection);
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
                        currentHexagonObject.SetMesh(mesh);
                        currentHexagonObject.SetMaterial(landMaterial);
                    }

                    // height
                    Vector3 newPos = currentHexagonObject.transform.position;
                    newPos.y = center.isWater ? -waterDepth : 0;
                    currentHexagonObject.transform.position = newPos;
                }
            }
        }

        private void UpdateHexagonColors(TerrainData terrainData)
        {
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(terrainData.terrainGraph.size.x, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(terrainData.terrainGraph.size.y, 2)));
            var colorMap = new Color[textureWidth * textureHeight];

            for (int y = 0; y < terrainData.terrainGraph.size.y; y++)
            {
                for (int x = 0; x < terrainData.terrainGraph.size.x; x++)
                {
                    Center center = terrainData.terrainGraph.centers[x + y * terrainData.terrainGraph.size.x];
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
                            color = Color.HSVToRGB(Mathf.Lerp(0.66666f, 0, center.Temperature), 1, 1);
                            break;
                        case PreviewMode.Biome:
                            color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
                            break;
                        case PreviewMode.Combine:
                            color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
                            break;
                        default:
                            color = Color.magenta;
                            break;
                    }
                    colorMap[x + y * textureWidth] = color;
                }
            }

            Texture2D colorMapTexture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };
            colorMapTexture.SetPixels(colorMap);
            colorMapTexture.Apply();
            landMaterial.SetTexture("_ColorMap", colorMapTexture);
            landMaterial.SetVector("_ColorMapSize", new Vector2(textureWidth, textureHeight));
            landMaterial.SetInt("_EnableBrightNoise", enableBrightNoise ? 1 : 0);
        }

        private void CleanUpHexagons()
        {
            foreach (GameObject hexagonObject in GameObject.FindGameObjectsWithTag("Hexagon"))
            {
                if (Application.isPlaying)
                {
                    Destroy(hexagonObject);
                }
                else
                {
                    DestroyImmediate(hexagonObject);
                }
            }
        }

        private void SpawnDecorations(Vector2Int mapSize, DecorationData.RenderData?[] renderDatas)
        {
            for (int i = 0; i < renderDatas.Length; i++)
            {
                if (renderDatas[i].HasValue == false)
                {
                    continue;
                }

                int x = i % mapSize.x;
                int y = i / mapSize.x;
                Vector3 decorationPos = HexagonPos.FromArrayXY(x, y).ToWorldPos();

                GameObject newDecorationObject = CloneDecorationObject(renderDatas[i].Value, transform, decorationPos);
            }
        }

        private void CleanUpDecorations()
        {
            foreach (GameObject hexagonObject in GameObject.FindGameObjectsWithTag("Decoration"))
            {
                if (Application.isPlaying)
                {
                    Destroy(hexagonObject);
                }
                else
                {
                    DestroyImmediate(hexagonObject);
                }
            }
        }

        private GameObject CloneDecorationObject(DecorationData.RenderData renderData, Transform parent, Vector3 position)
        {
            GameObject decoration = Instantiate(renderData.prefab, parent);

            decoration.transform.position = position;
            decoration.transform.LookAt(position + renderData.lookDirection);
            decoration.transform.localScale = renderData.scale;

            return decoration;
        }

        private HexagonObject CreateNewHexagon(HexagonObject hexagonObjectPrefab, HexagonPos hexPos)
        {
            HexagonObject newHexagon = Instantiate(hexagonObjectPrefab, transform);
            newHexagon.name = $"Hexagon {hexPos}";
            newHexagon.transform.position = hexPos.ToWorldPos();

            newHexagon.hexPos = hexPos;

            return newHexagon;
        }
    }
}
