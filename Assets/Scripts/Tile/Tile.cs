using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public enum TileType { Water, River, Mountain, Ground, 
        Campus, Factory, GovernmentBuilding, HolyLand, 
        Theator, WaterPipe, City, Empty }

    [System.Serializable]
    public struct Position
    {
        public int Row;
        public int Column;

        public Position(int myRow, int myColumn)
        {
            Row = myRow;
            Column = myColumn;
        }
    }

    public class Tile : MonoBehaviour
    {
        // 이 타일의 타입
        public TileType MyTileType { get { return myTileType; } private set { myTileType = value; } }
        [SerializeField]
        private TileType myTileType = TileType.Empty;
                
        // 이웃한 타일
        public List<Tile> NeighborTiles { get { return neighborTiles; } private set { neighborTiles = value; } }
        [SerializeField, Space]
        [ReadOnly]
        private List<Tile> neighborTiles;

        // 영역 범위, 영역 내 타일
        public int Range { get { return range; } private set { range = value; } }
        [SerializeField]
        private int range = 2;
        public List<Tile> RangeTiles { get { return rangeTiles; } private set { rangeTiles = value; } }
        [SerializeField, Header("Tiles In Range")]
        [ReadOnly]
        private List<Tile> rangeTiles;

        // 이 타일이 받는 보너스
        public int Bonus { get { return bonus; } protected set { bonus = value; } }
        [SerializeField, Space]
        [ReadOnly]
        private int bonus = 0;

        // 이 타일의 위치
        public Position MyPosition { get { return myPosition; } private set { myPosition = value; } }
        [SerializeField, Space]
        [ReadOnly]
        private Position myPosition = new Position(0, 0);

        // 범위 표시용 격자
        public GameObject RangeGrid { get; private set; }

        // 이 타일을 소유하고 있는 도시
        public CityTile OwnerCity { get; private set; } = null;

        public void ChangeTileType(TileType tileType)
        {
            MyTileType = tileType;
        }

        public void InitTile(TileType tileType, int row, int column)
        {
            ChangeTileType(tileType);
            InitPosition(row, column);
        }

        public void InitNeighborTiles(List<Tile> neighbors)
        {
            neighborTiles = neighbors;
        }

        private void InitPosition(int row, int column)
        {
            MyPosition = new Position(row, column);
        }

        // 범위 내의 타일 초기화
        public void InitRangeTiles()
        {
            List<Tile> rangeTiles = new List<Tile>(NeighborTiles);

            for (int i = 0; i < NeighborTiles.Count; i++)
            {
                NeighborTiles[i].initRangeTiles(rangeTiles, Range);
            }

            RangeTiles = rangeTiles;
        }

        public void InitRangeTiles(List<Tile> tiles)
        {
            RangeTiles = tiles;
        }

        // 내 이웃 타일 중 rangeList에 들어있지 않은 타일이 있다면 넣어줌.
        // rangeCount의 범위 내의 타일만 들어가게 됨.
        private void initRangeTiles(List<Tile> rangeList, int rangeCount)
        {
            if (rangeCount <= 1)
            {
                return;
            }

            foreach (var neighborTile in NeighborTiles)
            {
                if (!rangeList.Contains(neighborTile))
                {
                    rangeList.Add(neighborTile);
                    neighborTile.initRangeTiles(rangeList, rangeCount - 1);
                }
            }
        }

        public void ChangeMaterial(bool isSelected)
        {
            MeshRenderer myRenderer = GetComponent<MeshRenderer>();
            MeshRenderer childRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();

            if (isSelected)
            {
                myRenderer.material = TileManager.Instance.SelectedMaterial;
                childRenderer.material = TileManager.Instance.SelectedMaterial;
            }
            else
            {
                myRenderer.material = TileManager.Instance.NormalMaterial;
                childRenderer.material = TileManager.Instance.NormalMaterial;
            }
        }

        // 이웃 타일들에게서 prev를 제거하고 current를 넣음
        public void UpdateNeighborTile(Tile prev, Tile current)
        {
            for (int i = 0; i < NeighborTiles.Count; i++)
            {
                NeighborTiles[i].updateNeighborTile(prev, current);
            }
        }

        // 내 이웃 타일에서 prev를 제거하고 current를 넣음
        private void updateNeighborTile(Tile prev, Tile current)
        {
            NeighborTiles.Remove(prev);
            NeighborTiles.Add(current);
        }

        // 내 Range 타일에서 prev를 제거하고 current를 넣음
        public void UpdateRangeTile(Tile prev, Tile current)
        {
            if (RangeTiles.Contains(prev))
            {
                RangeTiles.Remove(prev);
                RangeTiles.Add(current);
            }
        }

        // 내 타일이 pivotType의 보너스에 해당하는지 검사하고 해당 점수 return
        public int CountSpecificBonus(TileType pivotType)
        {
            int bonusPoint = 0;

            if (MyTileType == TileType.GovernmentBuilding)
            {
                bonusPoint = 1;
            }
            else if (pivotType == TileType.Campus)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotType == TileType.Factory)
            {
                if (MyTileType == TileType.WaterPipe)
                {
                    bonusPoint = 2;
                }
            }
            else if (pivotType == TileType.HolyLand)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotType == TileType.Theator)
            {
                // 나중에 불가사의 추가할 것
            }

            return bonusPoint;
        }

        // 격자 생성
        public void MakeGrid(GameObject grid)
        {
            RangeGrid = Instantiate(grid, transform);
            RangeGrid.transform.localPosition = Vector3.zero;
        }

        // 격자 on off
        public void TurnGrid(bool isOn)
        {
            if (isOn)
            {
                RangeGrid.SetActive(true);
            }
            else
            {
                RangeGrid.SetActive(false);
            }
        }

        // 범위 내의 격자 on off
        public void TurnRangeGrid(bool isOn)
        { 
            foreach(Tile rangeTile in RangeTiles)
            {
                rangeTile.TurnGrid(isOn);
            }
        }

        public void SetCityTile(CityTile cityTile)
        {
            if (cityTile == null)
            {
                Debug.LogError("인자 도시 타일이 null 값임");
                return;
            }

            OwnerCity = cityTile;
        }
    }
}