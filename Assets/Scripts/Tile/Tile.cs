using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Procedural;

namespace TilePuzzle
{
    public class Tile : MonoBehaviour
    {
        public HexagonTileObject hexagonTileObject;

        [ShowInInspector]
        public TileInfo MyHexagonInfo => hexagonTileObject.TileInfo;

        [ShowInInspector]
        public DecorationInfo MyDecorationInfo => hexagonTileObject.DecorationInfo.GetValueOrDefault();

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
        public GameObject RangeGrid
        {
            get
            {
                if (rangeGrid == null)
                {
                    MakeGrid(TileManager.Instance.GridPrefab);
                }

                return rangeGrid;
            }
            private set
            {
                rangeGrid = value;
            }
        }

        private GameObject rangeGrid;

        // 이 타일을 소유하고 있는 도시
        public CityTile OwnerCity
        {
            get { return ownerCity; }
            private set
            {
                if (!RangeCitys.Contains(value))
                {
                    RangeCitys.Add(value);
                }

                ownerCity = value;
            }
        }
        [SerializeField, ReadOnly, Header("Owner Of This Tile")]
        private CityTile ownerCity = null;

        public List<CityTile> RangeCitys { get; private set; } = new List<CityTile>();

        // 타일 설치 비용
        public int Cost { get { return cost; } set { cost = value; } }
        [SerializeField, Space, Header("Tile Cost")]
        private int cost = 0;

        // 모델 밑으로 조금 내림
        public float DownOffset = 0.2f;

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
        public void InitInfo(HexagonTileObject tileObject)
        {
            hexagonTileObject = tileObject;

            // 일단 1개만 들어있다고 가정
            foreach (var tag in MyHexagonInfo.biome.tags)
            {
                myTileTerrain = (TileTerrain)System.Enum.Parse(typeof(TileTerrain), tag);
            }

            // 얹힌 것도 추가
            if (MyDecorationInfo.type == DecorationInfo.Type.Forest)
            {
                if (MyTileTerrain == TileTerrain.RainForest)
                {
                    myTileFeature = TileFeature.Jungle;
                }
                else
                {
                    myTileFeature = TileFeature.Forest;
                }
            }
        }

        // Info 초기화
        public void InitInfo(HexagonTileObject tileObject, int range)
        {
            baseRange = range;
            Range = range;

            InitInfo(tileObject);
        }

        // 내 타일이 pivotBuilding의 보너스에 해당하는지 검사하고 해당 점수 return
        public float CountSpecificBonus(TileBuilding pivotBuilding)
        {
            float bonusPoint = 0;

            var tileData = DataTableManager.Instance.GetBuildingData(AgeManager.Instance.WorldAge, pivotBuilding);

            if (tileData == null)
            {
                return 0;
            }

            if (MyTileBuilding != TileBuilding.Empty)
            {
                // 보너스 건물에 속하는지 검사하고 보너스 계산.
                foreach (var bonusPerBuilding in tileData.BonusPerBuildings)
                {
                    // 모든 건물 보너스 이거나
                    // 혹은 이 타일의 건물이 보너스에 해당하면
                    // 보너스 획득
                    if (bonusPerBuilding.MyBuilding == TileBuilding.Empty ||
                        bonusPerBuilding.MyBuilding == MyTileBuilding)
                    {
                        bonusPoint += bonusPerBuilding.Bonus;
                    }
                }
            }

            if (MyTileFeature != TileFeature.Empty)
            {
                // 이 타일의 나무(정글)이 보너스에 속하는지 검사하고 보너스 계산
                foreach (var bonusPerFeature in tileData.BonusPerFeatures)
                {
                    if (bonusPerFeature.MyFeature == MyTileFeature)
                    {
                        bonusPoint += bonusPerFeature.Bonus;
                    }
                }
            }

            if (MyTileType != TileType.Empty)
            {
                // 이 타일의 지형 타입이 보너스에 속하는지 검사하고 보너스 계산
                foreach (var bonusPerType in tileData.BonusPerTypes)
                {
                    if (bonusPerType.MyType == MyTileType)
                    {
                        bonusPoint += bonusPerType.Bonus;
                    }
                }
            }

            return bonusPoint;
        }

        // tileBuilding이 설치 되었을 때의
        // 예상 보너스 return
        public int CalculateBonus(TileBuilding tileBuilding)
        {
            // 특수지구 개수. 2개당 +1
            int buildingCount = 0;
            // 특수지구 별 보너스 점수
            float specificBonus = 0;

            var buildingData = DataTableManager.Instance.GetBuildingData(AgeManager.Instance.WorldAge, tileBuilding);

            if (buildingData == null)
            {
                return 0;
            }

            int baseBonus = buildingData.BaseBonus;

            // 특수지구 개수를 세고, 내 타일 빌딩의 보너스 추가
            for (int i = 0; i < RangeTiles.Count; i++)
            {
                if (RangeTiles[i] is BuildingTile)
                {
                    buildingCount += 1;
                }

                specificBonus += RangeTiles[i].CountSpecificBonus(tileBuilding);
            }

            // 주둔지는 기본 10점, 범위 안의 도시타일 1개마다 -1점
            if (tileBuilding == TileBuilding.Encampment)
            {
                return  baseBonus + (int)specificBonus;
            }
            // 유흥단지는 범위 안의 도시 타일 1개마다 +2점
            else if (tileBuilding == TileBuilding.EntertainmentComplex)
            {
                return baseBonus + (int)specificBonus;
            }
            else
            {
                return baseBonus + buildingCount / 2 + (int)specificBonus;
            }
        }

        // 격자 생성
        // 생성 성공시 true return
        public bool MakeGrid(GameObject grid)
        {
            // 아직 설치되지 않은 타일이라면 그리드 생성 금지
            if (hexagonTileObject == null)
            {
                return false;
            }

            RangeGrid = Instantiate(grid, transform);
            RangeGrid.transform.position = hexagonTileObject.land.transform.position + Vector3.down * DownOffset;

            return true;
        }

        // 격자 on off
        public void TurnGrid(bool isOn)
        {
            if (RangeGrid == null)
            {
                if (!MakeGrid(TileManager.Instance.GridPrefab))
                {
                    return;
                }
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

            bool isBuilding = MyTileBuilding != TileBuilding.Empty &&
                MyTileBuilding != TileBuilding.Wonder &&
                MyTileBuilding != TileBuilding.City;

            if (isBuilding)
            {
                cityTile.ownBuildings.Add(this);
            }

            OwnerCity = cityTile;
        }

        public void SetRangeCitys(List<CityTile> cityTiles)
        {
            RangeCitys = cityTiles;
        }

        // 현재 범위 내 도시 중
        // 이 타일에 설치된 건물을 소지 하지 않은 도시가 있다면
        // 그 도시에게 소유권을 넘기고 true return
        // 없다면 false return
        public bool TryChangeOwner(List<CityTile> checkedCitys)
        {
            foreach (var cityTile in RangeCitys)
            {
                if (checkedCitys.Contains(cityTile))
                {
                    continue;
                }

                if (!cityTile.HasThatTile(MyTileBuilding, false))
                {
                    SetCityTile(cityTile);
                    return true;
                }
            }

            return false;
        }

        // 범위 내 도시 타일에 인자로 받은 도시 타일이 있다면
        // 그 타일로 교체
        // 외부에서 직접 접근할 수 있으므로
        // 빈 타일만 교체 가능
        public bool TryChangeOwner(CityTile changeCity)
        {
            if (MyTileBuilding != TileBuilding.Empty)
            {
                return false;
            }

            if (RangeCitys.Contains(changeCity))
            {
                SetCityTile(changeCity);
                return true;
            }

            return false;
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