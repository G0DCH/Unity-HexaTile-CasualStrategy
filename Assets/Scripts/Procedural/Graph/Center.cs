using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilePuzzle.Procedural;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class Center : IEquatable<Center>
    {
        private static readonly HexagonPos[] neighborOffsetTable = new HexagonPos[]
        {
            HexagonPos.TopRight, HexagonPos.Right, HexagonPos.BottomRight, HexagonPos.BottomLeft, HexagonPos.Left, HexagonPos.TopLeft
        };

        public readonly HexagonPos hexPos;
        public readonly Vector3 centerPos;

        public bool isWater = false;
        public bool isSea = false;
        public bool isCoast = false;
        public bool isBorder = false;
        public float elevation = 0f;
        public float moisture = 0f;
        public int biomeId = 0;

        private readonly Dictionary<HexagonPos, Center> neighborCenters;
        private readonly Corner[] neighborCorners;

        public float Temperature => 1 - elevation;
        public IReadOnlyDictionary<HexagonPos, Center> NeighborCenters => neighborCenters;
        public Corner[] NeighborCorners => neighborCorners;

        public Center(HexagonPos hexPos)
        {
            this.hexPos = hexPos;
            centerPos = hexPos.ToWorldPos();

            neighborCenters = new Dictionary<HexagonPos, Center>();
            neighborCorners = new Corner[6];
        }

        public void ConnectNeighborCenter(Center neighbor)
        {
            HexagonPos neighborOffset = neighbor.hexPos - hexPos;
            if (neighborOffset.HexagonDistance != 1)
            {
                throw new InvalidOperationException($"이웃한 center가 아님, {hexPos}, {neighbor.hexPos}");
            }
             
            // 서로 center를 연결
            neighborCenters.Add(neighborOffset, neighbor);
            neighbor.neighborCenters.Add(-neighborOffset, this);
        }

        public void CreateOrAssignCorners()
        {
            int range = neighborOffsetTable.Length;
            float distanceToVertex = HexagonObject.Size * Mathf.Sin(Mathf.PI / 3) / 3 * 2;
            for (int i = 0; i < range; i++)
            {
                HexagonPos rightNeighborOffset = neighborOffsetTable[i];
                HexagonPos leftNeighborOffset = neighborOffsetTable[Modulo(i - 1, range)];
                neighborCenters.TryGetValue(rightNeighborOffset, out Center rightNeighbor);
                neighborCenters.TryGetValue(leftNeighborOffset, out Center leftNeighbor);

                // 주변 center들이 이미 corner를 가지고 있는지 확인
                Corner shareCorner = neighborCorners[i];
                if (shareCorner == null && rightNeighbor != null)
                {
                    shareCorner = rightNeighbor.neighborCorners[Modulo(i - 2, range)];
                }
                if (shareCorner == null && leftNeighbor != null)
                {
                    shareCorner = leftNeighbor.neighborCorners[Modulo(i + 2, range)];
                }

                // 새로운 corner 생성
                if (shareCorner == null)
                {
                    Vector3 cornerOffset = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * distanceToVertex;
                    shareCorner = new Corner(centerPos + cornerOffset);
                    shareCorner.AddNeighborCenter(this);
                    shareCorner.AddNeighborCenter(rightNeighbor);
                    shareCorner.AddNeighborCenter(leftNeighbor);
                }

                if (isBorder)
                {
                    shareCorner.isBorder = true;
                }

                // 각각 corner 할당
                neighborCorners[i] = shareCorner;
                if (rightNeighbor != null)
                {
                    rightNeighbor.neighborCorners[Modulo(i - 2, range)] = shareCorner;
                }
                if (leftNeighbor != null)
                {
                    leftNeighbor.neighborCorners[Modulo(i + 2, range)] = shareCorner;
                }
            }
        }

        public void ConnectCornerToCorner()
        {
            int range = neighborCorners.Length;
            for (int i = 0; i < range; i++)
            {
                Corner corner = neighborCorners[i];
                if (corner == null)
                {
                    continue;
                }

                // 오른쪽 코너가 있으면 서로 연결
                Corner rightCorner = neighborCorners[Modulo(i + 1, range)];
                if (rightCorner != null)
                {
                    Corner.Connect(corner, rightCorner);
                }

                // 왼쪽 코너가 있으면 서로 연결
                Corner leftCorner = neighborCorners[Modulo(i - 1, range)];
                if (leftCorner != null)
                {
                    Corner.Connect(corner, leftCorner);
                }
            }
        }

        private int Modulo(int x, int m)
        {
            int remainder = x % m;
            return remainder < 0 ? remainder + m : remainder;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Center);
        }

        public bool Equals(Center other)
        {
            return other != null &&
                   hexPos.Equals(other.hexPos);
        }

        public override int GetHashCode()
        {
            return 888507424 + hexPos.GetHashCode();
        }

        public static bool operator ==(Center left, Center right)
        {
            return EqualityComparer<Center>.Default.Equals(left, right);
        }

        public static bool operator !=(Center left, Center right)
        {
            return !(left == right);
        }
    }
}