using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TilePuzzle
{
    public class WonderTablePrefabInit: MonoBehaviour
    {
        public List<WonderDataTable> wonderDataTables;
        public List<GameObject> wonderPrefabs;

        // 테이블에 프리팹 넣어줌.
        [Button]
        public void InitTablePrefab()
        {
            foreach (var wonderDataTable in wonderDataTables)
            {
                foreach (var wonderData in wonderDataTable.WonderDatas)
                {
                    foreach (var wonderPrefab in wonderPrefabs)
                    {
                        if (wonderData.WonderName == wonderPrefab.name)
                        {
                            //wonderData.MyPrefab = wonderPrefab;
                            wonderPrefabs.Remove(wonderPrefab);
                            break;
                        }
                    }
                }

                UnityEditor.EditorUtility.SetDirty(wonderDataTable);
            }
        }
    }
}