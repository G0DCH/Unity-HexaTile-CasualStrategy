using Sirenix.OdinInspector;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public class WonderToolTip: ToolTip
    {
        public string WonderName { get { return wonderName; } }
        [PropertyOrder(-1), SerializeField]
        private string wonderName = string.Empty;
    }
}