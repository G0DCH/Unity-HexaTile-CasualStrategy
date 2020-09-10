using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public class ToolTip
    {
        public TileBuilding BuildingType { get { return buildingType; } }
        [SerializeField]
        private TileBuilding buildingType = TileBuilding.Wonder;
        
        /// <summary>
        /// 각 인덱스에 해당하는 툴팁이
        /// enum Age에 대응됨.
        /// </summary>
        public List<string> ToolTipDescribe { get { return toolTipDescribe; } }
        [SerializeField, TextArea]
        private List<string> toolTipDescribe = new List<string>();
    }
}