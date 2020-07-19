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
    public class NoiseMapGenerator : MonoBehaviour
    {
        [Required]
        public ComputeShader perlinNoiseShader;
        [Required]
        public ComputeShader simplexNoiseShader;

        [Title("Noise")]
        [EnumToggleButtons]
        public NoiseType noiseType;
        [FoldoutGroup("Noise options"), HideLabel]
        public NoiseSettings settings;

        [TitleGroup("Debug")]
        public TerrainGenerator terrainGenerator;
        public bool autoUpdateTerrain;
        public MeshRenderer previewRenderer;
        public bool updatePreview;

        public enum NoiseType
        {
            Perlin, Simplex
        }

        private void OnValidate()
        {
            if (autoUpdateTerrain)
            {
                if (terrainGenerator != null)
                {
                    terrainGenerator.hasParameterUpdated = true;
                }
            }
        }

        public void GenerateNoiseMap(int width, int height, ComputeBuffer noiseMapBuffer)
        {
            ComputeShader noiseShader = GetNoiseShader(noiseType);

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

            octaveOffsetsBuffer.Release();
        }

        public void GenerateNoiseMap(int width, int height, out float[] noiseMap)
        {
            Profiler.BeginSample(nameof(GenerateNoiseMap));

            int noiseBufferSize = width * height;
            ComputeBuffer noiseMapBuffer = new ComputeBuffer(noiseBufferSize, sizeof(float));
            GenerateNoiseMap(width, height, noiseMapBuffer);

            noiseMap = new float[noiseBufferSize];
            noiseMapBuffer.GetData(noiseMap);
            noiseMapBuffer.Release();

            Profiler.EndSample();

            if (updatePreview)
            {
                UpdatePreviewTexture(width, height, ref noiseMap);
            }
        }

        private ComputeShader GetNoiseShader(NoiseType noiseType)
        {
            switch (noiseType)
            {
                case NoiseType.Perlin:
                    return perlinNoiseShader;
                case NoiseType.Simplex:
                    return simplexNoiseShader;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void UpdatePreviewTexture(int width, int height, ref float[] noiseMap)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            Color[] colors = noiseMap
                .Select(x => Color.Lerp(Color.black, Color.white, x))
                .ToArray();
            texture.SetPixels(colors);
            texture.Apply();

            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_Texture", texture);
            previewRenderer.SetPropertyBlock(properties);
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
