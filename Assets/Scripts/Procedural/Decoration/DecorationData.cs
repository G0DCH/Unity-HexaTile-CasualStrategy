using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class DecorationData
    {
        public Vector2Int mapSize;
        public Decoration?[] decorations;
        public RenderData?[] renderDatas;

        public DecorationData(Vector2Int mapSize)
        {
            this.mapSize = mapSize;
            decorations = new Decoration?[mapSize.x * mapSize.y];
            renderDatas = new RenderData?[mapSize.x * mapSize.y];
        }

        public DecorationData(Vector2Int mapSize, Decoration?[] decorations, RenderData?[] renderDatas)
        {
            this.mapSize = mapSize;
            this.decorations = decorations ?? throw new ArgumentNullException(nameof(decorations));
            this.renderDatas = renderDatas ?? throw new ArgumentNullException(nameof(renderDatas));
        }

        public struct RenderData
        {
            public GameObject prefab;
            public Vector3 scale;
            public Vector3 lookDirection;

            public RenderData(GameObject prefab, Vector3 scale, Vector3 lookDirection)
            {
                this.prefab = prefab;
                this.scale = scale;
                this.lookDirection = lookDirection;
            }
        }
    }
}
