using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/ToolTipTable/WonderToolTipTable")]
    public class WonderToolTipTable: ScriptableObject
    {
        public List<WonderToolTip> ToolTips { get { return toolTips; } }
        [SerializeField]
        private List<WonderToolTip> toolTips = new List<WonderToolTip>();
    }
}