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
    public class FalloffMapGenerator : MonoBehaviour
    {
        [Required]
        public ComputeShader falloffShader;
        public Vector2 falloffParameter = new Vector2(3f, 2.2f);

        [Title("Debug")]
        public TerrainGenerator terrainGenerator;
        public bool autoUpdateTerrain;
        [ShowIf(nameof(autoUpdateTerrain))]
        public MeshRenderer previewRenderer;
        [ShowIf(nameof(autoUpdateTerrain))]
        public bool updatePreview;

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

            falloffShader.SetBuffer(0, "falloffMap", falloffMapBuffer);
            falloffShader.SetInt("mapWidth", width);
            falloffShader.SetInt("mapHeight", height);
            falloffShader.SetFloat("maxX", maxX);
            falloffShader.SetFloat("maxY", maxY);
            falloffShader.SetVector("falloffParameter", falloffParameter);
            falloffShader.SetFloat("hexagonSize", Hexagon.Size);

            int threadGroupsX = Mathf.CeilToInt(width / 16f);
            int threadGroupsY = Mathf.CeilToInt(height / 16f);
            falloffShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

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

            if (updatePreview)
            {
                UpdatePreviewTexture(width, height, ref falloffMap);
            }
        }

        private void UpdatePreviewTexture(int width, int height, ref float[] falloffMap)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point
            };

            Color[] colors = falloffMap
                .Select(x => Color.Lerp(Color.black, Color.white, x))
                .ToArray();
            texture.SetPixels(colors);
            texture.Apply();

            var properties = new MaterialPropertyBlock();
            properties.SetTexture("_Texture", texture);
            previewRenderer.SetPropertyBlock(properties);
        }
    }
}
