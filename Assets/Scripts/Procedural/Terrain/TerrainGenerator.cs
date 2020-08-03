using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public static class TerrainGenerator
    {
        public static TerrainData GenerateTerrainData(TerrainGenerateSettings settings)
        {
            Profiler.BeginSample(nameof(GenerateTerrainData));

            Vector2Int terrainSize = settings.terrainSize;
            GraphGenerator.CreateHexagonGraph(terrainSize.x, terrainSize.y, out Center[] centers, out Corner[] corners);

            // 섬 모양 계산
            CalculateIslandShape(terrainSize, settings.landRatio, settings.globalSeed, settings.terrainShapeNoiseSettings, settings.terrainShapeFalloffSettings, ref corners);

            // 물, 바다, 땅, 해변 설정
            CalculateWaterGroundType(settings.lakeThreshold, ref centers, ref corners);

            // 높이 분포 계산
            CalculateElevation(settings.peakMultiplier, ref centers, ref corners);

            // 내리막 계산
            CalculateDownSlope(ref corners);

            // 강 생성
            int riverSpawnTry = (int)((terrainSize.x + terrainSize.y) / 2 * settings.riverSpawnMultiplier);
            CalculateRiver(settings.globalSeed + settings.riverSeed, riverSpawnTry, settings.riverSpawnRange, ref corners);

            // 습도 계산
            CalculateMoisture(ref centers, ref corners);

            // 바이옴 생성
            BiomeTable biomeTable = settings.biomeTableSettings.GetBiomeTable();
            CalculateBiome(biomeTable, ref centers);

            Profiler.EndSample();

            TerrainData terrainData = new TerrainData(terrainSize, centers, corners, biomeTable);
            return terrainData;
        }

        private static void CalculateIslandShape(Vector2Int mapSize, float targetLandRatio, int globalSeed, NoiseSettings noiseSettings, FalloffSettings falloffSettings, ref Corner[] corners)
        {
            // 노이즈 값이 seaLevel 보다 낮으면 물로 설정
            Vector2[] cornerPoints = corners.Select(x => new Vector2(x.cornerPos.x, x.cornerPos.z)).ToArray();
            NoiseGenerator.Instance.EvaluateNoise(ref cornerPoints, out float[] cornerNoiseValues, globalSeed, noiseSettings);
            FalloffGenerator.Instance.EvaluateFalloff(mapSize.x, mapSize.y, ref cornerPoints, out float[] cornerFalloffValues, falloffSettings);    // TODO: radial falloff 으로 변경

            // Binary search
            float min = 0;
            float max = 1;
            float seaLevel = 0.5f;
            int iteration = 0;
            while (iteration++ < 10)
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i].isWater = cornerNoiseValues[i] * cornerFalloffValues[i] < seaLevel;
                }

                float currentLandRatio = corners.Count(x => x.isWater == false) / (float)corners.Length;
                if (Mathf.Abs(currentLandRatio - targetLandRatio) <= 0.01f)
                {
                    break;
                }
                else if (currentLandRatio < targetLandRatio)
                {
                    max = seaLevel;
                    seaLevel = (seaLevel + min) / 2;
                }
                else
                {
                    min = seaLevel;
                    seaLevel = (seaLevel + max) / 2;
                }
            }

            // 맵 가장자리의 높이를 0으로 설정
            Queue<Corner> elevationFloodFill = new Queue<Corner>();
            foreach (Corner corner in corners)
            {
                if (corner.isBorder)
                {
                    corner.elevation = 0;
                    elevationFloodFill.Enqueue(corner);
                }
                else
                {
                    corner.elevation = float.MaxValue;
                }
            }

            // 맵 가장자리부터 땅의 높이를 증가시킴
            while (elevationFloodFill.Count > 0)
            {
                Corner currentCorner = elevationFloodFill.Dequeue();
                foreach (Corner neighborCorner in currentCorner.NeighborCorners)
                {
                    float newElevation = currentCorner.elevation + 0.01f;
                    if (currentCorner.isWater == false && neighborCorner.isWater == false)
                    {
                        newElevation += 1;
                    }

                    if (newElevation < neighborCorner.elevation)
                    {
                        neighborCorner.elevation = newElevation;
                        elevationFloodFill.Enqueue(neighborCorner);
                    }
                }
            }
        }

        private static void CalculateWaterGroundType(float lakeThreshold, ref Center[] centers, ref Corner[] corners)
        {
            Queue<Center> seaFloodFill = new Queue<Center>();
            foreach (Center center in centers)
            {
                int totalWaterCorner = 0;
                foreach (Corner neighborCorner in center.NeighborCorners)
                {
                    if (neighborCorner.isBorder)
                    {
                        center.isBorder = true;
                        center.isSea = true;
                        neighborCorner.isWater = true;
                        seaFloodFill.Enqueue(center);
                    }
                    if (neighborCorner.isWater)
                    {
                        totalWaterCorner++;
                    }
                }
                center.isWater = center.isSea || totalWaterCorner >= 6 * lakeThreshold;
            }

            while (seaFloodFill.Count > 0)
            {
                Center currentCenter = seaFloodFill.Dequeue();
                foreach (Center neighborCenter in currentCenter.NeighborCenters.Values)
                {
                    if (neighborCenter.isWater && neighborCenter.isSea == false)
                    {
                        neighborCenter.isSea = true;
                        seaFloodFill.Enqueue(neighborCenter);
                    }
                }
            }

            // 인접한 center가 바다와 땅이면 해변으로 설정
            foreach (Center center in centers)
            {
                bool surroundedBySea = false;
                bool surroundedByLand = false;
                foreach (Center neighborCenter in center.NeighborCenters.Values)
                {
                    if (neighborCenter.isSea)
                    {
                        surroundedBySea = true;
                    }
                    else if (neighborCenter.isWater == false)
                    {
                        surroundedByLand = true;
                    }
                }
                center.isCoast = surroundedBySea && surroundedByLand;
            }

            // 이웃한 center를 보고 corner의 물, 바다, 땅, 해변 설정
            foreach (Corner corner in corners)
            {
                int neighborSeaCount = 0;
                int neighborLandCount = 0;
                foreach (Center neighborCenter in corner.NeighborCenters)
                {
                    if (neighborCenter.isSea)
                    {
                        neighborSeaCount++;
                    }
                    else if (neighborCenter.isWater == false)
                    {
                        neighborLandCount++;
                    }
                }

                corner.isSea = neighborSeaCount > 0 && neighborLandCount == 0;
                corner.isCoast = neighborSeaCount > 0 && neighborLandCount > 0;
                corner.isWater = corner.isBorder || ((neighborLandCount != corner.NeighborCenters.Count) && corner.isCoast == false);
            }
        }

        private static void CalculateElevation(float scaleFactor, ref Center[] centers, ref Corner[] corners)
        {
            Corner[] sortedLandCorners = corners
                .Where(x => x.isSea == false && x.isCoast == false)
                .OrderBy(x => x.elevation)
                .ToArray();
            for (int i = 0; i < sortedLandCorners.Length; i++)
            {
                float y = i / (float)sortedLandCorners.Length;
                float x = Mathf.Sqrt(scaleFactor) - Mathf.Sqrt(scaleFactor * (1 - y));
                x = Mathf.Min(x, 1);
                sortedLandCorners[i].elevation = x;
            }

            // 바다와 해변의 높이를 0으로 설정
            foreach (Corner corner in corners)
            {
                if (corner.isSea || corner.isCoast)
                {
                    corner.elevation = 0;
                }
            }

            // 이웃한 corner 높이들의 평균으로 center의 높이를 계산
            foreach (Center center in centers)
            {
                center.elevation = center.NeighborCorners.Sum(x => x.elevation) / center.NeighborCorners.Count();
            }
        }

        private static void CalculateDownSlope(ref Corner[] corners)
        {
            foreach (Corner corner in corners)
            {
                Corner lowestCorner = corner;
                foreach (Corner neighborCorner in corner.NeighborCorners)
                {
                    if (neighborCorner.elevation <= lowestCorner.elevation)
                    {
                        lowestCorner = neighborCorner;
                    }
                }
                corner.downslope = lowestCorner;
            }
        }

        private static void CalculateRiver(int seed, int riverSpawnTry, Vector2 riverSpawnRange, ref Corner[] corners)
        {
            System.Random random = new System.Random(seed);
            for (int i = 0; i < riverSpawnTry; i++)
            {
                Corner randomCorner = corners[random.Next(corners.Length)];
                if (randomCorner.isSea || randomCorner.elevation < riverSpawnRange.x || randomCorner.elevation > riverSpawnRange.y)
                {
                    continue;
                }

                Corner riverCorner = randomCorner;
                while (riverCorner.isCoast == false && riverCorner.downslope != riverCorner)
                {
                    // 강이 호수 위로 지나가는것 방지
                    if (riverCorner.isWater && riverCorner.downslope.isWater)
                    {
                        break;
                    }

                    // edge.river
                    riverCorner.river += 1;
                    riverCorner.downslope.river += 1;
                    riverCorner = riverCorner.downslope;
                }
            }
        }

        private static void CalculateMoisture(ref Center[] centers, ref Corner[] corners)
        {
            Queue<Corner> moistureFloodFill = new Queue<Corner>();
            foreach (Corner corner in corners)
            {
                if (corner.isSea == false && (corner.isWater || corner.river > 0))
                {
                    corner.moisture = corner.river > 0 ? Mathf.Min(0.2f * corner.river, 3f) : 1f;
                    moistureFloodFill.Enqueue(corner);
                }
                else
                {
                    corner.moisture = 0;
                }
            }

            while (moistureFloodFill.Count > 0)
            {
                Corner corner = moistureFloodFill.Dequeue();
                foreach (Corner neighborCorner in corner.NeighborCorners)
                {
                    float newMoisture = corner.moisture * 0.9f;
                    if (newMoisture > neighborCorner.moisture)
                    {
                        neighborCorner.moisture = newMoisture;
                        moistureFloodFill.Enqueue(neighborCorner);
                    }
                }
            }

            foreach (Corner corner in corners)
            {
                if (corner.isSea || corner.isCoast)
                {
                    corner.moisture = 1f;
                }
            }

            Corner[] sortedLandCorners = corners
                .Where(x => x.isSea == false && x.isCoast == false)
                .OrderBy(x => x.moisture)
                .ToArray();
            for (int i = 0; i < sortedLandCorners.Length; i++)
            {
                sortedLandCorners[i].moisture = i / (float)(sortedLandCorners.Length - 1);
            }

            foreach (Center center in centers)
            {
                center.moisture = center.NeighborCorners.Sum(x => x.moisture) / center.NeighborCorners.Length;
            }
        }

        private static void CalculateBiome(BiomeTable biomeTable, ref Center[] centers)
        {
            foreach (Center center in centers)
            {
                Biome biome = biomeTable.EvaluateBiome(center.moisture, center.Temperature);
                center.biomeId = biome.id;
            }
        }

        //[Button]
        //public void GenerateTerrain()
        //{
        //    int width = mapSize.x;
        //    int height = mapSize.y;
        //    int mapLength = width * height;

        //    // 섬 노이즈
        //    islandShape.GenerateHexagonNoiseMap(width, height, out float[] islandNoiseMap);
        //    islandShapeFalloff.GenerateFalloffMap(width, height, out float[] falloffMap);
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        islandNoiseMap[i] *= falloffMap[i];
        //    }

        //    // 물 맵 생성
        //    GenerateWaterMap(seaLevel, ref islandNoiseMap, out bool[] waterMap);
        //    Color[] waterMapColors = waterMap.Select(x => x ? Color.white : Color.black).ToArray();
        //    UpdatePreviewTexture(width, height, waterMapRenderer, waterMapColors);

        //    // 바다, 호수, 육지, 해변 맵 생성
        //    GenerateNodeTypeMap(width, height, ref waterMap, out int[] nodeTypeMap);
        //    Color[] nodeTypeColors = nodeTypeMap.Select(x =>
        //    {
        //        switch ((NodeType)x)
        //        {
        //            case NodeType.Land:
        //                return Color.green;
        //            case NodeType.Coast:
        //                return Color.yellow;
        //            case NodeType.Sea:
        //                return Color.blue;
        //            case NodeType.Lake:
        //                return Color.cyan;
        //            default:
        //                return Color.black;
        //        }
        //    }).ToArray();
        //    UpdatePreviewTexture(width, height, nodeTypeMapRenderer, nodeTypeColors);

        //    // 높이 맵 생성
        //    GenerateHeightMap(width, height, ref waterMap, out float[] heightMap);
        //    Color[] heightMapColors = heightMap.Select(x => Color.Lerp(Color.black, Color.white, x)).ToArray();
        //    UpdatePreviewTexture(width, height, heightMapRenderer, heightMapColors);

        //    // 강 생성
        //    GenerateRiverMap(width, height, riverSeed, riverSpawnRange, ref nodeTypeMap, ref heightMap, out int[] riverMap);
        //    int maxRiverStrength = riverMap.Max();
        //    Color[] riverMapColors = riverMap.Select(x => x == 0 ? Color.black : Color.Lerp(Color.cyan, Color.blue, x / (float)maxRiverStrength)).ToArray();
        //    UpdatePreviewTexture(width, height, riverMapRenderer, riverMapColors);

        //    // 습도 맵 생성
        //    GenerateMoistureMap(width, height, ref nodeTypeMap, ref riverMap, out float[] moistureMap);
        //    Color[] moistureMapColors = moistureMap.Select(x => Color.Lerp(Color.black, Color.blue, x)).ToArray();
        //    UpdatePreviewTexture(width, height, moistureMapRenderer, moistureMapColors);

        //    // 온도 맵 생성
        //    GenerateTemperatureMap(width, height, ref nodeTypeMap, ref heightMap, out float[] temperatureMap);
        //    Color[] temperatureMapColors = temperatureMap.Select(x => x < 0 ? Color.black : Color.HSVToRGB(Mathf.Lerp(0.6667f, 0f, x), 1, 1)).ToArray();
        //    UpdatePreviewTexture(width, height, temperatureMapRenderer, temperatureMapColors);

        //    // 바이옴 맵 생성
        //    BiomeTable biomeTable = biomeTableSettings.GetBiomeTable();
        //    GenerateBiomeMap(width, height, biomeTable, ref nodeTypeMap, ref moistureMap, ref temperatureMap, out int[] biomeMap);
        //    Color[] biomeMapColors = biomeMap.Select(x => x < 0 ? Color.black : biomeTable.biomeDictionary[x].color).ToArray();
        //    UpdatePreviewTexture(width, height, biomeMapRenderer, biomeMapColors);

        //    // 프리뷰 월드 업데이트
        //    Color[] hexColors = new Color[mapLength];
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (waterMap[i])
        //        {
        //            if (nodeTypeMap[i] == (int)NodeType.Sea)
        //            {
        //                hexColors[i] = Color.Lerp(Color.black, Color.blue, heightMap[i]);
        //            }
        //            else
        //            {
        //                hexColors[i] = new Color(0.5f, 0.5f, 1f);
        //            }
        //        }
        //        else
        //        {
        //            hexColors[i] = biomeMapColors[i];
        //            if (riverMap[i] > 0)
        //            {
        //                hexColors[i] = Color.Lerp(new Color(0.5f, 0.5f, 1f), Color.blue, riverMap[i] / (float)maxRiverStrength);
        //            }
        //        }
        //    }
        //    previewWorld.GenerateDefaultHexagons(mapSize);
        //    previewWorld.SetHexagonColorMap(width, height, ref hexColors);
        //    previewWorld.SetHexagonsElevation(ref heightMap, heightMultiplier);
        //}

        //private void GenerateWaterMap(float seaLevel, ref float[] islandNoiseMap, out bool[] waterMap)
        //{
        //    waterMap = islandNoiseMap
        //        .Select(x => x <= seaLevel)
        //        .ToArray();
        //}

        //private void GenerateNodeTypeMap(int width, int height, ref bool[] waterMap, out int[] nodeTypeMap)
        //{
        //    int mapLength = width * height;
        //    nodeTypeMap = new int[mapLength];

        //    // 맵 경계는 바다
        //    Queue<HexagonPos> floodFillQueue = new Queue<HexagonPos>();
        //    for (int x = 0; x < width; x++)
        //    {
        //        nodeTypeMap[x] = (int)NodeType.Sea;
        //        nodeTypeMap[x + (height - 1) * width] = (int)NodeType.Sea;
        //        floodFillQueue.Enqueue(HexagonPos.FromArrayXY(x, 0));
        //        floodFillQueue.Enqueue(HexagonPos.FromArrayXY(x, height - 1));
        //    }
        //    for (int y = 1; y < height - 1; y++)
        //    {
        //        nodeTypeMap[y * width] = (int)NodeType.Sea;
        //        nodeTypeMap[width - 1 + y * width] = (int)NodeType.Sea;
        //        floodFillQueue.Enqueue(HexagonPos.FromArrayXY(0, y));
        //        floodFillQueue.Enqueue(HexagonPos.FromArrayXY(width - 1, y));
        //    }

        //    // flood fill으로 바다 채우기
        //    while (floodFillQueue.Count > 0)
        //    {
        //        HexagonPos hexPos = floodFillQueue.Dequeue();
        //        for (int hexZ = -1; hexZ <= 1; hexZ++)
        //        {
        //            for (int hexX = -1; hexX <= 1; hexX++)
        //            {
        //                if (hexX == hexZ)
        //                {
        //                    continue;
        //                }

        //                HexagonPos neighborHexPos = hexPos + new HexagonPos(hexX, hexZ);
        //                Vector2Int neighborXY = neighborHexPos.ToArrayXY();
        //                if (neighborXY.x < 0 || neighborXY.x >= width || neighborXY.y < 0 || neighborXY.y >= height)
        //                {
        //                    continue;
        //                }

        //                int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                if (waterMap[neighborIndex] == true && nodeTypeMap[neighborIndex] != (int)NodeType.Sea)
        //                {
        //                    nodeTypeMap[neighborIndex] = (int)NodeType.Sea;
        //                    floodFillQueue.Enqueue(neighborHexPos);
        //                }
        //            }
        //        }
        //    }

        //    // 호수, 해변, 땅 생성
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int currentIndex = x + y * width;
        //            if (nodeTypeMap[currentIndex] == (int)NodeType.Sea)
        //            {
        //                continue;
        //            }

        //            if (waterMap[currentIndex] == true)
        //            {
        //                if (nodeTypeMap[currentIndex] != (int)NodeType.Sea)
        //                {
        //                    nodeTypeMap[x + y * width] = (int)NodeType.Lake;
        //                }
        //                continue;
        //            }

        //            HexagonPos hexPos = HexagonPos.FromArrayXY(x, y);
        //            bool surroundedBySea = false;
        //            for (int hexZ = -1; hexZ <= 1; hexZ++)
        //            {
        //                for (int hexX = -1; hexX <= 1; hexX++)
        //                {
        //                    if (hexX == hexZ)
        //                    {
        //                        continue;
        //                    }

        //                    HexagonPos neighborHexPos = hexPos + new HexagonPos(hexX, hexZ);
        //                    Vector2Int neighborXY = neighborHexPos.ToArrayXY();
        //                    int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                    if (nodeTypeMap[neighborIndex] == (int)NodeType.Sea)
        //                    {
        //                        surroundedBySea = true;
        //                    }
        //                }
        //            }

        //            if (surroundedBySea)
        //            {
        //                nodeTypeMap[x + y * width] = (int)NodeType.Coast;
        //            }
        //            else
        //            {
        //                nodeTypeMap[x + y * width] = (int)NodeType.Land;
        //            }
        //        }
        //    }
        //}

        //private void GenerateHeightMap(int width, int height, ref bool[] waterMap, out float[] heightMap)
        //{
        //    int mapLength = width * height;
        //    heightMap = new float[mapLength];
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        heightMap[i] = float.MaxValue;
        //    }

        //    // 맵 경계의 높이는 가장 낮음
        //    Queue<HexagonPos> queue = new Queue<HexagonPos>();
        //    for (int x = 0; x < width; x++)
        //    {
        //        heightMap[x] = 0;
        //        heightMap[x + (height - 1) * width] = 0;
        //        queue.Enqueue(HexagonPos.FromArrayXY(x, 0));
        //        queue.Enqueue(HexagonPos.FromArrayXY(x, height - 1));
        //    }
        //    for (int y = 1; y < height - 1; y++)
        //    {
        //        heightMap[y * width] = 0;
        //        heightMap[width - 1 + y * width] = 0;
        //        queue.Enqueue(HexagonPos.FromArrayXY(0, y));
        //        queue.Enqueue(HexagonPos.FromArrayXY(width - 1, y));
        //    }

        //    while (queue.Count > 0)
        //    {
        //        HexagonPos currentHexPos = queue.Dequeue();
        //        Vector2Int currentXY = currentHexPos.ToArrayXY();
        //        int currentIndex = currentXY.x + currentXY.y * width;
        //        for (int hexZ = -1; hexZ <= 1; hexZ++)
        //        {
        //            for (int hexX = -1; hexX <= 1; hexX++)
        //            {
        //                if (hexX == hexZ)
        //                {
        //                    continue;
        //                }

        //                HexagonPos neighborHexPos = currentHexPos + new HexagonPos(hexX, hexZ);
        //                Vector2Int neighborXY = neighborHexPos.ToArrayXY();
        //                if (neighborXY.x < 0 || neighborXY.x >= width || neighborXY.y < 0 || neighborXY.y >= height)
        //                {
        //                    continue;
        //                }

        //                int neighborIndex = neighborXY.x + neighborXY.y * width;

        //                float newHeight = heightMap[currentIndex] + 0.01f;
        //                if (waterMap[currentIndex] == false && waterMap[neighborIndex] == false)
        //                {
        //                    newHeight += 1;
        //                    // add more randomness
        //                }

        //                if (newHeight < heightMap[neighborIndex])
        //                {
        //                    heightMap[neighborIndex] = newHeight;
        //                    queue.Enqueue(neighborHexPos);
        //                }
        //            }
        //        }
        //    }

        //    // redistribute
        //    float scaleFactor = 1.1f;
        //    var sortKV = new KeyValuePair<int, float>[mapLength];
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        sortKV[i] = new KeyValuePair<int, float>(i, heightMap[i]);
        //    }

        //    sortKV.Sort((x, y) => x.Value.CompareTo(y.Value));
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        float y = i / (float)(mapLength - 1);
        //        float x = Mathf.Sqrt(scaleFactor) - Mathf.Sqrt(scaleFactor * (1 - y));
        //        x = Mathf.Min(x, 1);
        //        heightMap[sortKV[i].Key] = x;
        //    }
        //}

        //private void GenerateRiverMap(int width, int height, int seed, Vector2 riverSpawnRange, ref int[] nodeTypeMap, ref float[] heightMap, out int[] riverMap)
        //{
        //    int mapLength = width * height;
        //    riverMap = new int[mapLength];

        //    System.Random random = new System.Random(seed);

        //    int maxSpawnTry = (width + height) / 2;
        //    for (int i = 0; i < maxSpawnTry; i++)
        //    {
        //        int spawnX = random.Next(width);
        //        int spawnY = random.Next(height);
        //        int spawnIndex = spawnX + spawnY * width;

        //        float spawnHeight = heightMap[spawnIndex];
        //        if (nodeTypeMap[spawnIndex] == (int)NodeType.Sea
        //            || nodeTypeMap[spawnIndex] == (int)NodeType.Lake
        //            || spawnHeight < riverSpawnRange.x
        //            || spawnHeight > riverSpawnRange.y)
        //        {
        //            continue;
        //        }

        //        // if not water
        //        // increase river value
        //        // find next lowland
        //        // repeat

        //        HexagonPos currentHexPos = HexagonPos.FromArrayXY(spawnX, spawnY);
        //        Vector2Int currentXY = currentHexPos.ToArrayXY();
        //        int currentIndex = currentXY.x + currentXY.y * width;
        //        int previousRiverValue = 0;
        //        while (nodeTypeMap[currentIndex] == (int)NodeType.Land || nodeTypeMap[currentIndex] == (int)NodeType.Coast)
        //        {
        //            riverMap[currentIndex] += previousRiverValue + 1;
        //            previousRiverValue = riverMap[currentIndex];

        //            HexagonPos lowlandHexPos = new HexagonPos(1, 1);
        //            float lowestHeight = float.MaxValue;
        //            for (int hexZ = -1; hexZ <= 1; hexZ++)
        //            {
        //                for (int hexX = -1; hexX <= 1; hexX++)
        //                {
        //                    if (hexX == hexZ)
        //                    {
        //                        continue;
        //                    }

        //                    HexagonPos neighborHexPos = currentHexPos + new HexagonPos(hexX, hexZ);
        //                    Vector2Int neighborXY = neighborHexPos.ToArrayXY();
        //                    int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                    if (heightMap[neighborIndex] < lowestHeight)
        //                    {
        //                        lowestHeight = heightMap[neighborIndex];
        //                        lowlandHexPos = neighborHexPos;
        //                    }
        //                }
        //            }

        //            currentHexPos = lowlandHexPos;
        //            currentXY = currentHexPos.ToArrayXY();
        //            currentIndex = currentXY.x + currentXY.y * width;
        //        }
        //    }
        //}

        //private void GenerateMoistureMap(int width, int height, ref int[] nodeTypeMap, ref int[] riverMap, out float[] moistureMap)
        //{
        //    int mapLength = width * height;
        //    moistureMap = new float[mapLength];

        //    Queue<HexagonPos> queue = new Queue<HexagonPos>();
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            HexagonPos hexPos = HexagonPos.FromArrayXY(x, y);
        //            int index = x + y * width;
        //            if (nodeTypeMap[index] == (int)NodeType.Lake)
        //            {
        //                moistureMap[index] = 1;
        //                queue.Enqueue(hexPos);
        //            }
        //            else if (riverMap[index] > 0)
        //            {
        //                moistureMap[index] = Mathf.Min(riverMap[index] * 0.2f, 2f);
        //                queue.Enqueue(hexPos);
        //            }
        //        }
        //    }

        //    while (queue.Count > 0)
        //    {
        //        HexagonPos currentHexPos = queue.Dequeue();
        //        Vector2Int currentXY = currentHexPos.ToArrayXY();
        //        int currentIndex = currentXY.x + currentXY.y * width;
        //        for (int hexZ = -1; hexZ <= 1; hexZ++)
        //        {
        //            for (int hexX = -1; hexX <= 1; hexX++)
        //            {
        //                if (hexX == hexZ)
        //                {
        //                    continue;
        //                }

        //                HexagonPos neighborHexPos = currentHexPos + new HexagonPos(hexX, hexZ);
        //                Vector2Int neighborXY = neighborHexPos.ToArrayXY();
        //                if (neighborXY.x < 0 || neighborXY.x > width - 1
        //                    || neighborXY.y < 0 || neighborXY.y > height - 1)
        //                {
        //                    continue;
        //                }

        //                int neighborIndex = neighborXY.x + neighborXY.y * width;
        //                if (nodeTypeMap[neighborIndex] != (int)NodeType.Land && nodeTypeMap[neighborIndex] != (int)NodeType.Coast)
        //                {
        //                    continue;
        //                }

        //                float newMoisture = moistureMap[currentIndex] * 0.9f;
        //                if (newMoisture > moistureMap[neighborIndex])
        //                {
        //                    moistureMap[neighborIndex] = newMoisture;
        //                    queue.Enqueue(neighborHexPos);
        //                }
        //            }
        //        }
        //    }

        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (nodeTypeMap[i] == (int)NodeType.Coast)
        //        {
        //            moistureMap[i] = Mathf.Min(moistureMap[i] * 1.5f, 1f);
        //        }
        //    }

        //    // TODO: smoothing 필터 적용해보면 좋을 듯

        //    // redistribute
        //    var sortKV = new List<KeyValuePair<int, float>>();
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (moistureMap[i] > 0)
        //        {
        //            sortKV.Add(new KeyValuePair<int, float>(i, moistureMap[i]));
        //        }
        //    }
        //    sortKV.Sort((x, y) => x.Value.CompareTo(y.Value));
        //    for (int i = 0; i < sortKV.Count; i++)
        //    {
        //        moistureMap[sortKV[i].Key] = i / (float)(sortKV.Count - 1);
        //    }
        //}

        //private void GenerateTemperatureMap(int width, int height, ref int[] nodeTypeMap, ref float[] heightMap, out float[] temperatureMap)
        //{
        //    int mapLength = width * height;
        //    temperatureMap = new float[mapLength];

        //    float minTemperature = float.MaxValue;
        //    float maxTemperature = float.MinValue;
        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (nodeTypeMap[i] == (int)NodeType.Sea)
        //        {
        //            temperatureMap[i] = -1;
        //        }
        //        else
        //        {
        //            float temperature = 1 - heightMap[i];
        //            temperatureMap[i] = temperature;
        //            minTemperature = Mathf.Min(temperature, minTemperature);
        //            maxTemperature = Mathf.Max(temperature, maxTemperature);
        //        }
        //    }

        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (temperatureMap[i] > 0)
        //        {
        //            temperatureMap[i] = Mathf.InverseLerp(minTemperature, maxTemperature, temperatureMap[i]);
        //        }
        //    }
        //}

        //private void GenerateBiomeMap(int width, int height, BiomeTable biomeTable, ref int[] nodeTypeMap, ref float[] moistureMap, ref float[] temperatureMap, out int[] biomeMap)
        //{
        //    int mapLength = width * height;
        //    biomeMap = new int[mapLength];

        //    for (int i = 0; i < mapLength; i++)
        //    {
        //        if (nodeTypeMap[i] == (int)NodeType.Sea)
        //        {
        //            biomeMap[i] = -1;
        //        }
        //        else
        //        {
        //            BiomeTable.Biome biome = biomeTable.EvaluateBiome(moistureMap[i], temperatureMap[i]);
        //            biomeMap[i] = biome.id;
        //        }
        //    }
        //}

        //private void UpdatePreviewTexture(int width, int height, MeshRenderer renderer, Color[] colors)
        //{
        //    Texture2D texture = new Texture2D(width, height)
        //    {
        //        filterMode = FilterMode.Point
        //    };

        //    texture.SetPixels(colors);
        //    texture.Apply();

        //    var properties = new MaterialPropertyBlock();
        //    properties.SetTexture("_Texture", texture);
        //    renderer.SetPropertyBlock(properties);
        //}
    }
}
