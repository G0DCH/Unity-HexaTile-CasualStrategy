using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    public class CityTile : BuildingTile
    {
        // 도시 범위 표기용 격자
        private List<GameObject> grids = new List<GameObject>();

        private void Start()
        {
            SetCityTile(this);
        }

        // 범위 내 타일의 격자를 도시 타일에 복제함.
        public void SetRangeGrids()
        {
            // 기존에 존재한 격자 제거
            foreach(var grid in grids)
            {
                Destroy(grid);
            }

            foreach(var rangeTile in RangeTiles)
            {
                GameObject grid = Instantiate(rangeTile.RangeGrid, transform, true);
                grids.Add(grid);
            }

            TurnGrids(true);
        }

        // 범위 내 타일의 소유 도시를 나로 설정
        public void SetOwnerInRange()
        {
            foreach (var rangeTile in RangeTiles)
            {
                rangeTile.SetCityTile(this);
            }
        }

        // 범위 내 해당 타입의 타일이 존재하는지 검사
        public bool HasThatTile(TileType tileType)
        {
            foreach(var rangeTile in RangeTiles)
            {
                // 존재 한다면 true return
                if (rangeTile.MyTileType == tileType)
                {
                    return true;
                }
            }

            return false;
        }

        // 범위 표기용 격자 on off
        public void TurnGrids(bool isOn)
        {
            foreach(GameObject grid in grids)
            {
                grid.SetActive(isOn);
            }
        }
    }
}
