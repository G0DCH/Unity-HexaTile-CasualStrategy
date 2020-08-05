using Sirenix.OdinInspector;
using TilePuzzle.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class FalloffGenerator : Singleton<FalloffGenerator>
    {
        [Required]
        public ComputeShader falloffMap;
        [Required]
        public ComputeShader vectorFalloff;

        public void GenerateFalloffMap(int width, int height, ComputeBuffer falloffMapBuffer, FalloffSettings settings)
        {
            float maxX = (width - 1) * HexagonObject.Size + (HexagonObject.Size / 2);
            float maxY = (height - 1) * HexagonObject.Size * Mathf.Sin(Mathf.PI / 3);

            falloffMap.SetBuffer(0, "falloffMap", falloffMapBuffer);
            falloffMap.SetInt("mapWidth", width);
            falloffMap.SetInt("mapHeight", height);
            falloffMap.SetFloat("maxX", maxX);
            falloffMap.SetFloat("maxY", maxY);
            falloffMap.SetVector("falloffParameter", settings.falloffParameter);
            falloffMap.SetFloat("hexagonSize", HexagonObject.Size);

            int threadGroupsX = Mathf.CeilToInt(width / 16f);
            int threadGroupsY = Mathf.CeilToInt(height / 16f);
            falloffMap.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        }

        public void GenerateFalloffMap(int width, int height, out float[] falloffMap, FalloffSettings settings)
        {
            Profiler.BeginSample(nameof(GenerateFalloffMap));

            int bufferSize = width * height;
            ComputeBuffer falloffMapBuffer = new ComputeBuffer(bufferSize, sizeof(float));
            GenerateFalloffMap(width, height, falloffMapBuffer, settings);

            falloffMap = new float[bufferSize];
            falloffMapBuffer.GetData(falloffMap);
            falloffMapBuffer.Release();

            Profiler.EndSample();
        }

        private void EvaluateFalloff(int width, int height, ComputeBuffer samplePointsBuffer, ComputeBuffer resultsBuffer, int totalPoints, FalloffSettings settings)
        {
            float maxX = (width - 1) * HexagonObject.Size + (HexagonObject.Size / 2);
            float maxY = (height - 1) * HexagonObject.Size * Mathf.Sin(Mathf.PI / 3);

            vectorFalloff.SetBuffer(0, "samplePoints", samplePointsBuffer);
            vectorFalloff.SetBuffer(0, "results", resultsBuffer);

            vectorFalloff.SetInt("totalPoints", totalPoints);
            vectorFalloff.SetFloat("maxX", maxX);
            vectorFalloff.SetFloat("maxY", maxY);
            vectorFalloff.SetVector("falloffParameter", settings.falloffParameter);

            int threadGroupsX = Mathf.CeilToInt(totalPoints / 16f);
            vectorFalloff.Dispatch(0, threadGroupsX, 1, 1);
        }

        public void EvaluateFalloff(int width, int height, ref Vector2[] samplePoints, out float[] results, FalloffSettings settings)
        {
            Profiler.BeginSample(nameof(EvaluateFalloff));

            int totalPoints = samplePoints.Length;
            ComputeBuffer samplePointsBuffer = new ComputeBuffer(totalPoints, sizeof(float) * 2);
            ComputeBuffer resultsBuffer = new ComputeBuffer(totalPoints, sizeof(float));
            samplePointsBuffer.SetData(samplePoints);

            EvaluateFalloff(width, height, samplePointsBuffer, resultsBuffer, totalPoints, settings);

            results = new float[totalPoints];
            resultsBuffer.GetData(results);
            samplePointsBuffer.Release();
            resultsBuffer.Release();

            Profiler.EndSample();
        }
    }
}
