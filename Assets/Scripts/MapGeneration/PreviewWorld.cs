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
    public class PreviewWorld : MonoBehaviour
    {
        [Required]
        public Hexagon hexagonPrefab;
        [Required]
        public Material hexagonMaterial;
        public Transform hexagonHolder;

        private Vector2Int mapSize;
        private Hexagon[] hexagons;

        [Button]
        public void GenerateDefaultHexagons(Vector2Int mapSize)
        {
            if (hexagons != null && this.mapSize == mapSize)
            {
                return;
            }

            DestroyAllHexagons();

            this.mapSize = mapSize;
            hexagons = new Hexagon[mapSize.x * mapSize.y];
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    Hexagon newHexagon = CreateNewHexagon(HexagonPos.FromArrayXY(x, y));

                    int index = XYToIndex(x, y);
                    hexagons[index] = newHexagon;
                }
            }
        }

        public void SetHexagonColorMap(int mapWidth, int mapHeight, ref Color[] colorMap)
        {
            Profiler.BeginSample(nameof(SetHexagonColorMap));
            int textureWidth = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(mapWidth, 2)));
            int textureHeight = (int)Mathf.Pow(2, Mathf.CeilToInt(Mathf.Log(mapHeight, 2)));
            Texture2D texture = new Texture2D(textureWidth, textureHeight)
            {
                filterMode = FilterMode.Point
            };

            Color[] textureColors = new Color[textureWidth * textureHeight];
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    textureColors[x + y * textureWidth] = colorMap[x + y * mapWidth];
                }
            }
            texture.SetPixels(textureColors);
            texture.Apply();

            hexagonMaterial.SetTexture("_ColorMap", texture);
            hexagonMaterial.SetVector("_ColorMapSize", new Vector2(textureWidth, textureHeight));
            Profiler.EndSample();
        }

        public void SetHexagonsElevation(ref float[] elevations, float multiplier)
        {
            for (int i = 0; i < hexagons.Length; i++)
            {
                var pos = hexagons[i].transform.position;
                pos.y = elevations[i] * multiplier;
                hexagons[i].transform.position = pos;
            }
        }

        [Button]
        private void DestroyAllHexagons()
        {
            foreach (var hexagon in GameObject.FindGameObjectsWithTag("Hexagon"))
            {
                DestroyImmediate(hexagon);
            }
            hexagons = null;
        }

        private int XYToIndex(int x, int y)
        {
            return x + y * mapSize.x;
        }

        private Hexagon CreateNewHexagon(HexagonPos hexPos)
        {
            Hexagon newHexagon = Instantiate(hexagonPrefab, hexagonHolder);
            newHexagon.hexPos = hexPos;
            newHexagon.name = $"Hexagon {newHexagon.hexPos}";
            newHexagon.transform.position = newHexagon.hexPos.ToWorldPos();
            newHexagon.GetComponent<MeshRenderer>().sharedMaterial = hexagonMaterial;

            return newHexagon;
        }
    }
}
