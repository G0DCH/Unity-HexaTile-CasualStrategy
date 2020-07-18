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
    [ExecuteInEditMode]
    public class MapGenerator : MonoBehaviour
    {
        [Title("Generate options")]
        public Vector2Int mapSize = new Vector2Int(30, 30);
        [Required] public NoiseMapGenerator noiseGenerator;
        [Required] public FalloffMapGenerator falloffGenerator;
        [Range(0, 1)]
        public float threshhold = 0.5f;

        [Title("Debug options")]
        [Required]
        public PreviewWorld previewWorld;
        public bool autoUpdateWorld;
        public Color lowlandColor;
        public Color highlandColor;
        public Color oceanColor;

        [HideInInspector]
        public bool hasParameterUpdated;

        private void Update()
        {
            if (hasParameterUpdated && autoUpdateWorld)
            {
                hasParameterUpdated = false;
                UpdatePreview();
            }
        }

        private void OnValidate()
        {
            hasParameterUpdated = true;
        }

        [Button]
        public void UpdatePreview()
        {
            noiseGenerator.GenerateNoiseMap(mapSize.x, mapSize.y, out float[] noiseMap);
            falloffGenerator.GenerateFalloffMap(mapSize.x, mapSize.y, out float[] falloffMap);

            Profiler.BeginSample("Update world");
            Color[] hexagonColors = new Color[noiseMap.Length];
            float[] elevationMap = new float[noiseMap.Length];
            for (int i = 0; i < noiseMap.Length; i++)
            {
                if (noiseMap[i] * falloffMap[i] >= threshhold)
                {
                    hexagonColors[i] = Color.Lerp(lowlandColor, highlandColor, Mathf.InverseLerp(threshhold, 1, noiseMap[i]));
                    elevationMap[i] = noiseMap[i];
                }
                else
                {
                    hexagonColors[i] = oceanColor;
                    elevationMap[i] = 0;
                }
            }
            previewWorld.GenerateDefaultHexagons(mapSize);
            previewWorld.SetHexagonsColor(ref hexagonColors);
            previewWorld.SetHexagonsElevation(ref elevationMap);
            Profiler.EndSample();
        }
    }
}
