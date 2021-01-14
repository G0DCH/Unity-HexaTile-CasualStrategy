using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Procedural;

namespace TilePuzzle
{
    public class TileManager : Utility.Singleton<TileManager>
    {
        // 범위 표기 프리팹
        public GameObject GridPrefab;
        public TilePlacementDrawer tilePlacementDrawer;

        [Space]
        [Header("Selected Tile")]
        [ReadOnly]
        private Tile selectedTile;
        public Tile SelectedTile
        {
            get { return selectedTile; }
            set
            {
                tilePlacementDrawer.PlacementObject = value != null ? value.gameObject : null;
                selectedTile = value;
            }
        }

        [Space, Header("SelectTileCost")]
        public int SelectTileCost = 0;

        // 이전에 마우스가 위에 올라가있던 타일
        private Tile prevOverTile = null;

        private int pointerID = -1;

        // 점수 보너스 계산 delegate
        public delegate void WonderBonus(Tile currentTile, TileBuilding tileBuilding);
        public WonderBonus CalculateBonusByWonder = null;

        // 소모 비용 계산 delegate
        public delegate void WonderCost(Tile currentTile, TileBuilding tileBuilding);
        public WonderCost CalculateCost = null;

        // 범위 보너스 delegate
        public delegate void WonderRange(Tile currentTile, TileBuilding tileBuilding);
        public WonderRange CalculateRange = null;

        private Dictionary<HexagonPos, Tile> TileMap = new Dictionary<HexagonPos, Tile>();

        // 도시 타일 갯수
        public int CityNum { get { return cityNum; } private set { cityNum = value; } }
        [Space, SerializeField, ReadOnly]
        private int cityNum = 0;

        [Space, SerializeField]
        private GameObject TileContainer = null;

        private const string tileContainerName = "TileContainer";

        private void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            pointerID = -1;
#endif

#if UNITY_IOS || UNITY_ANDROID
            pointerID = 0;
#endif
            StartCoroutine(MouseOverAction());
            StartCoroutine(TileClickAction());
        }

        public void InitTileMap()
        {
            Vector2Int terrainSize = GameManager.Instance.terrainGenerateSettings.terrainSize;
            HexagonTerrain hexagonTerrain = GameManager.Instance.MyHexagonTerrain;

            for (int x = 0; x < terrainSize.x; x++)
            {
                for (int y = 0; y < terrainSize.y; y++)
                {
                    HexagonTileObject hexagonTileObject = hexagonTerrain.GetHexagonTile(HexagonPos.FromArrayXY(x, y));
                    Tile tile = Instantiate(TileContainer, hexagonTileObject.transform).GetComponent<Tile>();
                    tile.transform.localPosition = Vector3.zero;
                    tile.name = tileContainerName;
                    tile.InitInfo(hexagonTileObject);

                    TileMap.Add(hexagonTileObject.TileInfo.hexPos, tile);
                }
            }
        }

        // 범위 내 타일 return
        public List<Tile> GetRangeTiles(Tile myTile, int range)
        {
            IEnumerable<HexagonTileObject> neighborHexagons = GameManager.Instance.MyHexagonTerrain.GetHexagonTiles(myTile.MyHexagonInfo.hexPos, new RangeInt(1, range - 1));
            List<Tile> neighborTiles = new List<Tile>();

            foreach (HexagonTileObject neighbor in neighborHexagons)
            {
                neighborTiles.Add(TileMap[neighbor.TileInfo.hexPos]);
            }

            return neighborTiles;
        }

        // 마우스가 타일 맵 위에 올라갔을 때의 행동들
        // 격자 갱신, 범위 계산, 비용 계산을 함.
        private IEnumerator MouseOverAction()
        {
            while (true)
            {
                if (SelectedTile == null)
                {
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerID))
                {
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                Tile overTile = GetTileAtMousePos();
                CalculateTileCost(overTile);
                bool canPutTile = CanPutTile(overTile);

                UpdateGrid(overTile, canPutTile);
                prevOverTile = overTile;
                if (SelectedTile == null || overTile == null)
                {
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                SelectedTile.transform.position = overTile.hexagonTileObject.land.transform.position + Vector3.up * 0.1f;
                tilePlacementDrawer.IsTilePlaceable = canPutTile;

                if (!canPutTile)
                {
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                // 범위 계산, 범위 내 타일 갱신
                CalculateRange?.Invoke(overTile, SelectedTile.MyTileBuilding);

                // 선택한 타일이 설치 되었을 때의 점수 계산
                int expectedBonus = overTile.CalculateBonus(selectedTile.MyTileBuilding);
                UIManager.Instance.ShowExpectBonus(expectedBonus, overTile.transform.position);

                // TODO : 없어도 되는지 검사
                //overTile.UpdateRangeTiles();

                yield return new WaitForSeconds(0.02f);
            }
        }

        // 타일을 클릭 했을 때의 행동들, 해당 위치에 타일을 놓음
        private IEnumerator TileClickAction()
        {
            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (SelectedTile == null)
                    {
                        yield return null;
                        continue;
                    }

                    if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerID))
                    {
                        yield return null;
                        continue;
                    }

                    Tile clickedTile = GetTileAtMousePos();

                    if (clickedTile == null)
                    {
                        yield return null;
                    }

                    if (!CanPutTile(clickedTile))
                    {
                        yield return new WaitForSeconds(0.02f);
                        continue;
                    }
                    UIManager.Instance.TurnExpectBonus(false);

                    tilePlacementDrawer.PlacementObject = null;
                    TileBuilding tileBuilding = SelectedTile.MyTileBuilding;

                    // 클릭한 타일의 컴포넌트를
                    // 설치하려는 타일로 교체
                    Tile changedTile = ChangeTile(clickedTile);

                    // 건물 보너스 갱신
                    if (changedTile is BuildingTile)
                    {
                        ((BuildingTile)changedTile).RefreshBonus();
                    }
                    // 불가사의로 인한 보너스 추가, 보너스 출력
                    CalculateBonusByWonder?.Invoke(changedTile, tileBuilding);
                    // 시대별 업그레이드 보너스 추가
                    //MyBuildingBonus?.Invoke(changedTile, tileBuilding);

                    if (changedTile is WonderTile)
                    {
                        // 딜리케이트 추가
                        ((WonderTile)changedTile).AddToDelegate();
                        // 설치한 불가사의 버튼 비활성화
                        UIManager.Instance.DisableWonderButton();
                    }

                    GameManager.Instance.AddPoint(changedTile.Bonus);

                    // 건물 모델을 타일 위에 올림
                    Transform building = SelectedTile.transform.GetChild(0);
                    building.SetParent(changedTile.transform, true);

                    // 건물 모델 위치 조정
                    if (changedTile is WonderTile)
                    {
                        building.position = changedTile.hexagonTileObject.land.transform.position + Vector3.up * changedTile.DownOffset;
                    }
                    else
                    {
                        building.position = changedTile.hexagonTileObject.land.transform.position + Vector3.down * changedTile.DownOffset;
                    }

                    // 기존 데코레이션 삭제
                    changedTile.hexagonTileObject.DestroyDecoration();

                    Destroy(SelectedTile.gameObject);
                    SelectedTile = null;
                }

                yield return null;
            }
        }

        // 마우스 포인터 위치의 타일 return
        // 없으면 null return
        private Tile GetTileAtMousePos()
        {
            Tile clickedTile = null;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit))
            {
                clickedTile = hit.transform.Find(tileContainerName).GetComponent<Tile>();
            }

            return clickedTile;
        }

        // 이전 타일 격자 off
        // 현재 타일 격자 on
        private void UpdateGrid(Tile currentOverTile, bool canPutTile)
        {
            if (prevOverTile != null)
            {
                if (prevOverTile != currentOverTile)
                {
                    prevOverTile.TurnRangeGrid(false);
                }
            }

            if (currentOverTile != null && canPutTile)
            {
                currentOverTile.TurnRangeGrid(true);
            }
        }

        // 현재 타일의 비용 계산
        private void CalculateTileCost(Tile currentTile)
        {
            // 코스트 계산
            if (SelectedTile.MyTileBuilding == TileBuilding.City)
            {
                SelectTileCost = CityNum;
            }
            else
            {
                SelectTileCost = SelectedTile.Cost;
            }

            CalculateCost?.Invoke(currentTile, SelectedTile.MyTileBuilding);
        }

        // targetTile을 SelectedTile로 교체
        private Tile ChangeTile(Tile targetTile)
        {
            // 기존 타일 컴포넌트 제거
            GameObject clickedObject = targetTile.gameObject;
            HexagonTileObject hexagonTileObject = targetTile.hexagonTileObject;
            int range = targetTile.Range;
            CityTile city = targetTile.OwnerCity;
            List<CityTile> rangeCitys = targetTile.RangeCitys;
            TileMap.Remove(hexagonTileObject.TileInfo.hexPos);
            Destroy(targetTile);

            // Tile 컴포넌트를 SelectTile의 컴포넌트로 교체
            Tile newTile = (Tile)clickedObject.AddComponent(SelectedTile.GetType());
            newTile.InitInfo(hexagonTileObject, range);
            newTile.MyTileBuilding = SelectedTile.MyTileBuilding;

            // 타일 속성에 맞는 초기화 진행
            if (SelectedTile is CityTile)
            {
                ((CityTile)newTile).SetRangeGrids();
                ((CityTile)newTile).SetOwnerInRange();
                // 도시 개수 증가
                CityNum++;
            }
            else
            {
                newTile.SetRangeCitys(rangeCitys);
                newTile.SetCityTile(city);

                if (SelectedTile is WonderTile)
                {
                    ((WonderTile)newTile).InitWonderBonus(((WonderTile)SelectedTile).WonderBonus);
                }
            }

            TileMap.Add(hexagonTileObject.TileInfo.hexPos, newTile);

            // 이웃 타일들의 범위 내 타일과 이웃 타일을
            // 교체한 타일 컴포넌트로 바꿈.
            foreach (Tile rangeTile in newTile.RangeTiles)
            {
                rangeTile.UpdateNeighborRange();
            }

            return newTile;
        }

        // currentTile의 위치에 Selected 타일을 배치할 수 있는가
        private bool CanPutTile(Tile currentTile)
        {
            if (currentTile == null)
            {
                return false;
            }
            else if (SelectedTile == null)
            {
                return false;
            }
            // 포인트가 모자라다면 false return
            else if (SelectTileCost > GameManager.Instance.BuildPoint)
            {
                return false;
            }
            // 이미 다른 건물이 지어졌다면 false return
            else if (currentTile.MyTileBuilding != TileBuilding.Empty)
            {
                return false;
            }
            else if (SelectedTile.MyTileBuilding == TileBuilding.Empty)
            {
                Debug.LogError("잘못된 타일이 선택됨.");
                return false;
            }


            // 소유 도시가 없고, 선택 타일이 도시 타일인지 검사
            if (currentTile.OwnerCity == null)
            {
                if (!(SelectedTile is CityTile))
                {
                    return false;
                }

                // 항만을 제외하고는 물이나 산 타일에 지을 수 없음.
                if (currentTile.MyTileType == TileType.Water ||
                    currentTile.MyTileType == TileType.Mountain)
                {
                    return false;
                }
            }
            else
            {
                // 불가사의 타일이라면 전용 검사.
                if (SelectedTile is WonderTile)
                {
                    return ((WonderTile)SelectedTile).WonderLimit(currentTile);
                }
                // 불가사의 타일이 아니라면 건물 검사
                else
                {
                    bool hasThatTile = true;

                    // 범위 내 도시에 이미 해당 건물이 설치 되었는지 검사
                    foreach (var rangeCity in currentTile.RangeCitys)
                    {
                        if (!rangeCity.HasThatTile(selectedTile.MyTileBuilding, true))
                        {
                            hasThatTile = false;
                            if (rangeCity != currentTile.OwnerCity)
                            {
                                // 바꾸는데 실패한다면 타일 건물이 설치된 것
                                if (!currentTile.TryChangeOwner(rangeCity))
                                {
                                    hasThatTile = true;
                                }
                            }

                            break;
                        }
                    }
                    
                    if (hasThatTile)
                    {                        
                        return false;
                    }

                    // 항만의 경우 물타일인지 검사
                    if (SelectedTile.MyTileBuilding == TileBuilding.Harbor)
                    {
                        if (currentTile.MyTileType == TileType.Water)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // 항만을 제외하고는 물이나 산 타일에 지을 수 없음.
                        if (currentTile.MyTileType == TileType.Water ||
                            currentTile.MyTileType == TileType.Mountain)
                        {
                            return false;
                        }

                        // 송수로의 경우 주변에 도시와 산이 있는지 검사
                        // 또는 해당 타일이 도시 옆 강 타일인지 검사.
                        if (SelectedTile.MyTileBuilding == TileBuilding.Aqueduct)
                        {
                            bool isRiver = false;
                            bool nearCity = false;
                            bool nearMountain = false;

                            if (currentTile.MyTileType == TileType.River)
                            {
                                isRiver = true;
                            }

                            for (int i = 0; i < currentTile.NeighborTiles.Count; i++)
                            {
                                if (currentTile.NeighborTiles[i].MyTileBuilding == TileBuilding.City)
                                {
                                    nearCity = true;
                                }
                                else if (currentTile.NeighborTiles[i].MyTileType == TileType.Mountain)
                                {
                                    nearMountain = true;
                                }

                                if (nearMountain && nearCity ||
                                    isRiver && nearCity)
                                {
                                    return true;
                                }
                            }

                            return false;
                        }
                    }

                }
            }

            // 다른 타일은 그냥 배치 가능
            return true;
        }

        //#region 시대 별 건물 보너스
        //// 시대 별 빌딩 보너스 업그레이드
        //public void BuildingUpgrade()
        //{
        //    switch (AgeManager.Instance.WorldAge)
        //    {
        //        case Age.Classical:
        //            MyBuildingBonus += ClassicalBonus;
        //            MyAgeCost += ClassicalCost;
        //            break;
        //        case Age.Medieval:
        //            MyBuildingBonus += MedievalBonus;
        //            MyAgeCost += MedievalCost;
        //            break;
        //        case Age.Renaissance:
        //            MyBuildingBonus += RenaissanceBonus;
        //            MyAgeCost += RenaissanceCost;
        //            break;
        //        case Age.Industrial:
        //            MyBuildingBonus += IndustrialBonus;
        //            MyAgeCost += IndustrialCost;
        //            break;
        //        case Age.Modern:
        //            MyBuildingBonus += ModernBonus;
        //            MyAgeCost += ModernCost;
        //            break;
        //        case Age.Atomic:
        //            MyBuildingBonus += AtomicBonus;
        //            MyAgeCost += AtomicCost;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 고전 시대 건물 업그레이드 보너스
        //private void ClassicalBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        case TileBuilding.HolySite:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        case TileBuilding.Encampment:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 중세 시대 건물 업그레이드 보너스
        //private void MedievalBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.HolySite:
        //            currentTile.ChangeBonus(4);
        //            break;
        //        case TileBuilding.TheaterSquare:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        case TileBuilding.Harbor:
        //            currentTile.ChangeBonus(5);
        //            break;
        //        case TileBuilding.CommercialHub:
        //            currentTile.ChangeBonus(3);
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 르네상스 시대 건물 업그레이드 보너스
        //private void RenaissanceBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            currentTile.ChangeBonus(4);
        //            break;
        //        case TileBuilding.IndustrialZone:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        case TileBuilding.Encampment:
        //            currentTile.ChangeBonus(5);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 산업 시대 건물 업그레이드 보너스
        //private void IndustrialBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.TheaterSquare:
        //            currentTile.ChangeBonus(2);
        //            break;
        //        case TileBuilding.Harbor:
        //            currentTile.ChangeBonus(10);
        //            break;
        //        case TileBuilding.CommercialHub:
        //            currentTile.ChangeBonus(5);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 현대 시대 건물 업그레이드 보너스
        //private void ModernBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.IndustrialZone:
        //            currentTile.ChangeBonus(6);
        //            break;
        //        case TileBuilding.Encampment:
        //            currentTile.ChangeBonus(6);
        //            break;
        //        case TileBuilding.CommercialHub:
        //            currentTile.ChangeBonus(10);
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            currentTile.ChangeBonus(4);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 원자력 시대 건물 업그레이드 보너스
        //private void AtomicBonus(Tile currentTile, TileBuilding tileBuilding)
        //{
        //    switch (tileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            currentTile.ChangeBonus(8);
        //            break;
        //        case TileBuilding.IndustrialZone:
        //            currentTile.ChangeBonus(9);
        //            break;
        //        case TileBuilding.TheaterSquare:
        //            currentTile.ChangeBonus(6);
        //            break;
        //        case TileBuilding.Harbor:
        //            currentTile.ChangeBonus(15);
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            currentTile.ChangeBonus(6);
        //            break;
        //        default:
        //            break;
        //    }
        //}
        //#endregion

        //#region 시대 별 건물/불가사의 비용 증가
        //// 고전 시대 건물 비용 증가
        //public void ClassicalCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            SelectTileCost += 1;
        //            break;
        //        case TileBuilding.HolySite:
        //            SelectTileCost += 1;
        //            break;
        //        case TileBuilding.Encampment:
        //            SelectTileCost += 1;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 중세 시대 건물 비용 증가
        //public void MedievalCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.HolySite:
        //            SelectTileCost += 2;
        //            break;
        //        case TileBuilding.TheaterSquare:
        //            SelectTileCost += 1;
        //            break;
        //        case TileBuilding.Harbor:
        //            SelectTileCost += 2;
        //            break;
        //        case TileBuilding.CommercialHub:
        //            SelectTileCost += 1;
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            SelectTileCost += 1;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 르네상스 시대 건물 비용 증가
        //public void RenaissanceCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            SelectTileCost += 2;
        //            break;
        //        case TileBuilding.IndustrialZone:
        //            SelectTileCost += 1;
        //            break;
        //        case TileBuilding.Encampment:
        //            SelectTileCost += 2;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 산업 시대 건물 비용 증가
        //public void IndustrialCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.TheaterSquare:
        //            SelectTileCost += 2;
        //            break;
        //        case TileBuilding.Harbor:
        //            SelectTileCost += 3;
        //            break;
        //        case TileBuilding.CommercialHub:
        //            SelectTileCost += 3;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 현대 시대 건물 비용 증가
        //public void ModernCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.IndustrialZone:
        //            SelectTileCost += 2;
        //            break;
        //        case TileBuilding.CommercialHub:
        //            SelectTileCost += 4;
        //            break;
        //        case TileBuilding.Encampment:
        //            SelectTileCost += 3;
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            SelectTileCost += 2;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 원자력 시대 건물 비용 증가
        //public void AtomicCost()
        //{
        //    switch (SelectedTile.MyTileBuilding)
        //    {
        //        case TileBuilding.Campus:
        //            SelectTileCost += 4;
        //            break;
        //        case TileBuilding.IndustrialZone:
        //            SelectTileCost += 4;
        //            break;
        //        case TileBuilding.TheaterSquare:
        //            SelectTileCost += 3;
        //            break;
        //        case TileBuilding.Harbor:
        //            SelectTileCost += 7;
        //            break;
        //        case TileBuilding.EntertainmentComplex:
        //            SelectTileCost += 3;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //// 시대 별 불가사의 비용 증가
        //public void AgeWonderCost()
        //{
        //    if (SelectedTile.MyTileBuilding != TileBuilding.Wonder)
        //    {
        //        return;
        //    }

        //    // 시대 차이
        //    int AgeDiff = AgeManager.Instance.WorldAge - ((WonderTile)SelectedTile).WonderAge;

        //    // AgeDiff + 1 배 만큼 불가사의 비용이 증가함.
        //    for (int i = 0; i < AgeDiff; i++)
        //    {
        //        SelectTileCost += SelectedTile.Cost;
        //    }
        //}
        //#endregion
    }
}