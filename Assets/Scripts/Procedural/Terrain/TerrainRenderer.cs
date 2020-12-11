using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    [Obsolete("더 이상 사용되지 않음, HexagonTerrain을 사용", true)]
    public class TerrainRenderer : MonoBehaviour
    {
        [Required]
        public HexagonTileObject hexagonObjectPrefab;
        public Transform hexagonHolder;

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
        public float waterDepth = 0.5f;

        [Title("River")]
        [PropertyRange(0.05f, 1f)]
        public float riverSize = 0.2f;

        private HexagonTileObject[] spawnedHexagonObjects;

        /// <summary>
        /// 입력을 기반으로 헥사곤 지형 오브젝트 생성
        /// </summary>
        /// <param name="terrainData">지형 생성에 필요한 정보</param>
        public void SpawnHexagonTerrains(TerrainData terrainData)
        {
            //Profiler.BeginSample(nameof(SpawnHexagonTerrains));

            //CleanUpHexagons();
            //int width = terrainData.terrainGraph.size.x;
            //int height = terrainData.terrainGraph.size.y;
            //spawnedHexagonObjects = new HexagonTileObject[width * height];

            //if (hexagonHolder == null)
            //{
            //    hexagonHolder = new GameObject("Hexagon Holder").transform;
            //}

            //Mesh flatHexagonMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize);
            //Mesh cliffHexagonMesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize, cliffDepth);
            //var riverMeshCache = new Dictionary<HexagonMeshGenerator.VertexDirection, Mesh>();

            //int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(width, 2)));
            //int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(height, 2)));
            //Color[] colorMap = new Color[textureWidth * textureHeight];

            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {
            //        Center center = terrainData.terrainGraph.centers[x + y * width];
            //        HexagonTileObject newHexagonObject = CreateNewHexagonObject(hexagonObjectPrefab, hexagonHolder, HexagonPos.FromArrayXY(x, y));
            //        spawnedHexagonObjects[x + y * width] = newHexagonObject;

            //        if (center.isWater)
            //        {
            //            newHexagonObject.TileMesh = flatHexagonMesh;
            //            newHexagonObject.TileMaterial = center.isSea ? seaMaterial : lakeMaterial;
            //        }
            //        else
            //        {
            //            HexagonMeshGenerator.VertexDirection riverDirection = 0;
            //            for (int neighborIndex = 0; neighborIndex < center.NeighborCorners.Length; neighborIndex++)
            //            {
            //                Corner neighborCorner = center.NeighborCorners[neighborIndex];
            //                if (neighborCorner.river > 0)
            //                {
            //                    riverDirection |= (HexagonMeshGenerator.VertexDirection)(1 << neighborIndex);
            //                }
            //            }

            //            Mesh mesh;
            //            // 강이 있을때
            //            if (center.isWater == false && riverDirection > 0)
            //            {
            //                if (riverMeshCache.TryGetValue(riverDirection, out mesh) == false)
            //                {
            //                    mesh = HexagonMeshGenerator.BuildMesh(HexagonTileObject.TileSize, cliffDepth, riverSize, null);
            //                    riverMeshCache.Add(riverDirection, mesh);
            //                }
            //            }
            //            // 절벽일때 (주변에 물)
            //            else if (center.isWater == false && center.NeighborCenters.Values.Any(neighborrCenter => neighborrCenter.isWater))
            //            {
            //                mesh = cliffHexagonMesh;
            //            }
            //            // 평지일때
            //            else
            //            {
            //                mesh = flatHexagonMesh;
            //            }
            //            newHexagonObject.TileMesh = mesh;
            //            newHexagonObject.TileMaterial = landMaterial;
            //        }

            //        // height
            //        Vector3 newPos = newHexagonObject.transform.position;
            //        newPos.y = center.isWater ? -waterDepth : 0;
            //        newHexagonObject.transform.position = newPos;

            //        // color
            //        Color color = terrainData.biomeTable.biomeDictionary[center.biomeId].color;
            //        colorMap[x + y * textureWidth] = color;
            //    }
            //}

            //Texture2D colorMapTexture = new Texture2D(textureWidth, textureHeight)
            //{
            //    filterMode = FilterMode.Point
            //};
            //colorMapTexture.SetPixels(colorMap);
            //colorMapTexture.Apply();
            //landMaterial.SetTexture("_ColorMap", colorMapTexture);
            //landMaterial.SetVector("_ColorMapSize", new Vector2(textureWidth, textureHeight));
            //landMaterial.SetInt("_EnableBrightNoise", enableBrightNoise ? 1 : 0);

            //Profiler.EndSample();
        }

        public void CleanUpHexagons()
        {
            if (Application.isPlaying)
            {
                if (spawnedHexagonObjects != null)
                {
                    foreach (HexagonTileObject hexagonObject in spawnedHexagonObjects)
                    {
                        Destroy(hexagonObject.gameObject);
                    }
                    spawnedHexagonObjects = null;
                }
            }
            else
            {
                foreach (GameObject hexagonObject in GameObject.FindGameObjectsWithTag("Hexagon"))
                {
                    DestroyImmediate(hexagonObject);
                }
            }
        }

        private HexagonTileObject CreateNewHexagonObject(HexagonTileObject hexagonPrefab, Transform parent, HexagonPos hexPos)
        {
            HexagonTileObject newHexagon = Instantiate(hexagonPrefab, parent);
            newHexagon.name = $"Hexagon {hexPos}";
            newHexagon.transform.position = hexPos.ToWorldPos();

            //newHexagon.hexPos = hexPos;

            return newHexagon;
        }
    }
}
