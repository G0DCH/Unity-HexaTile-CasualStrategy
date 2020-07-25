using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilePuzzle.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class NoiseGenerator : Singleton<NoiseGenerator>
    {
        [Required]
        public ComputeShader hexagonNoiseMap;
        [Required]
        public ComputeShader vectorNoise;

        private Queue<ComputeBuffer> bufferToRelease;

        public void GenerateHexagonNoiseMap(int width, int height, ComputeBuffer noiseMapBuffer, int globalSeed, NoiseSettings settings)
        {
            if (bufferToRelease == null)
            {
                bufferToRelease = new Queue<ComputeBuffer>();
            }

            ComputeBuffer octaveOffsetsBuffer = new ComputeBuffer(settings.octaves, sizeof(float) * 3);
            bufferToRelease.Enqueue(octaveOffsetsBuffer);
            Vector3[] octaveOffsets = CalculateOctaveOffsets(globalSeed, settings);
            octaveOffsetsBuffer.SetData(octaveOffsets);

            int kernelIndex = GetKernelIndex(settings.noiseType);
            hexagonNoiseMap.SetBuffer(kernelIndex, "noiseMap", noiseMapBuffer);
            hexagonNoiseMap.SetBuffer(kernelIndex, "octaveOffsets", octaveOffsetsBuffer);

            hexagonNoiseMap.SetInt("mapWidth", width);
            hexagonNoiseMap.SetInt("mapHeight", height);
            hexagonNoiseMap.SetFloat("hexagonSize", Hexagon.Size);

            hexagonNoiseMap.SetBool("isRigid", settings.isRigid);
            hexagonNoiseMap.SetVector("offset", settings.offset);
            hexagonNoiseMap.SetInt("octaves", settings.octaves);
            hexagonNoiseMap.SetFloat("lacunarity", settings.lacunarity);
            hexagonNoiseMap.SetFloat("persistance", settings.persistance);
            hexagonNoiseMap.SetFloat("scale", settings.scale);
            hexagonNoiseMap.SetFloat("strength", settings.strength);
            hexagonNoiseMap.SetFloat("weightMultiplier", settings.weightMultiplier);

            int threadGroupsX = Mathf.CeilToInt(width / 16f);
            int threadGroupsY = Mathf.CeilToInt(height / 16f);
            hexagonNoiseMap.Dispatch(kernelIndex, threadGroupsX, threadGroupsY, 1);
        }

        public void GenerateHexagonNoiseMap(int width, int height, out float[] noiseMap, int globalSeed, NoiseSettings settings)
        {
            Profiler.BeginSample(nameof(GenerateHexagonNoiseMap));

            int noiseBufferSize = width * height;
            ComputeBuffer noiseMapBuffer = new ComputeBuffer(noiseBufferSize, sizeof(float));
            GenerateHexagonNoiseMap(width, height, noiseMapBuffer, globalSeed, settings);

            noiseMap = new float[noiseBufferSize];
            noiseMapBuffer.GetData(noiseMap);
            noiseMapBuffer.Release();
            while (bufferToRelease.Count > 0)
            {
                bufferToRelease.Dequeue().Release();
            }

            Profiler.EndSample();
        }

        public void EvaluateNoise(ComputeBuffer samplePointsBuffer, ComputeBuffer resultsBuffer, int totalPoints, int globalSeed, NoiseSettings settings)
        {
            if (bufferToRelease == null)
            {
                bufferToRelease = new Queue<ComputeBuffer>();
            }

            ComputeBuffer octaveOffsetsBuffer = new ComputeBuffer(settings.octaves, sizeof(float) * 3);
            bufferToRelease.Enqueue(octaveOffsetsBuffer);
            Vector3[] octaveOffsets = CalculateOctaveOffsets(globalSeed, settings);
            octaveOffsetsBuffer.SetData(octaveOffsets);

            int kernelIndex = GetKernelIndex(settings.noiseType);
            vectorNoise.SetBuffer(kernelIndex, "samplePoints", samplePointsBuffer);
            vectorNoise.SetBuffer(kernelIndex, "results", resultsBuffer);
            vectorNoise.SetBuffer(kernelIndex, "octaveOffsets", octaveOffsetsBuffer);

            vectorNoise.SetInt("totalPoints", totalPoints);

            vectorNoise.SetBool("isRigid", settings.isRigid);
            vectorNoise.SetVector("offset", settings.offset);
            vectorNoise.SetInt("octaves", settings.octaves);
            vectorNoise.SetFloat("lacunarity", settings.lacunarity);
            vectorNoise.SetFloat("persistance", settings.persistance);
            vectorNoise.SetFloat("scale", settings.scale);
            vectorNoise.SetFloat("strength", settings.strength);
            vectorNoise.SetFloat("weightMultiplier", settings.weightMultiplier);

            int threadGroupsX = Mathf.CeilToInt(totalPoints / 32f);
            vectorNoise.Dispatch(kernelIndex, threadGroupsX, 1, 1);
        }

        public void EvaluateNoise(ref Vector2[] samplePoints, out float[] results, int globalSeed, NoiseSettings settings)
        {
            Profiler.BeginSample(nameof(EvaluateNoise));

            int totalPoints = samplePoints.Length;
            ComputeBuffer samplePointsBuffer = new ComputeBuffer(totalPoints, sizeof(float) * 2);
            ComputeBuffer resultsBuffer = new ComputeBuffer(totalPoints, sizeof(float));
            samplePointsBuffer.SetData(samplePoints);

            EvaluateNoise(samplePointsBuffer, resultsBuffer, totalPoints, globalSeed, settings);

            results = new float[totalPoints];
            resultsBuffer.GetData(results);
            samplePointsBuffer.Release();
            resultsBuffer.Release();
            while (bufferToRelease.Count > 0)
            {
                bufferToRelease.Dequeue().Release();
            }

            Profiler.EndSample();
        }

        private Vector3[] CalculateOctaveOffsets(int globalSeed, NoiseSettings settings)
        {
            System.Random random = new System.Random(globalSeed + settings.seed);
            Vector3[] octaveOffsets = new Vector3[settings.octaves];
            float offsetRange = 1000;
            for (int i = 0; i < settings.octaves; i++)
            {
                octaveOffsets[i] = new Vector3(
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1,
                    (float)random.NextDouble() * 2 - 1) * offsetRange;
            }

            return octaveOffsets;
        }

        private int GetKernelIndex(NoiseSettings.NoiseType noiseType)
        {
            switch (noiseType)
            {
                case NoiseSettings.NoiseType.Perlin:
                    return 0;
                case NoiseSettings.NoiseType.Simplex:
                    return 1;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
