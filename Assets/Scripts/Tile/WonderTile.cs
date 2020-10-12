using UnityEngine;

namespace TilePuzzle
{
    // 불가사의 타일
    public abstract class WonderTile : Tile
    {
        public int WonderBonus { get { return wonderBonus; } }
        [SerializeField, Header("Bonus Point")]
        protected int wonderBonus = 0;
        [SerializeField, Header("Age Of Wonder")]
        public Age WonderAge = Age.Ancient;

        private bool isInitBonus = false;

        // WonderFunction을 TileManager의 delegate에 추가함.
        public abstract void AddToDelegate();

        // 불가사의 건설 제한
        public abstract bool WonderLimit(Tile currentTile);
        // 불가사의 효과
        public abstract void WonderFunction(Tile currentTile, TileBuilding tileBuilding);

        /// <summary>
        /// TileManager에서 WonderBonus 초기화 할 때 사용
        /// </summary>
        /// <param name="bonus"></param>
        public void InitWonderBonus(int bonus)
        {
            if (!isInitBonus)
            {
                wonderBonus = bonus;
            }
        }
    }
}