using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    /// <summary>
    /// <see cref="DecorationGenerator"/>로 부터 생성된 데코레이션 정보와 렌더링에 필요한 데이터
    /// </summary>
    public class DecorationData
    {
        /// <summary>
        /// 데코레이션 맵 크기 (width, height)
        /// </summary>
        public readonly Vector2Int mapSize;
        /// <summary>
        /// <inheritdoc cref="Decoration"/>
        /// </summary>
        public readonly Decoration?[] decorationInfos;
        /// <summary>
        /// <inheritdoc cref="RenderData"/>
        /// </summary>
        public readonly RenderData?[] renderDatas;

        public DecorationData(Vector2Int mapSize)
        {
            this.mapSize = mapSize;
            decorationInfos = new Decoration?[mapSize.x * mapSize.y];
            renderDatas = new RenderData?[mapSize.x * mapSize.y];
        }

        /// <summary>
        /// 데코레이션 렌더링에 필요한 정보
        /// </summary>
        public struct RenderData
        {
            /// <summary>
            /// 데코레이션 프리팹
            /// </summary>
            public readonly GameObject prefab;
            /// <summary>
            /// 데코레이션 오브젝트 크기
            /// </summary>
            public readonly Vector3 scale;
            /// <summary>
            /// 데코레이션 오브젝트가 바라보는 방향
            /// </summary>
            public readonly Vector3 lookDirection;

            public RenderData(GameObject prefab, Vector3 scale, Vector3 lookDirection)
            {
                this.prefab = prefab;
                this.scale = scale;
                this.lookDirection = lookDirection;
            }
        }
    }
}
