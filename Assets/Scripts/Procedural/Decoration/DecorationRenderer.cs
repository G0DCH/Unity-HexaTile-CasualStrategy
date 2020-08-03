using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class DecorationRenderer : MonoBehaviour
    {
        [Required]
        public DecorationObject decorationPrefab;
        public Transform decorationHolder;

        [Title("Object pooling")]
        [PropertyRange(0, nameof(maxPoolSize))]
        public int initPoolSize = 500;
        [Min(100)]
        public int maxPoolSize = 2000;

        private ObjectPool<DecorationObject> decorationObjectPool;
        private DecorationObject[] decorationObjectMap;

        private void Awake()
        {
            Debug.Assert(decorationPrefab != null, $"Missing {nameof(decorationPrefab)}");
            if (decorationHolder == null)
            {
                decorationHolder = new GameObject("Decoration Holder").transform;
            }
            decorationObjectPool = new ObjectPool<DecorationObject>(decorationPrefab, maxPoolSize, decorationHolder);
        }

        private void Start()
        {
            Profiler.BeginSample("Prepare Decoration Pool");
            decorationObjectPool.Prepare(initPoolSize);
            Profiler.EndSample();
        }

        public void Build(Vector2Int mapSize, DecorationData.RenderData?[] renderDatas)
        {
            CleanUpDecorations();
            decorationObjectMap = new DecorationObject[mapSize.x * mapSize.y];

            for (int i = 0; i < decorationObjectMap.Length; i++)
            {
                if (renderDatas[i].HasValue == false)
                {
                    continue;
                }

                int x = i % mapSize.x;
                int y = i / mapSize.y;
                Vector3 decorationPos = HexagonPos.FromArrayXY(x, y).ToWorldPos();

                DecorationObject newDecorationObject = ReuseDecorationObject(renderDatas[i].Value, decorationPos);
                decorationObjectMap[i] = newDecorationObject;
            }
        }

        private void CleanUpDecorations()
        {
            if (decorationObjectMap == null)
            {
                return;
            }

            foreach (DecorationObject decorationObject in decorationObjectMap)
            {
                PoolDecorationObject(decorationObject);
            }
        }

        private DecorationObject ReuseDecorationObject(DecorationData.RenderData renderData, Vector3 position)
        {
            DecorationObject decoration = decorationObjectPool.Pop();
            decoration.meshFilter.sharedMesh = renderData.mesh;
            decoration.meshRenderer.sharedMaterials = renderData.materials;

            decoration.transform.position = position;
            decoration.transform.LookAt(position + renderData.lookDirection);
            decoration.transform.localScale = renderData.scale;

            return decoration;
        }

        private void PoolDecorationObject(DecorationObject decoration)
        {
            decorationObjectPool.Push(decoration);
        }
    }
}
