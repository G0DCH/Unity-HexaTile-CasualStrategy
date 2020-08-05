using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TilePuzzle.Rendering;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class TerrainRenderer : MonoBehaviour
    {
        [Required] public Hexagon hexagonPrefab;

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

        private Hexagon[] spawnedHexagonObjects;

        /// <summary>
        /// 입력을 기반으로 헥사곤 지형 오브젝트 생성
        /// </summary>
        /// <param name="terrainData">지형 생성에 필요한 정보</param>
        public void SpawnHexagonTerrains(TerrainData terrainData)
        {
            CleanUpHexagons();
            int width = terrainData.terrainSize.x;
            int height = terrainData.terrainSize.y;
            spawnedHexagonObjects = new Hexagon[width * height];

            Mesh flatHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size);
            Mesh cliffHexagonMesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, cliffDepth);
            var riverMeshCache = new Dictionary<HexagonMeshGenerator.VertexDirection, Mesh>();

            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(width, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(height, 2)));
            Color[] colorMap = new Color[textureWidth * textureHeight];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Center center = terrainData.centers[x + y * width];
                    Hexagon newHexagon = CreateNewHexagon(hexagonPrefab, HexagonPos.FromArrayXY(x, y));
                    spawnedHexagonObjects[x + y * width] = newHexagon;

                    if (center.isWater)
                    {
                        newHexagon.meshFilter.sharedMesh = flatHexagonMesh;
                        newHexagon.GetComponent<MeshRenderer>().sharedMaterial = center.isSea ? seaMaterial : lakeMaterial;
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
                                mesh = HexagonMeshGenerator.BuildMesh(Hexagon.Size, cliffDepth, riverSize, riverDirection);
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
                        newHexagon.GetComponent<MeshRenderer>().sharedMaterial = landMaterial;
                    }

                    // height
                    Vector3 newPos = newHexagon.transform.position;
                    newPos.y = center.isWater ? -waterDepth : 0;
                    newHexagon.transform.position = newPos;

                    // color
                    Color color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
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
            if (spawnedHexagonObjects == null)
            {
                return;
            }

            foreach (Hexagon hexagon in spawnedHexagonObjects)
            {
                Destroy(hexagon.gameObject);
            }
        }

        private Hexagon CreateNewHexagon(Hexagon hexagonPrefab, HexagonPos hexPos)
        {
            Hexagon newHexagon = Instantiate(hexagonPrefab, transform);
            newHexagon.name = $"Hexagon {hexPos}";
            newHexagon.transform.position = hexPos.ToWorldPos();
            newHexagon.meshFilter = newHexagon.GetComponent<MeshFilter>();

            newHexagon.hexPos = hexPos;

            return newHexagon;
        }
    }
}
