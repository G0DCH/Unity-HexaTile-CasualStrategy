using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class HexagonGraph
    {
        public readonly Vector2Int size;
        public readonly Center[] centers;
        public readonly Corner[] corners;

        public HexagonGraph(Vector2Int graphSize)
        {
            Profiler.BeginSample("Create Hexagon Graph");

            if (graphSize.x < 0 || graphSize.y < 0)
            {
                throw new ArgumentException($"그래프 크기가 0보다 작을 수 없음");
            }

            size = graphSize;
            int width = graphSize.x;
            int height = graphSize.y;
            int totalCenters = width * height;

            centers = new Center[totalCenters];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    HexagonPos hexPos = HexagonPos.FromArrayXY(x, y);

                    // 새로운 center 생성
                    Center newCenter = new Center(hexPos);
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        newCenter.isBorder = true;
                    }
                    centers[x + y * width] = newCenter;

                    // 이웃한 center들을 서로 연결
                    for (int hexZ = -1; hexZ <= 1; hexZ++)
                    {
                        for (int hexX = -1; hexX <= 1; hexX++)
                        {
                            if (hexX == hexZ)
                            {
                                continue;
                            }

                            HexagonPos neighborHexPos = hexPos + new HexagonPos(hexX, hexZ);
                            Vector2Int neighborXY = neighborHexPos.ToArrayXY();
                            if (neighborXY.x < 0 || neighborXY.x >= width || neighborXY.y < 0 || neighborXY.y >= height)
                            {
                                continue;
                            }

                            int neighborIndex = neighborXY.x + neighborXY.y * width;
                            Center neighbor = centers[neighborIndex];
                            if (neighbor != null)
                            {
                                newCenter.ConnectNeighborCenter(neighbor);
                            }
                        }
                    }
                }
            }

            // center에 corner 할당
            foreach (Center center in centers)
            {
                center.CreateOrAssignCorners();
            }

            // center의 corner중 이웃한 corner끼리 연결
            foreach (Center center in centers)
            {
                center.ConnectCornerToCorner();
            }

            // 중복없는 모든 corner 리스트
            HashSet<Corner> cornerSet = new HashSet<Corner>();
            foreach (Center center in centers)
            {
                cornerSet.UnionWith(center.NeighborCorners);
            }
            corners = cornerSet.ToArray();

            Profiler.EndSample();
        }
    }
}
