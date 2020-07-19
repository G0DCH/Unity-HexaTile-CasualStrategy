using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle
{
    [ExecuteInEditMode]
    public class TerrainGenerator : MonoBehaviour
    {
        [Title("Generate options")]
        public Vector2Int mapSize = new Vector2Int(30, 30);
        public NoiseMapGenerator islandNoise;
        public FalloffMapGenerator islandFalloff;
        [Range(0, 1)]
        public float seaLevel;

        [Title("Debug options")]
        [Required]
        public PreviewWorld previewWorld;
        public bool autoUpdateWorld;
        public Color lowlandColor;
        public Color highlandColor;
        public Color oceanColor;
        public MeshRenderer heightMapRenderer;
        public float heightMultiplier;

        [HideInInspector]
        public bool hasParameterUpdated;

        public enum NodeType : int
        {
            Invalid = 0, Land = 1, Coast = 2, Sea = 3, Lake = 4
        }

        private void Update()
        {
            if (hasParameterUpdated && autoUpdateWorld)
            {
                hasParameterUpdated = false;
                GenerateTerrain();
            }
        }

        private void OnValidate()
        {
            hasParameterUpdated = true;
        }

        [Button]
        public void GenerateTerrain()
        {
            int width = mapSize.x;
            int height = mapSize.y;
            int mapLength = width * height;

            // 섬 노이즈
            islandNoise.GenerateNoiseMap(width, height, out float[] islandNoiseMap);
            islandFalloff.GenerateFalloffMap(width, height, out float[] falloffMap);
            for (int i = 0; i < mapLength; i++)
            {
                islandNoiseMap[i] *= falloffMap[i];
            }

            // 물 맵 생성
            CalculateWaterMap(seaLevel, ref islandNoiseMap, out bool[] waterMap);

            // 바다, 호수, 육지, 해변 맵 생성
            CalculateNodeTypeMap(width, height, ref waterMap, out int[] nodeTypeMap);

            // 높이 맵 생성
            CalculateHeightMap(width, height, ref waterMap, ref nodeTypeMap, out float[] heightMap);
            Color[] heightMapColors = heightMap.Select(x => Color.Lerp(Color.black, Color.white, x)).ToArray();
            UpdatePreviewTexture(width, height, heightMapRenderer, heightMapColors);

            previewWorld.GenerateDefaultHexagons(mapSize);
            Color[] nodeTypeColors = nodeTypeMap.Select(x =>
            {
                switch ((NodeType)x)
                {
                    case NodeType.Land:
                        return Color.green;
                    case NodeType.Coast:
                        return Color.yellow;
                    case NodeType.Sea:
                        return Color.blue;
                    case NodeType.Lake:
                        return Color.cyan;
                    default:
                        return Color.black;
                }
            }).ToArray();
            previewWorld.SetHexagonsColor(ref nodeTypeColors);
            previewWorld.SetHexagonsElevation(ref heightMap, heightMultiplier);
        }

        private void CalculateWaterMap(float seaLevel, ref float[] islandNoiseMap, out bool[] waterMap)
        {
            waterMap = islandNoiseMap
                .Select(x => x <= seaLevel)
                .ToArray();
        }

        private void CalculateNodeTypeMap(int width, int height, ref bool[] waterMap, out int[] nodeTypeMap)
        {
            int mapLength = width * height;
            nodeTypeMap = new int[mapLength];

            // 맵 경계는 바다
            Queue<HexagonPos> floodFillQueue = new Queue<HexagonPos>();
            for (int x = 0; x < width; x++)
            {
                nodeTypeMap[x] = (int)NodeType.Sea;
                nodeTypeMap[x + (height - 1) * width] = (int)NodeType.Sea;
                floodFillQueue.Enqueue(HexagonPos.FromArrayXY(x, 0));
                floodFillQueue.Enqueue(HexagonPos.FromArrayXY(x, height - 1));
            }
            for (int y = 1; y < height - 1; y++)
            {
                nodeTypeMap[y * width] = (int)NodeType.Sea;
                nodeTypeMap[width - 1 + y * width] = (int)NodeType.Sea;
                floodFillQueue.Enqueue(HexagonPos.FromArrayXY(0, y));
                floodFillQueue.Enqueue(HexagonPos.FromArrayXY(width - 1, y));
            }

            // flood fill으로 바다 채우기
            while (floodFillQueue.Count > 0)
            {
                HexagonPos hexPos = floodFillQueue.Dequeue();
                for (int hexZ = -1; hexZ <= 1; hexZ++)
                {
                    for (int hexX = -1; hexX <= 1; hexX++)
                    {
                        if (hexX == hexZ)
                        {
                            continue;
                        }

                        HexagonPos neighborHexPos = hexPos + new HexagonPos(hexX, hexZ);
                        Vector2Int neighborXY = neighborHexPos.ToArrayXY();
                        if (neighborXY.x < 0 || neighborXY.x >= width || neighborXY.y < 0 || neighborXY.y >= height)
                        {
                            continue;
                        }

                        int neighborIndex = neighborXY.x + neighborXY.y * width;
                        if (waterMap[neighborIndex] == true && nodeTypeMap[neighborIndex] != (int)NodeType.Sea)
                        {
                            nodeTypeMap[neighborIndex] = (int)NodeType.Sea;
                            floodFillQueue.Enqueue(neighborHexPos);
                        }
                    }
                }
            }

            // 호수, 해변, 땅 생성
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = x + y * width;
                    if (nodeTypeMap[currentIndex] == (int)NodeType.Sea)
                    {
                        continue;
                    }

                    if (waterMap[currentIndex] == true)
                    {
                        if (nodeTypeMap[currentIndex] != (int)NodeType.Sea)
                        {
                            nodeTypeMap[x + y * width] = (int)NodeType.Lake;
                        }
                        continue;
                    }

                    HexagonPos hexPos = HexagonPos.FromArrayXY(x, y);
                    bool surroundedBySea = false;
                    for (int hexZ = -1; hexZ <= 1; hexZ++)
                    {
                        for (int hexX = -1; hexX <= 1; hexX++)
                        {
                            if (hexX == hexZ)
                            {
                                continue;
                            }

                            HexagonPos neighborHexPos = hexPos + new HexagonPos(hexX, hexZ);
                            Vector2Int neighborXY = neighborHexPos.ToArrayXY();
                            int neighborIndex = neighborXY.x + neighborXY.y * width;
                            if (nodeTypeMap[neighborIndex] == (int)NodeType.Sea)
                            {
                                surroundedBySea = true;
                            }
                        }
                    }

                    if (surroundedBySea)
                    {
                        nodeTypeMap[x + y * width] = (int)NodeType.Coast;
                    }
                    else
                    {
                        nodeTypeMap[x + y * width] = (int)NodeType.Land;
                    }
                }
            }
        }

        private void CalculateHeightMap(int width, int height, ref bool[] waterMap, ref int[] nodeTypeMap, out float[] heightMap)
        {
            int mapLength = width * height;
            heightMap = new float[mapLength];
            for (int i = 0; i < mapLength; i++)
            {
                heightMap[i] = float.MaxValue;
            }

            // 맵 경계의 높이는 가장 낮음
            Queue<HexagonPos> queue = new Queue<HexagonPos>();
            for (int x = 0; x < width; x++)
            {
                heightMap[x] = 0;
                heightMap[x + (height - 1) * width] = 0;
                queue.Enqueue(HexagonPos.FromArrayXY(x, 0));
                queue.Enqueue(HexagonPos.FromArrayXY(x, height - 1));
            }
            for (int y = 1; y < height - 1; y++)
            {
                heightMap[y * width] = 0;
                heightMap[width - 1 + y * width] = 0;
                queue.Enqueue(HexagonPos.FromArrayXY(0, y));
                queue.Enqueue(HexagonPos.FromArrayXY(width - 1, y));
            }

            while (queue.Count > 0)
            {
                HexagonPos currentHexPos = queue.Dequeue();
                Vector2Int currentXY = currentHexPos.ToArrayXY();
                int currentIndex = currentXY.x + currentXY.y * width;
                for (int hexZ = -1; hexZ <= 1; hexZ++)
                {
                    for (int hexX = -1; hexX <= 1; hexX++)
                    {
                        if (hexX == hexZ)
                        {
                            continue;
                        }

                        HexagonPos neighborHexPos = currentHexPos + new HexagonPos(hexX, hexZ);
                        Vector2Int neighborXY = neighborHexPos.ToArrayXY();
                        if (neighborXY.x < 0 || neighborXY.x >= width || neighborXY.y < 0 || neighborXY.y >= height)
                        {
                            continue;
                        }

                        int neighborIndex = neighborXY.x + neighborXY.y * width;

                        float newHeight = heightMap[currentIndex] + 0.01f;
                        if (waterMap[currentIndex] == false && waterMap[neighborIndex] == false)
                        {
                            newHeight += 1;
                            // add more randomness
                        }

                        if (newHeight < heightMap[neighborIndex])
                        {
                            heightMap[neighborIndex] = newHeight;
                            queue.Enqueue(neighborHexPos);
                        }
                    }
                }
            }

            // redistribute
            float scaleFactor = 1.1f;
            var sortKV = new KeyValuePair<int, float>[mapLength];
            for (int i = 0; i < mapLength; i++)
            {
                sortKV[i] = new KeyValuePair<int, float>(i, heightMap[i]);
            }

            sortKV.Sort((x, y) => x.Value.CompareTo(y.Value));
            for (int i = 0; i < mapLength; i++)
            {
                float y = i / (float)(mapLength - 1);
                float x = Mathf.Sqrt(scaleFactor) - Mathf.Sqrt(scaleFactor * (1 - y));
                x = Mathf.Min(x, 1);
                heightMap[sortKV[i].Key] = x;
            }
        }

        private void UpdatePreviewTexture(int width, int height, MeshRenderer renderer, Color[] colors)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            texture.SetPixels(colors);
            texture.Apply();

            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_Texture", texture);
            renderer.SetPropertyBlock(properties);
        }

        //public void CalculateNodeType(int width, int height, float seaLevel, ref float[] islandNoiseMap, out int[] nodeTypeMap)
        //{
        //    // SeaLevel 기준으로 water map 생성
        //    bool[] waterMap = islandNoiseMap
        //        .Select(x => x <= seaLevel)
        //        .ToArray();

        //    // 맵 경계를 바다로 설정
        //    int mapLength = width * height;
        //    nodeTypeMap = new int[mapLength];
        //    for (int x = 0; x < width; x++)
        //    {
        //        nodeTypeMap[x] = (int)NodeType.Sea;
        //        nodeTypeMap[x + (height - 1) * width] = (int)NodeType.Sea;
        //    }
        //    for (int y = 1; y < height - 1; y++)
        //    {
        //        nodeTypeMap[y * width] = (int)NodeType.Sea;
        //        nodeTypeMap[width - 1 + y * width] = (int)NodeType.Sea;
        //    }

        //    // flood fill 으로 바다, 땅 설정
        //    Queue<Vector2Int> floodFillQueue = new Queue<Vector2Int>();
        //    floodFillQueue.Enqueue(new Vector2Int(1, 1));
        //    nodeTypeMap[1 + 1 * width] = (int)NodeType.Sea;
        //    while (floodFillQueue.Count > 0)
        //    {
        //        Vector2Int currentXY = floodFillQueue.Dequeue();
        //        NodeType currentNodeType = (NodeType)nodeTypeMap[currentXY.x + currentXY.y * width];
        //        for (int hexZ = -1; hexZ <= 1; hexZ++)
        //        {
        //            for (int hexX = -1; hexX <= 1; hexX++)
        //            {
        //                if ((hexX == -1 && hexZ == -1) || (hexX == 0 && hexZ == 0) || (hexX == 1 && hexZ == 1))
        //                {
        //                    continue;
        //                }

        //                Vector2Int neighborXY = currentXY + new HexagonPos(hexX, hexZ).ToArrayXY();
        //                int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                if (nodeTypeMap[neighborIndex] != (int)NodeType.Invalid)
        //                {
        //                    continue;
        //                }

        //                if (waterMap[neighborIndex])
        //                {
        //                    bool surroundedBySea = false;
        //                    for (int neighborHexZ = -1; neighborHexZ <= 1; neighborHexZ++)
        //                    {
        //                        for (int neighborHexX = -1; neighborHexX <= 1; neighborHexX++)
        //                        {
        //                            if ((neighborHexX == -1 && neighborHexZ == -1) || (neighborHexX == 0 && neighborHexZ == 0) || (neighborHexX == 1 && neighborHexZ == 1))
        //                            {
        //                                continue;
        //                            }

        //                            Vector2Int neighborNeighborXY = currentXY + new HexagonPos(neighborHexX, neighborHexZ).ToArrayXY();
        //                            int neighborNeighborIndex = neighborNeighborXY.x + neighborNeighborXY.y * width;
        //                            if (nodeTypeMap[neighborNeighborIndex] == (int)NodeType.Sea)
        //                            {
        //                                surroundedBySea = true;
        //                                break;
        //                            }
        //                        }

        //                        if (surroundedBySea)
        //                        {
        //                            break;
        //                        }
        //                    }

        //                    if (surroundedBySea)
        //                    {
        //                        nodeTypeMap[neighborIndex] = (int)NodeType.Sea;
        //                    }
        //                }
        //                else
        //                {
        //                    nodeTypeMap[neighborIndex] = (int)NodeType.Land;
        //                }

        //                if (nodeTypeMap[neighborIndex] != (int)NodeType.Invalid)
        //                {
        //                    floodFillQueue.Enqueue(neighborXY);
        //                }
        //            }
        //        }
        //    }

        //    for (int y = 1; y < height - 1; y++)
        //    {
        //        for (int x = 1; x < width - 1; x++)
        //        {
        //            Vector2Int currentXY = new Vector2Int(x, y);
        //            int currentIndex = x + y * width;
        //            if (nodeTypeMap[currentIndex] == (int)NodeType.Land)
        //            {
        //                // check neighbor
        //                bool surroundedBySea = false;
        //                for (int hexZ = -1; hexZ <= 1; hexZ++)
        //                {
        //                    for (int hexX = -1; hexX <= 1; hexX++)
        //                    {
        //                        if ((hexX == -1 && hexZ == -1) || (hexX == 0 && hexZ == 0) || (hexX == 1 && hexZ == 1))
        //                        {
        //                            continue;
        //                        }

        //                        Vector2Int neighborXY = currentXY + new HexagonPos(hexX, hexZ).ToArrayXY();
        //                        int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                        if (nodeTypeMap[neighborIndex] == (int)NodeType.Sea)
        //                        {
        //                            surroundedBySea = true;
        //                            break;
        //                        }
        //                    }

        //                    if (surroundedBySea)
        //                    {
        //                        break;
        //                    }
        //                }

        //                if (surroundedBySea)
        //                {
        //                    nodeTypeMap[currentIndex] = (int)NodeType.Coast;
        //                }
        //            }
        //            else if (nodeTypeMap[currentIndex] == (int)NodeType.Invalid)
        //            {
        //                nodeTypeMap[currentIndex] = (int)NodeType.Lake;
        //            }
        //        }
        //    }
        //}
    }
}
