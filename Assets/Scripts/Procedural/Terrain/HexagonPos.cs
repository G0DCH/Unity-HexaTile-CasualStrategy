using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [Serializable, InlineProperty(LabelWidth = 13)]
    public struct HexagonPos : IEquatable<HexagonPos>
    {
        [SerializeField, HideLabel]
        private readonly Vector3Int pos;

        public HexagonPos(int hexX, int hexZ)
        {
            pos = new Vector3Int(hexX, -hexX - hexZ, hexZ);
        }

        public int X => pos.x;
        public int Y => pos.y;
        public int Z => pos.z;

        public static HexagonPos Zero => new HexagonPos(0, 0);
        public static HexagonPos TopRight => new HexagonPos(0, 1);
        public static HexagonPos Right => new HexagonPos(1, 0);
        public static HexagonPos BottomRight => new HexagonPos(1, -1);
        public static HexagonPos BottomLeft => new HexagonPos(0, -1);
        public static HexagonPos Left => new HexagonPos(-1, 0);
        public static HexagonPos TopLeft => new HexagonPos(-1, 1);

        public int HexagonDistance => Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z));

        public static HexagonPos FromArrayXY(int x, int y)
        {
            int hexX = x - (y >> 1);
            int hexZ = y;
            return new HexagonPos(hexX, hexZ);
        }

        public Vector2Int ToArrayXY()
        {
            int arrayX = pos.x + (pos.z >> 1);
            int arrayY = pos.z;
            return new Vector2Int(arrayX, arrayY);
        }

        public int ToArrayIndex(int arrayWidth)
        {
            Vector2Int arrayXY = this.ToArrayXY();
            int index = arrayXY.x + arrayXY.y * arrayWidth;
            return index;
        }

        public Vector3 ToWorldPos(float worldY = 0)
        {
            float worldX = pos.x * HexagonTileObject.TileSize + pos.z * HexagonTileObject.TileSize / 2;
            float worldZ = pos.z * HexagonTileObject.TileSize * Mathf.Sin(Mathf.PI / 3);
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

        public static HexagonPos operator +(HexagonPos left, HexagonPos right)
        {
            Vector3Int newHexPos = left.pos + right.pos;
            return new HexagonPos(newHexPos.x, newHexPos.z);
        }

        public static HexagonPos operator -(HexagonPos right)
        {
            Vector3Int newHexPos = -right.pos;
            return new HexagonPos(newHexPos.x, newHexPos.z);
        }

        public static HexagonPos operator -(HexagonPos left, HexagonPos right)
        {
            Vector3Int newHexPos = left.pos - right.pos;
            return new HexagonPos(newHexPos.x, newHexPos.z);
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
