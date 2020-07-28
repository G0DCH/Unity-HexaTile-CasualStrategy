using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TilePuzzle.Rendering
{
    public static class HexagonMeshGenerator
    {
        /// <summary>
        /// <see cref="Vector3.forward"/> 기준 시계방향으로 60도 간격씩 나누어진 헥사곤 꼭짓점 A~F
        /// </summary>
        [Flags]
        public enum VertexDirection : int
        {
            A = 1,  // 0, 360
            B = 2,  // 60
            C = 4,  // 120
            D = 8,  // 180
            E = 16, // 240
            F = 32, // 300
        }

        /// <param name="hexagonSize">헥사곤의 중심으로부터 모서리까지의 길이</param>
        /// <returns>헥사곤 평면 메시</returns>
        public static Mesh BuildMesh(float hexagonSize)
        {
            return BuildMesh(hexagonSize, 0, false, 0, 0);
        }

        /// <param name="hexagonSize">헥사곤의 중심으로부터 모서리까지의 길이</param>
        /// <param name="cliffDepth">절벽 깊이 (헥사곤 옆면 높이)</param>
        /// <returns>옆면이 있는 헥사곤 메시</returns>
        public static Mesh BuildMesh(float hexagonSize, float cliffDepth)
        {
            return BuildMesh(hexagonSize, cliffDepth, true, 0, 0);
        }

        /// <param name="hexagonSize">헥사곤의 중심으로부터 모서리까지의 길이</param>
        /// <param name="cliffDepth">절벽 깊이 (헥사곤 옆면 높이)</param>
        /// <param name="riverSize">강의 폭</param>
        /// <param name="riverDirection">강이 지나가는 꼭짓점 방향들</param>
        /// <returns>강과 옆면이 포함된 헥사곤 메시</returns>
        public static Mesh BuildMesh(float hexagonSize, float cliffDepth, float riverSize, VertexDirection riverDirection)
        {
            return BuildMesh(hexagonSize, cliffDepth, true, riverSize, riverDirection);
        }

        private static Mesh BuildMesh(float hexagonSize, float cliffDepth, bool drawCliff, float riverSize, VertexDirection rivers)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            float diagonalMultiplier = Mathf.Sin(Mathf.PI / 3) / 3 * 2;
            float distanceToVertex = hexagonSize * diagonalMultiplier;
            float distanceToRiver = riverSize * diagonalMultiplier;

            // 헥사곤 윗면의 vertex를 계산
            VertexDirection[] riverTable = Enum.GetValues(typeof(VertexDirection))
                .Cast<VertexDirection>()
                .ToArray();
            vertices.Add(Vector3.zero);
            for (int i = 0; i < riverTable.Length; i++)
            {
                VertexDirection currentRiver = riverTable[i];
                VertexDirection leftRiver = riverTable[Modulo(i - 1, riverTable.Length)];
                VertexDirection rightRiver = riverTable[Modulo(i + 1, riverTable.Length)];

                // 강이 있는 방향의 vertex인 경우
                if (rivers.HasFlag(currentRiver))
                {
                    // 양쪽으로 강이 연결됨
                    if (rivers.HasFlag(leftRiver | rightRiver))
                    {
                        Vector3 vertex = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * (distanceToVertex - distanceToRiver);
                        vertices.Add(vertex);
                    }
                    // 왼쪽으로만 강이 연결됨
                    else if (rivers.HasFlag(leftRiver))
                    {
                        Vector3 centerVertex = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * distanceToVertex;
                        Vector3 rightVertex = Quaternion.AngleAxis(60 * (i + 1), Vector3.up) * Vector3.forward * distanceToVertex;

                        Vector3 vertex = centerVertex + (rightVertex - centerVertex).normalized * distanceToRiver;
                        vertices.Add(vertex);
                    }
                    // 오른쪽으로만 강이 연결됨
                    else if (rivers.HasFlag(rightRiver))
                    {
                        Vector3 centerVertex = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * distanceToVertex;
                        Vector3 leftVertex = Quaternion.AngleAxis(60 * (i - 1), Vector3.up) * Vector3.forward * distanceToVertex;

                        Vector3 vertex = centerVertex + (leftVertex - centerVertex).normalized * distanceToRiver;
                        vertices.Add(vertex);
                    }
                    // 다른 연결된 강이 없음
                    else
                    {
                        Vector3 centerVertex = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * distanceToVertex;
                        Vector3 leftVertex = Quaternion.AngleAxis(60 * (i - 1), Vector3.up) * Vector3.forward * distanceToVertex;
                        Vector3 rightVertex = Quaternion.AngleAxis(60 * (i + 1), Vector3.up) * Vector3.forward * distanceToVertex;

                        Vector3 vertex1 = centerVertex + (leftVertex - centerVertex).normalized * distanceToRiver;
                        Vector3 vertex2 = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * (distanceToVertex - distanceToRiver);
                        Vector3 vertex3 = centerVertex + (rightVertex - centerVertex).normalized * distanceToRiver;
                        vertices.Add(vertex1);
                        vertices.Add(vertex2);
                        vertices.Add(vertex3);
                    }
                }
                else
                {
                    Vector3 vertex = Quaternion.AngleAxis(60 * i, Vector3.up) * Vector3.forward * distanceToVertex;
                    vertices.Add(vertex);
                }
            }

            // 헥사곤 윗면의 triangle을 계산
            for (int i = 1; i < vertices.Count; i++)
            {
                triangles.Add(0);   // 헥사곤 윗면 중심점
                triangles.Add(i);
                triangles.Add(Mathf.Max(Modulo(i + 1, vertices.Count), 1));
            }

            // 절벽 생성 또는 강이 있으면 헥사곤 옆면 메시 생성
            if (drawCliff || rivers != 0)
            {
                // 헥사곤 옆면 vertex, triangle 계산
                int topVertexCount = vertices.Count - 1;
                int vertexIndex = vertices.Count;
                for (int i = 1; i <= topVertexCount; i++)
                {
                    vertices.Add(vertices[i]);
                    vertices.Add(vertices[Mathf.Max(Modulo(i + 1, topVertexCount + 1), 1)]);
                    vertices.Add(vertices[i] - new Vector3(0, cliffDepth, 0));
                    vertices.Add(vertices[Mathf.Max(Modulo(i + 1, topVertexCount + 1), 1)] - new Vector3(0, cliffDepth, 0));

                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2 + 1);
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 2 + 1);
                    triangles.Add(vertexIndex + 1);
                    vertexIndex += 4;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            return mesh;
        }

        private static int Modulo(int x, int m)
        {
            int remainder = x % m;
            return remainder < 0 ? remainder + m : remainder;
        }
    }
}
