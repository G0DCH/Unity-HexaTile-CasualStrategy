using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class TerrainData
    {
        public Vector2Int terrainSize;
        public Center[] centers;
        public Corner[] corners;
        public BiomeTable biomeTable;

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
