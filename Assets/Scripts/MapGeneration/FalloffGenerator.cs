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
    public class FalloffGenerator : MonoBehaviour
    {
        [Required]
        public ComputeShader falloffShader;
        public Vector2 falloffParameter = new Vector2(3f, 2.2f);
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

        public void GenerateFalloffMap(int width, int height, ComputeBuffer falloffMapBuffer)
        {
            Profiler.BeginSample(nameof(GenerateFalloffMap));

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

            Profiler.EndSample();
        }

        public void GenerateFalloffMap(int width, int height, out float[] falloffMap)
        {
            int bufferSize = width * height;
            ComputeBuffer falloffMapBuffer = new ComputeBuffer(bufferSize, sizeof(float));

            GenerateFalloffMap(width, height, falloffMapBuffer);

            falloffMap = new float[bufferSize];
            falloffMapBuffer.GetData(falloffMap);
            falloffMapBuffer.Release();
        }
    }
}
