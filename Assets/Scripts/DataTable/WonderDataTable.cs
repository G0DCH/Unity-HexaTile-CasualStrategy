using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [CreateAssetMenu(menuName = "Tile Puzzle/DataTable/WonderDataTable")]
    public class WonderDataTable: ScriptableObject
    {
        public Age TableAge { get { return tableAge; } }
        [SerializeField]
        private Age tableAge = Age.Ancient;
        public List<WonderData> WonderDatas { get { return wonderDatas; } }
        [SerializeField]
        private List<WonderData> wonderDatas = new List<WonderData>();
    }
}