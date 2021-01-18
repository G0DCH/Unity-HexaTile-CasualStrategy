using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TilePuzzle
{
    /// <summary>
    /// 테이블에 프리팹 넣어줌.
    /// </summary>
    public class BuildingTablePrefabInit: MonoBehaviour
    {
        public BuildingDataTable buildingDataTable;
        public List<GameObject> buildingPrefabs;

        // 테이블에 프리팹 넣어줌.
        [Button]
        public void InitTablePrefab()
        {
            foreach(var buildingData in buildingDataTable.BuildingDatas)
            {
                foreach (var buildingPrefab in buildingPrefabs)
                {
                    Tile tile = buildingPrefab.GetComponent<Tile>();

                    if (tile.MyTileBuilding == buildingData.MyBuilding)
                    {
                        //buildingData.MyPrefab = buildingPrefab;
                        buildingPrefabs.Remove(buildingPrefab);
                        break;
                    }
                }
            }

            UnityEditor.EditorUtility.SetDirty(buildingDataTable);
        }
    }
}