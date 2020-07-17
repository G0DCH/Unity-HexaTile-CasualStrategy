using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle
{
    public class NoiseGenerator : MonoBehaviour
    {
        [Required]
        public ComputeShader noiseShader;
        public NoiseSettings settings;
        public bool autoUpdate;

        private MapGenerator mapGenerator;

        private void OnValidate()
        {
            if (autoUpdate)
            {
                if (mapGenerator == null)
                {
                    mapGenerator = FindObjectOfType<MapGenerator>();
                }
                if (mapGenerator != null)
                {
                    mapGenerator.hasParameterUpdated = true;
                }
            }
        }

        public void GenerateNoiseMap(int width, int height, out float[] noiseMap)
        {
            Profiler.BeginSample(nameof(GenerateNoiseMap));

            System.Random random = new System.Random(settings.seed);
            Vector3[] octaveOffsets = new Vector3[settings.octaves];
            float offsetRange = 1000;
            for (int i = 0; i < settings.octaves; i++)
            {
                octaveOffsets[i] = new Vector3(
                    (float)random.NextDouble() * 2 - 1, 
                    (float)random.NextDouble() * 2 - 1, 
                    (float)random.NextDouble() * 2 - 1) * offsetRange;
            }

            int noiseBufferSize = width * height;
            ComputeBuffer noiseMapBuffer = new ComputeBuffer(noiseBufferSize, sizeof(float));
            ComputeBuffer octaveOffsetsBuffer = new ComputeBuffer(octaveOffsets.Length, sizeof(float) * 3);
            octaveOffsetsBuffer.SetData(octaveOffsets);

            noiseShader.SetBuffer(0, "noiseMap", noiseMapBuffer);
            noiseShader.SetBuffer(0, "octaveOffsets", octaveOffsetsBuffer);

            noiseShader.SetInt("mapWidth", width);
            noiseShader.SetInt("mapHeight", height);
            noiseShader.SetFloat("hexagonSize", Hexagon.Size);

            noiseShader.SetVector("offset", settings.offset);
            noiseShader.SetInt("octaves", settings.octaves);
            noiseShader.SetFloat("lacunarity", settings.lacunarity);
            noiseShader.SetFloat("persistance", settings.persistance);
            noiseShader.SetFloat("scale", settings.scale);
            noiseShader.SetFloat("strength", settings.strength);
            noiseShader.SetFloat("weightMultiplier", settings.weightMultiplier);

            int threadGroupsX = Mathf.CeilToInt(width / 16f);
            int threadGroupsY = Mathf.CeilToInt(height / 16f);
            noiseShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            noiseMap = new float[noiseBufferSize];
            noiseMapBuffer.GetData(noiseMap);

            octaveOffsetsBuffer.Release();
            noiseMapBuffer.Release();

            Profiler.EndSample();
        }

        [Serializable]
        public class NoiseSettings
        {
            public int seed;
            public Vector3 offset;
            [Range(1, 10)] public int octaves = 4;
            [Min(0)] public float lacunarity = 2;
            [Min(0)] public float persistance = 0.5f;
            [Min(0)] public float scale = 1;
            [Min(0)] public float strength = 1;
            [Min(0)] public float weightMultiplier = 1;
        }
    }
}
