using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle
{
    public struct HexagonPos
    {
        private Vector2Int pos;

        public HexagonPos(int x, int y)
        {
            pos = new Vector2Int(x, y);
        }

        public Vector3 ToWorldPos(float worldY = 0)
        {
            float worldX = (pos.y & 1) == 0
                ? pos.x * Hexagon.Size
                : (pos.x * Hexagon.Size) + (Hexagon.Size / 2);
            float worldZ = pos.y * Hexagon.Size * Mathf.Sin(Mathf.PI / 3);
            return new Vector3(worldX, worldY, worldZ);
        }
    }
}
