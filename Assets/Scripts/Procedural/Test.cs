using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class Test : MonoBehaviour
    {
        public int seed;
        public Vector2Int mapSize;
        public float globalLength = 1;
        public float islandFactor = 1.07f;
        public Material previewMaterial;
        public Vector2Int offset;

        private void OnValidate()
        {
            RadialTest();
        }

        [Button]
        public void GaussianFalloff()
        {

        }

        public void RadialTest()
        {
            var islandRandom = new System.Random(seed);
            var bumps = islandRandom.Next(1, 6);
            var startAngle = (float)islandRandom.NextDouble() * Mathf.PI * 2;
            var dipAngle = (float)islandRandom.NextDouble() * Mathf.PI * 2;
            var dipWidth = (float)islandRandom.NextDouble() * 0.5f + 0.2f;

            var map = new float[mapSize.x * mapSize.y];
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    float xPos = x + offset.x;
                    float yPos = y + offset.y;

                    var angle = Mathf.Atan2(yPos, xPos);
                    var length = 0.5 * (Mathf.Max(Mathf.Abs(xPos), Mathf.Abs(yPos)) + globalLength);

                    var r1 = 0.5 + 0.4 * Mathf.Sin(startAngle + bumps * angle + Mathf.Cos((bumps + 3) * angle));
                    var r2 = 0.7 - 0.2 * Mathf.Sin(startAngle + bumps * angle - Mathf.Sin((bumps + 2) * angle));
                    if (Mathf.Abs(angle - dipAngle) < dipWidth
                        || Mathf.Abs(angle - dipAngle + 2 * Mathf.PI) < dipWidth
                        || Mathf.Abs(angle - dipAngle - 2 * Mathf.PI) < dipWidth)
                    {
                        r1 = 0.2f;
                        r2 = 0.2f;
                    }

                    map[x + y * mapSize.x] = length < r1 || (length > r1 * islandFactor && length < 2) ? 1 : 0;
                }
            }

            TextureUpdate(map);
        }

        private void TextureUpdate(float[] map)
        {
            Texture2D texture = new Texture2D(mapSize.x, mapSize.y)
            {
                filterMode = FilterMode.Point
            };
            var colors = map.Select(x => Color.Lerp(Color.black, Color.white, x)).ToArray();
            texture.SetPixels(colors);
            texture.Apply();
            previewMaterial.SetTexture("_Texture", texture);
        }
    }
}
