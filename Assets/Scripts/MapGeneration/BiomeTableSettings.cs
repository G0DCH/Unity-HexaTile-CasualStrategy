#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/Biome table")]
    public class BiomeTableSettings : ScriptableObject
    {
        public const int MoistureLevels = 5;
        public const int TemperatureLevels = 5;

        [InfoBox("$errorMessage", nameof(HasError), InfoMessageType = InfoMessageType.Error)]
        [BoxGroup("Main biome"), SerializeField, Required]
        private string biomeName;
        [BoxGroup("Main biome"), SerializeField]
        private Color biomeColor = Color.green;

        [SerializeField, ListDrawerSettings(Expanded = true)]
        private List<BiomeData> subBiomes; 

        [SerializeField, HideInInspector]
        private BiomeData mainBiome;
        private Color[,] biomeColorMap;
        private Texture2D previewTexture;
        private string errorMessage = string.Empty;

        private bool HasError => errorMessage.Length > 0;

        private void OnValidate()
        {
            mainBiome.biomeName = biomeName;
            mainBiome.color = biomeColor;

            Validation();
            UpdateBiomeColorMap();
            UpdatePreviewTexture();
        }

        public BiomeTable GetBiomeTable()
        {
            if (Validation() == false)
            {
                throw new InvalidOperationException($"{nameof(BiomeTableSettings)} '{name}' has invalid datas\n{errorMessage}");
            }

            return new BiomeTable(mainBiome, subBiomes);
        }

        private void UpdateBiomeColorMap()
        {
            if (biomeColorMap == null)
            {
                biomeColorMap = new Color[MoistureLevels, TemperatureLevels];
            }

            for (int y = 0; y < TemperatureLevels; y++)
            {
                for (int x = 0; x < MoistureLevels; x++)
                {
                    biomeColorMap[x, y] = mainBiome.color;
                }
            }

            foreach (BiomeData biomeData in subBiomes)
            {
                for (int y = biomeData.temperatureRange.x; y < biomeData.temperatureRange.y; y++)
                {
                    for (int x = biomeData.moistureRange.x; x < biomeData.moistureRange.y; x++)
                    {
                        biomeColorMap[x, y] = biomeData.color;
                    }
                }
            }
        }

        private void UpdatePreviewTexture()
        {
            const int Width = 200;
            const int Height = 200;
            if (previewTexture == null)
            {
                previewTexture = new Texture2D(Width, Height)
                {
                    filterMode = FilterMode.Point
                };
            }

            Color[] colors = new Color[Width * Height];
            int widthBlockSize = Width / MoistureLevels;
            int heightBlockSize = Height / TemperatureLevels;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    colors[x + y * Width] = biomeColorMap[x / widthBlockSize, y / heightBlockSize];
                }
            }

            previewTexture.SetPixels(colors);
            previewTexture.Apply();
        }

        private bool Validation()
        {
            StringBuilder builder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(mainBiome.biomeName))
            {
                builder.AppendLine("Missing main biome name");
            }

            if (subBiomes.Any(x => string.IsNullOrWhiteSpace(x.biomeName)))
            {
                builder.AppendLine("Missing sub biome name");
            }

            var namedBiomes = subBiomes.Where(x => string.IsNullOrWhiteSpace(x.biomeName) == false);
            if (namedBiomes.Count() != namedBiomes.Distinct().Count())
            {
                builder.AppendLine("Duplicated sub biome names");
            }

            var invalidRangeBiomes = namedBiomes
                .Distinct()
                .Where(biome => biome.moistureRange.x < 0
                    || biome.moistureRange.y > MoistureLevels
                    || biome.moistureRange.x >= biome.moistureRange.y
                    || biome.temperatureRange.x < 0
                    || biome.temperatureRange.y > TemperatureLevels
                    || biome.temperatureRange.x >= biome.temperatureRange.y);
            foreach (BiomeData biome in invalidRangeBiomes)
            {
                builder.AppendLine($"Sub biome '{biome.biomeName}' has invalid moisture/temperature range");
            }

            // TODO: 서브 바이옴들 영역이 겹치는지 검사

            errorMessage = builder.ToString();
            return errorMessage.Length == 0;
        }

        [OnInspectorGUI, FoldoutGroup("Biome Preview", true, -10)]
        [InfoBox("X: Moisture, Y: Temperature")]
        private void DrawBiomePreview()
        {
            if (previewTexture != null)
            {
                GUILayout.BeginHorizontal(GUILayout.Width(EditorGUIUtility.currentViewWidth / 2f));
                GUILayout.Label(previewTexture);
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 인스펙터에서 바이옴을 세팅하기 위한 용도
        /// </summary>
        [Serializable]
        public class BiomeData : IEquatable<BiomeData>
        {
            [Required]
            public string biomeName;
            [MinMaxSlider(0, MoistureLevels, true)]
            public Vector2Int moistureRange;
            [MinMaxSlider(0, TemperatureLevels, true)]
            public Vector2Int temperatureRange;
            public Color color;

            private BiomeData() { }

            public override bool Equals(object obj)
            {
                return Equals(obj as BiomeData);
            }

            public bool Equals(BiomeData other)
            {
                return other != null &&
                       biomeName == other.biomeName;
            }

            public override int GetHashCode()
            {
                return 1618086760 + EqualityComparer<string>.Default.GetHashCode(biomeName);
            }

            public static bool operator ==(BiomeData left, BiomeData right)
            {
                return EqualityComparer<BiomeData>.Default.Equals(left, right);
            }

            public static bool operator !=(BiomeData left, BiomeData right)
            {
                return !(left == right);
            }
        }
    }
}
