using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TilePuzzle.Procedural;
using TilePuzzle.Entities;

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

        [SerializeField]
        private List<Tile> BuildingPrefabs = new List<Tile>();

        public Dictionary<TileBuilding, Tile> BuildingPrefabMap { get; } = new Dictionary<TileBuilding, Tile>();

        // 도시 타일 갯수
        [ShowInInspector]
        public int CityNum { get { return GameManager.Instance.TurnEntity.ownCitys.Count; } }

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
            InitPrefabMap();
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
            IEnumerable<HexagonTileObject> neighborHexagons =
                GameManager.Instance.MyHexagonTerrain.GetHexagonTiles(myTile.MyHexagonInfo.hexPos, new RangeInt(1, range - 1));
            List<Tile> neighborTiles = new List<Tile>();

            foreach (HexagonTileObject neighbor in neighborHexagons)
            {
                neighborTiles.Add(TileMap[neighbor.TileInfo.hexPos]);
            }

            return neighborTiles;
        }

        // 무작위 빈 타일을 return 함.
        public Tile GetRandomEmptyTile()
        {
            bool getRandomSuccess = false;
            Vector2Int terrainSize = GameManager.Instance.terrainGenerateSettings.terrainSize;
            Tile randomTile = null;

            while (!getRandomSuccess)
            {
                int x = Random.Range(0, terrainSize.x);
                int y = Random.Range(0, terrainSize.y);

                randomTile = TileMap[HexagonPos.FromArrayXY(x, y)];

                if (randomTile.MyTileBuilding == TileBuilding.Empty)
                {
                    getRandomSuccess = true;
                }
            }

            return randomTile;
        }

        // tiles 내의 무작위 빈 타일을 return 함.
        public Tile GetRandomEmptyTile(List<Tile> tiles)
        {
            bool getRandomSuccess = false;
            Tile randomTile = null;
            HashSet<Tile> ignoreTiles = new HashSet<Tile>();

            while (!getRandomSuccess)
            {
                if (ignoreTiles.Count >= tiles.Count)
                {
                    Debug.LogError("빈 타일이 없습니다.");
                    break;
                }

                int randomIndex = Random.Range(0, tiles.Count);

                randomTile = tiles[randomIndex];

                if (randomTile.MyTileBuilding == TileBuilding.Empty)
                {
                    getRandomSuccess = true;
                }
                else
                {
                    ignoreTiles.Add(randomTile);
                }
            }

            return randomTile;
        }

        // targetTile에 tileBuilding을 설치
        public bool PutBuildingAtTile(TileBuilding tileBuilding, Tile targetTile)
        {
            var buildingPrefab = BuildingPrefabMap[tileBuilding].gameObject;

            return PutTileAtTarget(buildingPrefab, targetTile);
        }

        // targetTile에 해당 이름을 가진 불가사의를 설치
        public bool PutWonderAtTile(string wonderName, Tile targetTile)
        {
            var wonderData = DataTableManager.Instance.GetWonderData(wonderName);

            return PutTileAtTarget(wonderData.MyPrefab, targetTile);
        }

        private bool PutTileAtTarget(GameObject tilePrefab, Tile targetTile)
        {
            ClearSelectedTile();
            bool instantiateSuccess = InstantiateTile(tilePrefab);

            if (!instantiateSuccess)
            {
                return false;
            }

            bool canPutTile = CanPutTile(targetTile);

            if (canPutTile)
            {
                // 타일 설치
                UpdateTile(targetTile);
            }

            return canPutTile;
        }

        // 타일 생성 후, SelectedTile에 할당
        public bool InstantiateTile(GameObject tilePrefab)
        {
            SelectedTile = Instantiate(tilePrefab, Vector3.up * 20f, Quaternion.identity).GetComponent<Tile>();
            SelectedTile.GetComponent<MeshCollider>().enabled = false;

            if (SelectedTile is CityTile)
            {
                SelectedTile.Cost = CityNum;
            }
            else if (SelectedTile is WonderTile)
            {
                // 불가사의 이름(Clone)
                // 이 형식으로 나오기 때문에
                // 문자열 분리해서 입력
                SelectedTile.Cost = DataTableManager.Instance.GetWonderData(SelectedTile.name.Split('(')[0]).Cost;
            }
            else if (SelectedTile.MyTileBuilding == TileBuilding.Aqueduct)
            {
                SelectedTile.Cost = 1;
            }
            else if (SelectedTile.MyTileBuilding == TileBuilding.GovernmentPlaza)
            {
                SelectedTile.Cost = 3;
            }
            else
            {
                var buildingData = DataTableManager.Instance.GetBuildingData
                                (AgeManager.Instance.WorldAge, SelectedTile.MyTileBuilding);

                if (buildingData == InfoPerAge.EmptyInfo)
                {
                    return false;
                }

                SelectTileCost = buildingData.Cost;
            }

            SelectedTile.TurnGrid(false);

            return true;
        }

        public void ClearSelectedTile()
        {
            if (SelectedTile != null)
            {
                Destroy(SelectedTile.gameObject);
                SelectedTile = null;
            }
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

                if (!(GameManager.Instance.TurnEntity is Player))
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

                    if (!(GameManager.Instance.TurnEntity is Player))
                    {
                        yield return new WaitForSeconds(0.02f);
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

                    // 타일 교체, 보너스 갱신, 모델 갱신 진행
                    UpdateTile(clickedTile);
                }

                yield return null;
            }
        }

        // target 타일을 Selected 타일로 교체, 보너스 갱신, 모델 갱신 진행
        private void UpdateTile(Tile targetTile)
        {
            tilePlacementDrawer.PlacementObject = null;

            // 클릭한 타일의 컴포넌트를
            // 설치하려는 타일로 교체
            Tile changedTile = ChangeTile(targetTile);

            // 건물 보너스 갱신
            UpdateBonus(changedTile);

            if (changedTile is WonderTile wonderTile)
            {
                // 딜리케이트 추가
                wonderTile.AddToDelegate();
                // 설치한 불가사의 버튼 비활성화
                UIManager.Instance.DisableWonderButton();
                // 설치에 성공한 불가사의 이름 제거
                DataTableManager.Instance.WonderNames.Remove(changedTile.GetType().ToString().Split('.')[1]);
            }

            GameManager.Instance.UpdatePoint(changedTile.Bonus);

            // 건물 모델을 타일 위에 올림
            PutModelOnTile(changedTile);
            ClearSelectedTile();

            // 다음 엔티티에게 턴을 넘겨줌
            GameManager.Instance.NextTurn();
        }

        // changedTile의 보너스 갱신
        private void UpdateBonus(Tile changedTile)
        {
            // 건물 보너스 갱신
            if (changedTile is BuildingTile tile)
            {
                tile.RefreshBonus();
            }
            // 불가사의로 인한 보너스 추가, 보너스 출력
            CalculateBonusByWonder?.Invoke(changedTile, changedTile.MyTileBuilding);
        }

        // 건물 모델을 타일 위에 올림
        private void PutModelOnTile(Tile changedTile)
        {
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

        // buildingPrefabMap 초기화
        private void InitPrefabMap()
        {
            foreach (var building in BuildingPrefabs)
            {
                BuildingPrefabMap.Add(building.MyTileBuilding, building);
            }
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
            if (newTile is CityTile cityTile)
            {
                cityTile.SetRangeGrids();
                cityTile.SetOwnerInRange();

                var turnEntity = GameManager.Instance.TurnEntity;

                turnEntity.ownCitys.Add(cityTile);
                cityTile.InitEntity();
            }
            else
            {
                newTile.SetRangeCitys(rangeCitys);
                newTile.SetCityTile(city);

                if (SelectedTile is WonderTile wonderTile)
                {
                    ((WonderTile)newTile).InitWonderBonus(wonderTile.WonderBonus);
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

                            foreach (var neighborTile in currentTile.NeighborTiles)
                            {
                                if (neighborTile.MyTileBuilding == TileBuilding.City)
                                {
                                    nearCity = true;
                                }
                                else if (neighborTile.MyTileType == TileType.Mountain)
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
    }
}