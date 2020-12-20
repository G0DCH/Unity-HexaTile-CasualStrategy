using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    public class BuildingTile : Tile
    {
        // 내 타일의 보너스 갱신
        public void RefreshBonus()
        {
            // 포인트가 없는 타일은 패스
            if (MyTileBuilding == TileBuilding.City)
            {
                return;
            }
            else if (MyTileBuilding == TileBuilding.GovernmentPlaza)
            {
                return;
            }
            else if (MyTileBuilding == TileBuilding.Aqueduct)
            {
                return;
            }
            else if (MyTileBuilding == TileBuilding.Carnal)
            {
                return;
            }
            else if (MyTileBuilding == TileBuilding.Empty)
            {
                Debug.LogError(string.Format("Error Tile Exist : {0}, {1}", name, transform.GetSiblingIndex()));
                return;
            }

            CalculateBonus(MyTileBuilding);
        }
    }
}