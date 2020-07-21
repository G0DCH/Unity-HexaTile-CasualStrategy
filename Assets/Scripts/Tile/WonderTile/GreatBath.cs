using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    // 대욕장
    public class GreatBath : WonderTile
    {
        [SerializeField, Header("Bonus Point")]
        private int wonderBonus = 2;

        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // currentTile이 강 타일일 때 여기에 성지를 지으면 +2점
        public override void WonderFunction(Tile currentTile, Tile selectedTile)
        {
            if (currentTile.MyTileType == TileType.River)
            {
                if (selectedTile.MyTileType == TileType.HolyLand)
                {
                    selectedTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // currentTile이 강 타일이라면 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.River)
            {
                return true;
            }

            return false;
        }
    }
}