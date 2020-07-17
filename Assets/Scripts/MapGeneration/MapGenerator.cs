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
        [OnValueChanged(nameof(OnMapSizeChanged))]
        public Vector2Int mapSize;
        public NoiseGenerator noiseGenerator;

        [Title("Preview")]
        public bool autoUpdatePreview;
        public PreviewWorld previewWorld;
        public MeshRenderer previewTextureRenderer;
        private Texture2D previewTexture;

        private void OnValidate()
        {
            if (autoUpdatePreview)
            {
                UpdatePreview();
            }
        }

        [Button]
        public void UpdatePreview()
        {
            Profiler.BeginSample("Update preview");

            noiseGenerator.GenerateNoiseMap(mapSize.x, mapSize.y, out float[] noiseMap);

            // Update texture
            Color[] colorMap = noiseMap
                .Select(x => Color.Lerp(Color.black, Color.white, x))
                .ToArray();
            UpdatePreviewTexture(ref colorMap);

            // Update world
            previewWorld.SetHexagonsColor(ref colorMap);

            Profiler.EndSample();
        }

        private void OnMapSizeChanged()
        {
            previewWorld.MapSize = mapSize;
            previewTexture = null;
        }

        private void UpdatePreviewTexture(ref Color[] colors)
        {
            if (previewTexture == null)
            {
                previewTexture = new Texture2D(mapSize.x, mapSize.y)
                {
                    filterMode = FilterMode.Point
                };
            }

            previewTexture.SetPixels(colors);
            previewTexture.Apply();

            var propertyBlock =  new MaterialPropertyBlock();
            propertyBlock.SetTexture("_Texture", previewTexture);
            previewTextureRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
