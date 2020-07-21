using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    // 불가사의 타일
    public abstract class WonderTile : Tile
    {
        // WonderFunction을 TileManager의 delegate에 추가함.
        public abstract void AddToDelegate();

        // 불가사의 건설 제한
        public abstract bool WonderLimit(Tile currentTile);
        // 불가사의 효과
        public abstract void WonderFunction(Tile currentTile, Tile selectedTile);
    }
}