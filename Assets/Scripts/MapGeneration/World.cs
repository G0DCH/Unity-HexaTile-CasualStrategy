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
    public class World : MonoBehaviour
    {
        public Vector2Int mapSize;
        public Hexagon hexagonPrefab;
        public Transform hexagonHolder;

        private Hexagon[] hexagons;

        [Button]
        public void GenerateHexagons()
        {
            DestroyAllHexagons();

            hexagons = new Hexagon[mapSize.x * mapSize.y];
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    Hexagon newHexagon = CreateNewHexagon(new Vector2Int(x, y));

                    int index = XYToIndex(x, y);
                    hexagons[index] = newHexagon;
                }
            }
        }

        public void Test(float[] noiseMap)
        {
            var propertyBlock = new MaterialPropertyBlock();
            for (int i = 0; i < hexagons.Length; i++)
            {
                Hexagon hexagon = hexagons[i];
                float value = noiseMap[i];

                propertyBlock.SetColor("_Color", Color.Lerp(Color.black, Color.white, value));
                hexagon.meshRenderer.SetPropertyBlock(propertyBlock);

                Vector3 newPos = hexagon.transform.position;
                newPos.y = value;
                hexagon.transform.position = newPos;
            }
        }

        [Button]
        public void DestroyAllHexagons()
        {
            foreach (var hexagon in GameObject.FindGameObjectsWithTag("Hexagon"))
            {
                DestroyImmediate(hexagon);
            }
        }

        public Vector3 HexagonPosToWorldPos(Vector2Int hexagonPos, float worldY = 0)
        {
            float worldX = (hexagonPos.y & 1) == 0 
                ? hexagonPos.x * Hexagon.Size 
                : (hexagonPos.x * Hexagon.Size) + (Hexagon.Size / 2);
            float worldZ = hexagonPos.y * Hexagon.Size * Mathf.Sin(Mathf.PI / 3);
            return new Vector3(worldX, worldY, worldZ);
        }

        private int XYToIndex(int x, int y)
        {
            return x + y * mapSize.x;
        }

        private Hexagon CreateNewHexagon(Vector2Int hexagonPos)
        {
            Hexagon newHexagon = Instantiate(hexagonPrefab, hexagonHolder);
            newHexagon.hexagonPos = hexagonPos;
            newHexagon.name = $"Hexagon {newHexagon.hexagonPos}";
            newHexagon.transform.position = HexagonPosToWorldPos(newHexagon.hexagonPos);

            return newHexagon;
        }
    }
}
