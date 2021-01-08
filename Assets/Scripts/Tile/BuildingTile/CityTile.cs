using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Entities;

namespace TilePuzzle
{
    public class CityTile : BuildingTile
    {
        // 도시 범위 표기용 격자
        private List<GameObject> grids = new List<GameObject>();
        private static List<CityTile> checkedCitys = new List<CityTile>();

        public Entity Entity { get; private set; } = null;

        public List<Tile> ownBuildings = new List<Tile>();

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

            TurnCityGrids(true);
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
        // checkStart는 검사 시작 유무
        // true면 리스트 초기화, 아니면 초기화 안함.
        public bool HasThatTile(TileBuilding tileBuilding, bool checkStart)
        {
            if (checkStart)
            {
                checkedCitys.Clear();
            }

            // 도시 범위 내에 도시를 설치할 수 없음.
            if (tileBuilding == TileBuilding.City)
            {
                return true;
            }

            foreach(var ownBuilding in ownBuildings)
            {
                if (ownBuilding.MyTileBuilding == tileBuilding &&
                    ownBuilding.OwnerCity == this)
                {
                    checkedCitys.Add(this);
                    // 소유 도시를 변경 해보고
                    // 변경 했다면 false return
                    if (ownBuilding.TryChangeOwner(checkedCitys))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // 범위 표기용 격자 on off
        private void TurnCityGrids(bool isOn)
        {
            foreach(GameObject grid in grids)
            {
                grid.SetActive(isOn);
            }
        }
    }
}
