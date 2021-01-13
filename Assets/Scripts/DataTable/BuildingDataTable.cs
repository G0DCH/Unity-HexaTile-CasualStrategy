using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/DataTable/BuildingDataTable")]
    public class BuildingDataTable: ScriptableObject
    {
        public List<BuildingData> BuildingDatas { get { return buildingDatas; } }
        [SerializeField]
        private List<BuildingData> buildingDatas = new List<BuildingData>();
    }
}