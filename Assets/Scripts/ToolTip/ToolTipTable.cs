using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/ToolTipTable")]
    public class ToolTipTable: ScriptableObject
    {
        public List<ToolTip> ToolTips { get { return toolTips; } }
        [SerializeField]
        private List<ToolTip> toolTips = new List<ToolTip>();
    }
}