using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Procedural;

namespace TilePuzzle
{
    public class Tile : MonoBehaviour
    {
        public HexagonInfo MyHexagonInfo
        {
            get
            {
                return myHexagonInfo;
            }
            set
            {
                myHexagonInfo = value;
            }
        }
        [SerializeField]
        private HexagonInfo myHexagonInfo = new HexagonInfo();

        public DecorationInfo MyDecorationInfo
        {
            get
            {
                return myDecorationInfo;
            }
            set
            {
                myDecorationInfo = value;
            }
        }
        [SerializeField]
        private DecorationInfo myDecorationInfo = new DecorationInfo();

        // 이 타일의 타입
        public TileType MyTileType
        {
            get
            {
                if (myTileType == TileType.Empty)
                {
                    if (MyDecorationInfo.type == DecorationInfo.Type.Mountain)
                    {
                        myTileType = TileType.Mountain;
                    }
                    else
                    {
                        if (!MyHexagonInfo.isWater)
                        {
                            myTileType = TileType.Ground;

                            if (MyHexagonInfo.hasRiver)
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
                    if (MyDecorationInfo.type == DecorationInfo.Type.Forest)
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
                    neighborTiles = TileManager.Instance.GetRangeTiles(this, 1);
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
        private int baseRange = 2;// 기본 범위
        public List<Tile> RangeTiles
        {
            get
            {
                if (rangeTiles.Count == 0)
                {
                    rangeTiles = TileManager.Instance.GetRangeTiles(this, Range);
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

        // HexagonInfo, DecorationInfo가 초기화 되었는지 확인
        private bool isInit = false;

        #region 범위 불가사의 on off
        // 다음 불가사의의 효과를 받았는가
        // 킬와 키시와니
        // 성 바실리 대성당
        // 옥스퍼드 대학
        // 브로드 웨이
        // 리오의 예수상
        private bool activeKilwa = false;
        private bool activeBasils = false;
        private bool activeOxford = false;
        private bool activeBroadway = false;
        private bool activeCristo = false;
        #endregion

        private void Awake()
        {
            baseRange = Range;
        }

        // Info 초기화
        public void InitInfo(HexagonInfo hexagonInfo, DecorationInfo decorationInfo)
        {
            myHexagonInfo = hexagonInfo;
            myDecorationInfo = decorationInfo;

            isInit = true;
        }

        // Info 초기화
        public void InitInfo(HexagonInfo hexagonInfo, DecorationInfo decorationInfo, int range)
        {
            myHexagonInfo = hexagonInfo;
            myDecorationInfo = decorationInfo;
            baseRange = range;
            Range = range;

            isInit = true;
        }


        // 내 타일이 pivotBuilding의 보너스에 해당하는지 검사하고 해당 점수 return
        public float CountSpecificBonus(TileBuilding pivotBuilding)
        {
            float bonusPoint = 0;

            if (MyTileBuilding == TileBuilding.GovernmentPlaza)
            {
                bonusPoint = 1;
            }
            // 캠퍼스
            else if (pivotBuilding == TileBuilding.Campus)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
                else if (MyTileFeature == TileFeature.RainForest)
                {
                    bonusPoint = 0.5f;
                }
            }
            // 산업구역
            else if (pivotBuilding == TileBuilding.IndustrialZone)
            {
                if (MyTileBuilding == TileBuilding.Aqueduct ||
                    MyTileBuilding == TileBuilding.Carnal)
                {
                    bonusPoint = 2;
                }
            }
            // 성지
            else if (pivotBuilding == TileBuilding.HolySite)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
                else if (MyTileFeature == TileFeature.Forest)
                {
                    bonusPoint = 0.5f;
                }
            }
            // 주둔지
            else if (pivotBuilding == TileBuilding.Encampment)
            {
                if (MyTileBuilding == TileBuilding.City)
                {
                    bonusPoint = -1;
                }
            }
            // 항만
            else if (pivotBuilding == TileBuilding.Harbor)
            {
                if (MyTileBuilding == TileBuilding.City)
                {
                    bonusPoint = 2;
                }
            }
            // 상업 중심지
            else if (pivotBuilding == TileBuilding.CommercialHub)
            {
                if (MyTileBuilding == TileBuilding.Harbor)
                {
                    bonusPoint += 2;
                }
                if (MyTileType == TileType.River)
                {
                    bonusPoint += 1;
                }
            }
            // 유흥단지
            else if (pivotBuilding == TileBuilding.EntertainmentComplex)
            {
                if (MyTileBuilding == TileBuilding.City)
                {
                    bonusPoint = 2;
                }
            }
            // 극장가
            else if (pivotBuilding == TileBuilding.TheaterSquare)
            {
                if (MyTileBuilding == TileBuilding.Wonder)
                {
                    bonusPoint = 2;
                }
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

        // 범위 내 타일 갱신
        public void UpdateRangeTiles()
        {
            if (baseRange != Range)
            {
                rangeTiles = TileManager.Instance.GetRangeTiles(this, Range);
            }
        }

        // 범위 내 타일과 이웃 타일 갱신
        public void UpdateNeighborRange()
        {
            neighborTiles = TileManager.Instance.GetRangeTiles(this, 1);
            rangeTiles = TileManager.Instance.GetRangeTiles(this, Range);
        }

        // 범위 변경 용 bool 값 변경
        public void ChangeRange(WonderTile wonder, bool active)
        {
            if (wonder is KilwaKisiwani)
            {
                activeKilwa = active;
            }
            if (wonder is SaintBasils)
            {
                activeBasils = active;
            }
            if (wonder is OxfordUniv)
            {
                activeOxford = active;
            }
            if (wonder is BroadWay)
            {
                activeBroadway = active;
            }
            if (wonder is CristoRedentor)
            {
                activeCristo = active;
            }

            ChangeRange();
        }

        // 진짜 범위 변경
        private void ChangeRange()
        {
            Range = baseRange;

            if (activeKilwa)
            {
                Range += 1;
            }
            if (activeBasils)
            {
                Range += 1;
            }
            if (activeOxford)
            {
                Range += 1;
            }
            if (activeBroadway)
            {
                Range += 1;
            }
            if (activeCristo)
            {
                Range += 1;
            }
        }
    }
}