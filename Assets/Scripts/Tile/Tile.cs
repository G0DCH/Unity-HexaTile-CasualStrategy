using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Procedural;

namespace TilePuzzle
{
    public class Tile : MonoBehaviour
    {
        public Hexagon MyHexagon
        {
            get
            {
                if (myHexagon == null)
                {
                    Hexagon hexagon = GetComponent<Hexagon>();
                    myHexagon = hexagon;
                }

                return myHexagon;
            }
            set
            {
                myHexagon = value;
            }
        }
        [SerializeField]
        private Hexagon myHexagon = null;

        // 이 타일의 타입
        public TileType MyTileType
        {
            get
            {
                if (myTileType == TileType.Empty)
                {
                    if (MyHexagon.HasMountain)
                    {
                        myTileType = TileType.Mountain;
                    }
                    else
                    {
                        if (MyHexagon.IsLand)
                        {
                            myTileType = TileType.Ground;

                            if (MyHexagon.HasRiver)
                            {
                                myTileType = TileType.River;
                            }
                        }
                        else
                        {
                            myTileType = TileType.Water;
                        }
                    }
                }
                return myTileType;
            }
            set
            {
                myTileType = value;
            }
        }
        [SerializeField]
        private TileType myTileType = TileType.Empty;

        public TileBuilding MyTileBuilding { get { return myTileBuilding; } set { myTileBuilding = value; } }
        [SerializeField]
        private TileBuilding myTileBuilding = TileBuilding.Empty;

        // 타일에 얹힌 열대우림이나 숲
        public TileFeature MyTileFeature
        {
            get
            {
                if (myTileFeature == TileFeature.Empty)
                {
                    if (MyHexagon.HasForest)
                    {
                        myTileFeature = TileFeature.Forest;
                    }
                }
                return myTileFeature;
            }
            set
            {
                myTileFeature = value;
            }
        }
        [SerializeField]
        private TileFeature myTileFeature = TileFeature.Empty;

        // 타일의 지형 특성. 평원, 설원 등등
        public TileTerrain MyTileTerrain { get { return myTileTerrain; } private set { myTileTerrain = value; } }
        [SerializeField]
        private TileTerrain myTileTerrain = TileTerrain.Plains;

        // 이웃한 타일
        public List<Tile> NeighborTiles
        {
            get
            {
                if (neighborTiles.Count == 0)
                {
                    neighborTiles = GetRangeTiles(1);
                }

                return neighborTiles;
            }
        }
        [SerializeField, Space]
        [ReadOnly]
        private List<Tile> neighborTiles = new List<Tile>();

        // 영역 범위, 영역 내 타일
        public int Range { get { return range; } private set { range = value; } }
        [SerializeField]
        private int range = 2;
        public List<Tile> RangeTiles
        {
            get
            {
                if (rangeTiles.Count == 0)
                {
                    rangeTiles = GetRangeTiles(Range);
                }

                return rangeTiles;
            }
        }
        [SerializeField, Header("Tiles In Range")]
        [ReadOnly]
        private List<Tile> rangeTiles = new List<Tile>();

        // 이 타일이 받는 보너스
        public int Bonus { get { return bonus; } protected set { bonus = value; } }
        [SerializeField, Space]
        [ReadOnly]
        private int bonus = 0;

        // 범위 표시용 격자
        public GameObject RangeGrid { get; private set; }

        // 이 타일을 소유하고 있는 도시
        public CityTile OwnerCity { get { return ownerCity; } private set { ownerCity = value; } }
        [SerializeField, ReadOnly, Header("Owner Of This Tile")]
        private CityTile ownerCity = null;

        // 타일 설치 비용
        public int Cost { get { return cost; } set { cost = value; } }
        [SerializeField, Space, Header("Tile Cost")]
        private int cost = 0;

        private List<Tile> GetRangeTiles(int range)
        {
            IEnumerable<Hexagon> neighborHexagons = Procedural.Terrain.Instance.GetNeighborHexagons(MyHexagon.hexPos, range);
            List<Tile> neighborTiles = new List<Tile>();

            foreach(Hexagon neighbor in neighborHexagons)
            {
                Tile tile = neighbor.gameObject.GetComponent<Tile>();

                if (tile == null)
                {
                    Debug.LogError(string.Format("No Tile Component At {0}", neighbor.hexPos));
                    return null;
                }

                neighborTiles.Add(neighbor.GetComponent<Tile>());
            }

            return neighborTiles;
        }

        // 내 타일이 pivotBuilding의 보너스에 해당하는지 검사하고 해당 점수 return
        public int CountSpecificBonus(TileBuilding pivotBuilding)
        {
            int bonusPoint = 0;

            if (MyTileBuilding == TileBuilding.GovernmentBuilding)
            {
                bonusPoint = 1;
            }
            else if (pivotBuilding == TileBuilding.Campus)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotBuilding == TileBuilding.Factory)
            {
                if (MyTileBuilding == TileBuilding.WaterPipe)
                {
                    bonusPoint = 2;
                }
            }
            else if (pivotBuilding == TileBuilding.HolyLand)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotBuilding == TileBuilding.Theator)
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
            if (RangeGrid == null)
            {
                MakeGrid(TileManager.Instance.GridPrefab);
            }

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
            foreach (Tile rangeTile in RangeTiles)
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

        // 이 타일의 보너스를 n만큼 변경함
        public void ChangeBonus(int n)
        {
            Bonus += n;
        }

        // 이 타일의 비용을 n만큼 변경함
        public void ChangeCost(int n)
        {
            Cost += n;
        }
    }
}