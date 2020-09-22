using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/ToolTipTable/BuildingToolTipTable")]
    public class BuildingToolTipTable: ScriptableObject
    {
        public List<BuildingToolTip> BuildingToolTips { get { return buildingToolTips; } }
        [SerializeField]
        private List<BuildingToolTip> buildingToolTips = new List<BuildingToolTip>();
    }
}