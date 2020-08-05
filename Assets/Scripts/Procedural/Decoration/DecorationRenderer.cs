﻿using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class DecorationRenderer : MonoBehaviour
    {
        [SerializeField]
        private Transform decorationHolder;
        private GameObject[] spawnedDecorationObjects;

        private void Awake()
        {
            if (decorationHolder == null)
            {
                decorationHolder = new GameObject("Decoration Holder").transform;
            }
        }

        /// <summary>
        /// 입력을 기반으로 데코레이션 오브젝트 생성
        /// </summary>
        /// <param name="mapSize">데코레이션 맵 크기 (width, height)</param>
        /// <param name="renderDatas">데코레이션 렌더링에 필요한 정보</param>
        public void SpawnDecorations(Vector2Int mapSize, DecorationData.RenderData?[] renderDatas)
        {
            CleanUpDecorations();
            spawnedDecorationObjects = new GameObject[mapSize.x * mapSize.y];

            for (int i = 0; i < spawnedDecorationObjects.Length; i++)
            {
                if (renderDatas[i].HasValue == false)
                {
                    continue;
                }

                int x = i % mapSize.x;
                int y = i / mapSize.y;
                Vector3 decorationPos = HexagonPos.FromArrayXY(x, y).ToWorldPos();

                GameObject newDecorationObject = CloneDecorationObject(renderDatas[i].Value, decorationHolder, decorationPos);
                spawnedDecorationObjects[i] = newDecorationObject;
            }
        }

        private void CleanUpDecorations()
        {
            if (spawnedDecorationObjects == null)
            {
                return;
            }

            foreach (GameObject decorationObject in spawnedDecorationObjects)
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
