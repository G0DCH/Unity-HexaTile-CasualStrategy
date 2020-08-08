using System;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    /// <summary>
    /// 지형 정보를 그래프의 형태로 관리
    /// </summary>
    public class TerrainData
    {
        /// <summary>
        /// 지형 정보 그래프
        /// </summary>
        public readonly HexagonGraph terrainGraph;
        /// <summary>
        /// 지형 정보 생성에 사용된 바이옴 테이블
        /// </summary>
        public readonly BiomeTable biomeTable;

        public TerrainData(HexagonGraph terrainGraph, BiomeTable biomeTable)
        {
            this.terrainGraph = terrainGraph ?? throw new ArgumentNullException(nameof(terrainGraph));
            this.biomeTable = biomeTable ?? throw new ArgumentNullException(nameof(biomeTable));
        }
    }
}
