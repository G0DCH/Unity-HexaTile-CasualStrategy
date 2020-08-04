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
        public Transform decorationHolder;

        private GameObject[] decorationObjectMap;

        private void Awake()
        {
            if (decorationHolder == null)
            {
                decorationHolder = new GameObject("Decoration Holder").transform;
            }
        }

        public void Build(Vector2Int mapSize, DecorationData.RenderData?[] renderDatas)
        {
            CleanUpDecorations();
            decorationObjectMap = new GameObject[mapSize.x * mapSize.y];

            for (int i = 0; i < decorationObjectMap.Length; i++)
            {
                if (renderDatas[i].HasValue == false)
                {
                    continue;
                }

                int x = i % mapSize.x;
                int y = i / mapSize.y;
                Vector3 decorationPos = HexagonPos.FromArrayXY(x, y).ToWorldPos();

                GameObject newDecorationObject = CloneDecorationObject(renderDatas[i].Value, decorationHolder, decorationPos);
                decorationObjectMap[i] = newDecorationObject;
            }
        }

        private void CleanUpDecorations()
        {
            if (decorationObjectMap == null)
            {
                return;
            }

            foreach (GameObject decorationObject in decorationObjectMap)
            {
                Destroy(decorationObject);
            }
        }

        private GameObject CloneDecorationObject(DecorationData.RenderData renderData, Transform parent, Vector3 position)
        {
            GameObject decoration = Instantiate(renderData.prefab, parent);

            decoration.transform.position = position;
            decoration.transform.LookAt(position + renderData.lookDirection);
            decoration.transform.localScale = renderData.scale;

            return decoration;
        }
    }
}
