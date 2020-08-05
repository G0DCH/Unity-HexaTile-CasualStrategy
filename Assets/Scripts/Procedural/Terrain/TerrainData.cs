using System;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class TerrainData
    {
        /// <summary>
        /// 지형 크기 (width, height)
        /// </summary>
        public readonly Vector2Int terrainSize;
        /// <summary>
        /// 헥사곤 중심 속성
        /// </summary>
        public readonly Center[] centers;
        /// <summary>
        /// 헥사곤 꼭짓점 속성
        /// </summary>
        public readonly Corner[] corners;
        public readonly BiomeTable biomeTable;

        public TerrainData(Vector2Int terrainSize, Center[] centers, Corner[] corners, BiomeTable biomeTable)
        {
            if (terrainSize.x < 0 || terrainSize.y < 0)
            {
                throw new ArgumentException($"지형 크기가 0보다 작을 수 없음");
            }

            this.terrainSize = terrainSize;
            this.centers = centers ?? throw new ArgumentNullException(nameof(centers));
            this.corners = corners ?? throw new ArgumentNullException(nameof(corners));
            this.biomeTable = biomeTable ?? throw new ArgumentNullException(nameof(biomeTable));
        }
    }
}
