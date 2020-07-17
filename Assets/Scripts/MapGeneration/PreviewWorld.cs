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
    public class PreviewWorld : MonoBehaviour
    {
        public Hexagon hexagonPrefab;
        public Transform hexagonHolder;

        private Vector2Int mapSize;
        private Hexagon[] hexagons;
        private MaterialPropertyBlock propertyBlock;

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
                    Hexagon newHexagon = CreateNewHexagon(new HexagonPos(x, y));

                    int index = XYToIndex(x, y);
                    hexagons[index] = newHexagon;
                }
            }
        }

        public void SetHexagonsColor(ref Color[] colorMap)
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            int colorPropertyId = Shader.PropertyToID("_Color");
            for (int i = 0; i < hexagons.Length; i++)
            {
                Hexagon hexagon = hexagons[i];
                Color color = colorMap[i];

                propertyBlock.SetColor(colorPropertyId, color);
                hexagon.meshRenderer.SetPropertyBlock(propertyBlock);
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

            return newHexagon;
        }
    }
}
