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
    [CreateAssetMenu(menuName = "Tile Puzzle/Biome data")]
    public class BiomeData : ScriptableObject
    {
        public const int MoistureLevels = 5;
        public const int TemperatureLevels = 5;

        [InfoBox("$errorMessage", nameof(HasError), InfoMessageType = InfoMessageType.Error)]
        [BoxGroup("Main biome"), Required]
        public string biomeName;
        [BoxGroup("Main biome")]
        public Color biomeColor = Color.green;

        [SerializeField, ListDrawerSettings(Expanded = true)]
        private List<Biome> subBiomes; 

        private Biome mainBiome = new Biome();
        private Biome[,] biomeMap;
        private Texture2D previewTexture;
        private string errorMessage = string.Empty;

        public bool IsValid { get; private set; }
        private bool HasError => errorMessage.Length > 0;

        private void OnValidate()
        {
            mainBiome.biomeName = biomeName;
            mainBiome.color = biomeColor;

            IsValid = Validation();

            UpdateBiomeMap();
            UpdatePreviewTexture();
        }

        private void UpdateBiomeMap()
        {
            if (biomeMap == null)
            {
                biomeMap = new Biome[MoistureLevels, TemperatureLevels];
            }

            for (int y = 0; y < TemperatureLevels; y++)
            {
                for (int x = 0; x < MoistureLevels; x++)
                {
                    biomeMap[x, y] = mainBiome;
                }
            }

            foreach (Biome biome in subBiomes)
            {
                for (int y = biome.temperatureRange.x; y < biome.temperatureRange.y; y++)
                {
                    for (int x = biome.moistureRange.x; x < biome.moistureRange.y; x++)
                    {
                        biomeMap[x, y] = biome;
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
                    colors[x + y * Width] = biomeMap[x / widthBlockSize, y / heightBlockSize].color;
                }
            }

            previewTexture.SetPixels(colors);
            previewTexture.Apply();
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
            foreach (Biome biome in invalidRangeBiomes)
            {
                builder.AppendLine($"Sub biome '{biome.biomeName}' has invalid moisture/temperature range");
            }

            // TODO: 서브 바이옴들 영역이 겹치는지 검사

            errorMessage = builder.ToString();
            return errorMessage.Length == 0;
        }

        [Serializable]
        private class Biome : IEquatable<Biome>
        {
            [Required]
            public string biomeName;
            [MinMaxSlider(0, MoistureLevels, true)]
            public Vector2Int moistureRange = new Vector2Int(0, 1);
            [MinMaxSlider(0, TemperatureLevels, true)]
            public Vector2Int temperatureRange = new Vector2Int(0, 1);
            public Color color = Color.black;

            public override bool Equals(object obj)
            {
                return Equals(obj as Biome);
            }

            public bool Equals(Biome other)
            {
                return other != null &&
                       biomeName == other.biomeName;
            }

            public override int GetHashCode()
            {
                return 1618086760 + EqualityComparer<string>.Default.GetHashCode(biomeName);
            }

            public static bool operator ==(Biome left, Biome right)
            {
                return EqualityComparer<Biome>.Default.Equals(left, right);
            }

            public static bool operator !=(Biome left, Biome right)
            {
                return !(left == right);
            }
        }
    }
}
