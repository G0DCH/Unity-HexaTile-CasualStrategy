using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle
{
    public struct HexagonPos : IEquatable<HexagonPos>
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

        public override bool Equals(object obj)
        {
            return obj is HexagonPos pos && Equals(pos);
        }

        public bool Equals(HexagonPos other)
        {
            return pos.Equals(other.pos);
        }

        public override int GetHashCode()
        {
            return 991532785 + pos.GetHashCode();
        }

        public override string ToString()
        {
            return pos.ToString();
        }

        public static bool operator ==(HexagonPos left, HexagonPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(HexagonPos left, HexagonPos right)
        {
            return !(left == right);
        }
    }
}
