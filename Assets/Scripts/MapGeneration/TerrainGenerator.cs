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
        [ProgressBar(0, 1)]
        public float seaLevel;
        public int riverSeed;
        public Vector2 riverSpawnRange;

        [Title("Preview")]
        [Required]
        public PreviewWorld previewWorld;
        public bool autoUpdateWorld;
        public MeshRenderer waterMapRenderer;
        public MeshRenderer nodeTypeMapRenderer;
        public MeshRenderer heightMapRenderer;
        public MeshRenderer riverMapRenderer;
        public MeshRenderer moistureMapRenderer;
        public MeshRenderer temperatureMapRenderer;

        [Title("Debug options")]
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
            GenerateWaterMap(seaLevel, ref islandNoiseMap, out bool[] waterMap);
            Color[] waterMapColors = waterMap.Select(x => x ? Color.white : Color.black).ToArray();
            UpdatePreviewTexture(width, height, waterMapRenderer, waterMapColors);

            // 바다, 호수, 육지, 해변 맵 생성
            GenerateNodeTypeMap(width, height, ref waterMap, out int[] nodeTypeMap);
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
            UpdatePreviewTexture(width, height, nodeTypeMapRenderer, nodeTypeColors);

            // 높이 맵 생성
            GenerateHeightMap(width, height, ref waterMap, out float[] heightMap);
            Color[] heightMapColors = heightMap.Select(x => Color.Lerp(Color.black, Color.white, x)).ToArray();
            UpdatePreviewTexture(width, height, heightMapRenderer, heightMapColors);

            // 강 생성
            GenerateRiverMap(width, height, riverSeed, riverSpawnRange, ref nodeTypeMap, ref heightMap, out int[] riverMap);
            int maxRiverStrength = riverMap.Max();
            Color[] riverMapColors = riverMap.Select(x => x == 0 ? Color.black : Color.Lerp(Color.cyan, Color.blue, x / (float)maxRiverStrength)).ToArray();
            UpdatePreviewTexture(width, height, riverMapRenderer, riverMapColors);

            // 습도 맵 생성
            GenerateMoistureMap(width, height, ref nodeTypeMap, ref riverMap, out float[] moistureMap);
            Color[] moistureMapColors = moistureMap.Select(x => Color.Lerp(Color.black, Color.blue, x)).ToArray();
            UpdatePreviewTexture(width, height, moistureMapRenderer, moistureMapColors);

            // 온도 맵 생성
            GenerateTemperatureMap(width, height, ref nodeTypeMap, ref heightMap, out float[] temperatureMap);
            Color[] temperatureMapColors = temperatureMap.Select(x => x < 0 ? Color.black : Color.HSVToRGB(Mathf.Lerp(0, 0.6667f, x), 1, 1)).ToArray();
            UpdatePreviewTexture(width, height, temperatureMapRenderer, temperatureMapColors);


            // 프리뷰 월드 업데이트
            Color[] hexColors = new Color[mapLength];
            for (int i = 0; i < mapLength; i++)
            {
                if (waterMap[i])
                {
                    if (nodeTypeMap[i] == (int)NodeType.Sea)
                    {
                        hexColors[i] = Color.Lerp(Color.black, Color.blue, heightMap[i]);
                    }
                    else
                    {
                        hexColors[i] = Color.cyan;
                    }
                }
                else
                {
                    hexColors[i] = heightMapColors[i];
                    if (riverMap[i] > 0)
                    {
                        hexColors[i] = Color.Lerp(Color.cyan, Color.blue, riverMap[i] / (float)maxRiverStrength);
                    }
                }
            }
            previewWorld.GenerateDefaultHexagons(mapSize);
            previewWorld.SetHexagonsColor(ref hexColors);
            previewWorld.SetHexagonsElevation(ref heightMap, heightMultiplier);
        }

        private void GenerateWaterMap(float seaLevel, ref float[] islandNoiseMap, out bool[] waterMap)
        {
            waterMap = islandNoiseMap
                .Select(x => x <= seaLevel)
                .ToArray();
        }

        private void GenerateNodeTypeMap(int width, int height, ref bool[] waterMap, out int[] nodeTypeMap)
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

        private void GenerateHeightMap(int width, int height, ref bool[] waterMap, out float[] heightMap)
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

        private void GenerateRiverMap(int width, int height, int seed, Vector2 riverSpawnRange, ref int[] nodeTypeMap, ref float[] heightMap, out int[] riverMap)
        {
            int mapLength = width * height;
            riverMap = new int[mapLength];

            System.Random random = new System.Random(seed);

            int maxSpawnTry = (width + height) / 2;
            for (int i = 0; i < maxSpawnTry; i++)
            {
                int spawnX = random.Next(width);
                int spawnY = random.Next(height);
                int spawnIndex = spawnX + spawnY * width;

                float spawnHeight = heightMap[spawnIndex];
                if (nodeTypeMap[spawnIndex] == (int)NodeType.Sea
                    || nodeTypeMap[spawnIndex] == (int)NodeType.Lake
                    || spawnHeight < riverSpawnRange.x
                    || spawnHeight > riverSpawnRange.y)
                {
                    continue;
                }

                // if not water
                // increase river value
                // find next lowland
                // repeat

                HexagonPos currentHexPos = HexagonPos.FromArrayXY(spawnX, spawnY);
                Vector2Int currentXY = currentHexPos.ToArrayXY();
                int currentIndex = currentXY.x + currentXY.y * width;
                int previousRiverValue = 0;
                while (nodeTypeMap[currentIndex] == (int)NodeType.Land || nodeTypeMap[currentIndex] == (int)NodeType.Coast)
                {
                    riverMap[currentIndex] += previousRiverValue + 1;
                    previousRiverValue = riverMap[currentIndex];

                    HexagonPos lowlandHexPos = new HexagonPos(1, 1);
                    float lowestHeight = float.MaxValue;
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
                            int neighborIndex = neighborXY.x + neighborXY.y * width;
                            if (heightMap[neighborIndex] < lowestHeight)
                            {
                                lowestHeight = heightMap[neighborIndex];
                                lowlandHexPos = neighborHexPos;
                            }
                        }
                    }

                    currentHexPos = lowlandHexPos;
                    currentXY = currentHexPos.ToArrayXY();
                    currentIndex = currentXY.x + currentXY.y * width;
                }
            }
        }

        private void GenerateMoistureMap(int width, int height, ref int[] nodeTypeMap, ref int[] riverMap, out float[] moistureMap)
        {
            int mapLength = width * height;
            moistureMap = new float[mapLength];

            Queue<HexagonPos> queue = new Queue<HexagonPos>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexagonPos hexPos = HexagonPos.FromArrayXY(x, y);
                    int index = x + y * width;
                    if (nodeTypeMap[index] == (int)NodeType.Lake)
                    {
                        moistureMap[index] = 1;
                        queue.Enqueue(hexPos);
                    }
                    else if (riverMap[index] > 0)
                    {
                        moistureMap[index] = Mathf.Min(riverMap[index] * 0.2f, 2f);
                        queue.Enqueue(hexPos);
                    }
                }
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
                        if (neighborXY.x < 0 || neighborXY.x > width - 1
                            || neighborXY.y < 0 || neighborXY.y > height - 1)
                        {
                            continue;
                        }

                        int neighborIndex = neighborXY.x + neighborXY.y * width;
                        if (nodeTypeMap[neighborIndex] != (int)NodeType.Land && nodeTypeMap[neighborIndex] != (int)NodeType.Coast)
                        {
                            continue;
                        }

                        float newMoisture = moistureMap[currentIndex] * 0.9f;
                        if (newMoisture > moistureMap[neighborIndex])
                        {
                            moistureMap[neighborIndex] = newMoisture;
                            queue.Enqueue(neighborHexPos);
                        }
                    }
                }
            }

            for (int i = 0; i < mapLength; i++)
            {
                if (nodeTypeMap[i] == (int)NodeType.Coast)
                {
                    moistureMap[i] = Mathf.Min(moistureMap[i] * 1.5f, 1f);
                }
            }

            // TODO: smoothing 필터 적용해보면 좋을 듯

            // redistribute
            var sortKV = new List<KeyValuePair<int, float>>();
            for (int i = 0; i < mapLength; i++)
            {
                if (moistureMap[i] > 0)
                {
                    sortKV.Add(new KeyValuePair<int, float>(i, moistureMap[i]));
                }
            }
            sortKV.Sort((x, y) => x.Value.CompareTo(y.Value));
            for (int i = 0; i < sortKV.Count; i++)
            {
                moistureMap[sortKV[i].Key] = i / (float)(sortKV.Count - 1);
            }
        }

        private void GenerateTemperatureMap(int width, int height, ref int[] nodeTypeMap, ref float[] heightMap, out float[] temperatureMap)
        {
            int mapLength = width * height;
            temperatureMap = new float[mapLength];

            float minLandHeight = float.MaxValue;
            for (int i = 0; i < mapLength; i++)
            {
                if (nodeTypeMap[i] == (int)NodeType.Sea)
                {
                    temperatureMap[i] = -1;
                }
                else
                {
                    temperatureMap[i] = heightMap[i];
                    minLandHeight = Mathf.Min(heightMap[i], minLandHeight);
                }
            }

            for (int i = 0; i < mapLength; i++)
            {
                if (temperatureMap[i] > 0)
                {
                    temperatureMap[i] = Mathf.InverseLerp(minLandHeight, 1, temperatureMap[i]);
                }
            }
        }

        private void GenerateBiomeMap(int width, int height, ref float[] moistureMap, ref float[] temperatureMap, out int[] biomeMap)
        {
            int mapLength = width * height;
            biomeMap = new int[mapLength];
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
    }
}
