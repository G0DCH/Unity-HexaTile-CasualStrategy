using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    // 불가사의 타일
    public abstract class WonderTile : Tile
    {
        // 불가사의 건설 제한
        public abstract bool WonderLimit();
        // 불가사의 효과
        public abstract void WonderFunction();
    }
}