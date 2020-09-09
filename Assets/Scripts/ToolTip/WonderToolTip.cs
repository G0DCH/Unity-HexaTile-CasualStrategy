using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public class WonderToolTip: ToolTip
    {
        public string WonderName { get { return wonderName; } }
        [SerializeField]
        private string wonderName = string.Empty;
    }
}