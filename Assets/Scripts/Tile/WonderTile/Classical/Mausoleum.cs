using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    // 할리카르나소스 마우솔로스 영묘
    public class Mausoleum : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 물 타일 옆에 건물을 지으면 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.Water)
                {
                    currentTile.ChangeBonus(wonderBonus);
                    return;
                }
            }
        }

        // 항만 옆 육지에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.Harbor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}