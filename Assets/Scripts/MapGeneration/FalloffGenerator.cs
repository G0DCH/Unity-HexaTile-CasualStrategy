using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class FalloffGenerator : MonoBehaviour
    {
        [Required]
        public ComputeShader falloffMap;
        [Required]
        public ComputeShader vectorFalloff;
        public Vector2 falloffParameter = new Vector2(3f, 2.2f);

        [Title("Debug")]
        public TerrainGenerator terrainGenerator;
        public bool autoUpdateTerrain;

        private void OnValidate()
        {
            if (autoUpdateTerrain && terrainGenerator != null)
            {
                terrainGenerator.hasParameterUpdated = true;
            }
        }

        public void GenerateFalloffMap(int width, int height, ComputeBuffer falloffMapBuffer)
        {
            float maxX = (width - 1) * Hexagon.Size + (Hexagon.Size / 2);
            float maxY = (height - 1) * Hexagon.Size * Mathf.Sin(Mathf.PI / 3);

            falloffMap.SetBuffer(0, "falloffMap", falloffMapBuffer);
            falloffMap.SetInt("mapWidth", width);
            falloffMap.SetInt("mapHeight", height);
            falloffMap.SetFloat("maxX", maxX);
            falloffMap.SetFloat("maxY", maxY);
            falloffMap.SetVector("falloffParameter", falloffParameter);
            falloffMap.SetFloat("hexagonSize", Hexagon.Size);

            int threadGroupsX = Mathf.CeilToInt(width / 16f);
            int threadGroupsY = Mathf.CeilToInt(height / 16f);
            falloffMap.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        }

        public void GenerateFalloffMap(int width, int height, out float[] falloffMap)
        {
            Profiler.BeginSample(nameof(GenerateFalloffMap));

            int bufferSize = width * height;
            ComputeBuffer falloffMapBuffer = new ComputeBuffer(bufferSize, sizeof(float));
            GenerateFalloffMap(width, height, falloffMapBuffer);

            falloffMap = new float[bufferSize];
            falloffMapBuffer.GetData(falloffMap);
            falloffMapBuffer.Release();

            Profiler.EndSample();
        }

        private void EvaluateFalloff(int width, int height, ComputeBuffer samplePointsBuffer, ComputeBuffer resultsBuffer, int totalPoints)
        {
            float maxX = (width - 1) * Hexagon.Size + (Hexagon.Size / 2);
            float maxY = (height - 1) * Hexagon.Size * Mathf.Sin(Mathf.PI / 3);

            vectorFalloff.SetBuffer(0, "samplePoints", samplePointsBuffer);
            vectorFalloff.SetBuffer(0, "results", resultsBuffer);

            vectorFalloff.SetInt("totalPoints", totalPoints);
            vectorFalloff.SetFloat("maxX", maxX);
            vectorFalloff.SetFloat("maxY", maxY);
            vectorFalloff.SetVector("falloffParameter", falloffParameter);

            int threadGroupsX = Mathf.CeilToInt(totalPoints / 16f);
            vectorFalloff.Dispatch(0, threadGroupsX, 1, 1);
        }

        public void EvaluateFalloff(int width, int height, ref Vector2[] samplePoints, out float[] results)
        {
            Profiler.BeginSample(nameof(EvaluateFalloff));

            int totalPoints = samplePoints.Length;
            ComputeBuffer samplePointsBuffer = new ComputeBuffer(totalPoints, sizeof(float) * 2);
            ComputeBuffer resultsBuffer = new ComputeBuffer(totalPoints, sizeof(float));
            samplePointsBuffer.SetData(samplePoints);

            EvaluateFalloff(width, height, samplePointsBuffer, resultsBuffer, totalPoints);

            results = new float[totalPoints];
            resultsBuffer.GetData(results);
            samplePointsBuffer.Release();
            resultsBuffer.Release();

            Profiler.EndSample();
        }
    }
}
