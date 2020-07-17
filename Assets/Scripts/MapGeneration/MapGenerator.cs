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
        [Required] public NoiseGenerator noiseGenerator;
        [Required] public FalloffGenerator falloffGenerator;

        [Title("Preview")]
        public bool autoUpdatePreview;
        [Required] public PreviewWorld previewWorld;
        [Required] public MeshRenderer noiseMapPreview;
        [Required] public MeshRenderer falloffMapPreview;

        [HideInInspector]
        public bool hasParameterUpdated;

        private void OnValidate()
        {
            hasParameterUpdated = true;
        }

        private void Update()
        {
            if (hasParameterUpdated && autoUpdatePreview)
            {
                hasParameterUpdated = false;
                UpdatePreview();
            }
        }

        [Button]
        public void UpdatePreview()
        {
            noiseGenerator.GenerateNoiseMap(mapSize.x, mapSize.y, out float[] noiseMap);
            falloffGenerator.GenerateFalloffMap(mapSize.x, mapSize.y, out float[] falloffMap);

            // Update texture
            Profiler.BeginSample("Generate color maps");
            Color[] noiseMapColors = noiseMap
                .Select(x => Color.Lerp(Color.black, Color.white, x))
                .ToArray();
            Color[] falloffMapColors = falloffMap
                .Select(x => Color.Lerp(Color.black, Color.white, x))
                .ToArray();
            Profiler.EndSample();

            Profiler.BeginSample("Apply textures");
            UpdatePreviewTexture(noiseMapPreview, ref noiseMapColors);
            UpdatePreviewTexture(falloffMapPreview, ref falloffMapColors);
            Profiler.EndSample();

            // Update world
            Profiler.BeginSample("Update world");
            previewWorld.GenerateDefaultHexagons(mapSize);
            previewWorld.SetHexagonsColor(ref noiseMapColors, ref falloffMapColors);
            Profiler.EndSample();
        }

        private void UpdatePreviewTexture(MeshRenderer previewRenderer, ref Color[] colors)
        {
            var previewTexture = new Texture2D(mapSize.x, mapSize.y)
            {
                filterMode = FilterMode.Point
            };
            previewTexture.SetPixels(colors);
            previewTexture.Apply();

            var propertyBlock =  new MaterialPropertyBlock();
            propertyBlock.SetTexture("_Texture", previewTexture);
            previewRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
