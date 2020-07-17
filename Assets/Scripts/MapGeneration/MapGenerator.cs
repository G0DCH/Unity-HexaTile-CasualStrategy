using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle
{
    [ExecuteInEditMode]
    public class MapGenerator : MonoBehaviour
    {
        [Title("Map settings")]
        public Vector2Int mapSize;

        [Title("Noise")]
        public ComputeShader noiseMapShader;
        public NoiseSettings noiseSettings;

        [Title("Debug")]
        public bool autoUpdate;
        public MeshRenderer previewRenderer;
        public World world;

        private void OnValidate()
        {
            if (autoUpdate)
            {
                RebuildPreview();
            }
        }

        [Button]
        private void RebuildPreview()
        {
            float[] noiseMap = GenerateNoiseMap(noiseSettings);
            world.Test(noiseMap);
            UpdatePreviewTexture(noiseMap);
        }

        private void UpdatePreviewTexture(float[] noiseMap)
        {
            Texture2D texture = new Texture2D(mapSize.x, mapSize.y)
            {
                filterMode = FilterMode.Point
            };

            Color[] colors = noiseMap.Select(x => Color.Lerp(Color.black, Color.white, x)).ToArray();
            texture.SetPixels(colors);
            texture.Apply();

            var propertyBlock =  new MaterialPropertyBlock();
            propertyBlock.SetTexture("_Texture", texture);
            previewRenderer.SetPropertyBlock(propertyBlock);
        }

        private float[] GenerateNoiseMap(NoiseSettings noiseSettings)
        {
            var mapBuffer = new ComputeBuffer(mapSize.x * mapSize.y, sizeof(float));
            noiseMapShader.SetBuffer(0, "map", mapBuffer);
            noiseMapShader.SetInt("mapWidth", mapSize.x);
            noiseMapShader.SetInt("mapHeight", mapSize.y);

            var random = new System.Random(noiseSettings.seed);
            Vector3[] offsets = new Vector3[noiseSettings.octaves];
            float offsetRange = 1000;
            for (int i = 0; i < noiseSettings.octaves; i++)
            {
                offsets[i] = new Vector3((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1) * offsetRange;
            }
            ComputeBuffer offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
            offsetsBuffer.SetData(offsets);
            noiseMapShader.SetBuffer(0, "offsets", offsetsBuffer);

            noiseMapShader.SetVector("offset", noiseSettings.offset);
            noiseMapShader.SetInt("octaves", noiseSettings.octaves);
            noiseMapShader.SetFloat("lacunarity", noiseSettings.lacunarity);
            noiseMapShader.SetFloat("persistance", noiseSettings.persistance);
            noiseMapShader.SetFloat("scale", noiseSettings.scale);
            noiseMapShader.SetFloat("weight", noiseSettings.weight);
            noiseMapShader.SetFloat("weightMultiplier", noiseSettings.weightMultiplier);

            int threadGroupsX = Mathf.CeilToInt(mapSize.x / 32f);
            int threadGroupsY = Mathf.CeilToInt(mapSize.y / 32f);
            noiseMapShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
            offsetsBuffer.Release();

            float[] noiseMap = new float[mapSize.x * mapSize.y];
            mapBuffer.GetData(noiseMap);
            mapBuffer.Release();

            return noiseMap;
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
            [Min(0)] public float weight = 1;
            [Min(0)] public float weightMultiplier = 1;
        }
    }
}
