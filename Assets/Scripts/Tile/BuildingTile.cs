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
            if (!(this is BuildingTile))
            {
                return;
            }

            // 포인트가 없는 타일은 패스
            if (MyTileType == TileType.City)
            {
                return;
            }
            else if (MyTileType == TileType.GovernmentBuilding)
            {
                return;
            }
            else if (MyTileType == TileType.WaterPipe)
            {
                return;
            }
            else if (MyTileType == TileType.Empty)
            {
                Debug.LogError(string.Format("Error Tile Exist : {0}, {1}", name, transform.GetSiblingIndex()));
                return;
            }

            // 특수지구 개수. 2개당 +1
            int buildingCount = 0;
            // 특수지구 별 보너스 점수
            int specificBonus = 0;

            // 특수지구 개수를 세고, 내 타일 타입의 보너스 추가
            for (int i = 0; i < RangeTiles.Count; i++)
            {
                if (RangeTiles[i] is BuildingTile)
                {
                    buildingCount += 1;
                }

                specificBonus += RangeTiles[i].CountSpecificBonus(MyTileType);
            }

            Bonus = buildingCount / 2 + specificBonus;
        }
    }
}