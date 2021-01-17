using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public class BuildingData
    {
        /// <summary>
        /// 이 툴팁의 건물
        /// </summary>
        public TileBuilding MyBuilding { get { return myBuilding; } }
        [SerializeField]
        private TileBuilding myBuilding = TileBuilding.Campus;

        public string ToolTipText { get { return toolTipText; } }
        [SerializeField, TextArea]
        private string toolTipText = string.Empty;

        public List<InfoPerAge> InfoPerAges { get { return infoPerAges; } }
        [SerializeField]
        private List<InfoPerAge> infoPerAges = new List<InfoPerAge>();
    }
}