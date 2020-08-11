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

        [Space]
        [Header("Selected Tile")]
        [ReadOnly]
        public Tile SelectedTile;

        [Space, Header("SelectTileCost")]
        public int SelectTileCost = 0;

        // 이전에 마우스가 위에 올라가있던 타일
        private Tile prevOverTile = null;

        private int pointerID = -1;

        // 점수 보너스 delegate
        public delegate void WonderBonus(Tile currentTile, TileBuilding tileBuilding);
        public WonderBonus MyWonderBonus = null;

        // 소모 점수 보너스 delegate
        public delegate void WonderCost(Tile currentTile, TileBuilding tileBuilding);
        public WonderCost MyWonderCost = null;

        // 범위 보너스 delegate
        public delegate void WonderRange(Tile currentTile, TileBuilding tileBuilding);
        public WonderRange MyWonderRange = null;

        private Dictionary<HexagonPos, Tile> TileMap = new Dictionary<HexagonPos, Tile>();

        private void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            pointerID = -1;
#endif

#if UNITY_IOS || UNITY_ANDROID
            pointerID = 0;
#endif
            StartCoroutine(MouseOverCheck());
            StartCoroutine(TileClickCheck());
        }

        public void InitTileMap()
        {
            HexagonObject[] hexagonObjects = FindObjectsOfType<HexagonObject>();

            foreach (HexagonObject hexagonObject in hexagonObjects)
            {
                Tile tile = hexagonObject.gameObject.AddComponent<Tile>();

                HexagonInfo hexagonInfo = GameManager.Instance.World.GetHexagonInfoAt(hexagonObject.hexPos);
                DecorationInfo decorationInfo = GameManager.Instance.World.GetDecorationInfoAt(hexagonObject.hexPos).GetValueOrDefault();

                tile.InitInfo(hexagonInfo, decorationInfo);
                TileMap.Add(hexagonObject.hexPos, tile);
            }
        }

        // 범위 내 타일 return
        public List<Tile> GetRangeTiles(Tile myTile, int range)
        {
            IEnumerable<HexagonInfo> neighborHexagons = GameManager.Instance.World.GetHexagonInfosInRange(myTile.MyHexagonInfo.hexPos, 1, range);
            List<Tile> neighborTiles = new List<Tile>();

            foreach (HexagonInfo neighbor in neighborHexagons)
            {
                neighborTiles.Add(TileMap[neighbor.hexPos]);
            }

            return neighborTiles;
        }

        // 마우스가 타일 맵 위에 올라갔는지 체크
        private IEnumerator MouseOverCheck()
        {
            RaycastHit hit = new RaycastHit();
            while (true)
            {
                if (SelectedTile == null)
                {
                    yield return new WaitForSeconds(0.02f);
                    continue;
                }

                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerID))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray.origin, ray.direction, out hit))
                    {
                        Tile overTile = hit.transform.GetComponent<Tile>();

                        if (!CanPutTile(overTile))
                        {
                            yield return new WaitForSeconds(0.02f);
                            continue;
                        }

                        if (prevOverTile != null)
                        {
                            if (prevOverTile != overTile)
                            {
                                prevOverTile.TurnRangeGrid(false);
                            }
                        }

                        SelectedTile.transform.position = overTile.transform.position + Vector3.up * 0.1f;
                        overTile.TurnRangeGrid(true);

                        // 코스트 계산
                        SelectTileCost = SelectedTile.Cost;
                        MyWonderCost?.Invoke(overTile, SelectedTile.MyTileBuilding);

                        // 범위 계산, 범위 내 타일 갱신
                        MyWonderRange?.Invoke(overTile, SelectedTile.MyTileBuilding);
                        overTile.UpdateRangeTiles();

                        prevOverTile = overTile;
                    }
                }
                yield return new WaitForSeconds(0.02f);
            }
        }

        // 타일을 클릭했는지 체크, 해당 위치에 타일을 놓음
        private IEnumerator TileClickCheck()
        {
            RaycastHit hit = new RaycastHit();
            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (SelectedTile == null)
                    {
                        yield return null;
                        continue;
                    }
                    if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerID))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray.origin, ray.direction, out hit))
                        {
                            Tile clickedTile = hit.transform.GetComponent<Tile>();

                            if (!CanPutTile(clickedTile))
                            {
                                yield return new WaitForSeconds(0.02f);
                                continue;
                            }

                            TileBuilding tileBuilding = SelectedTile.MyTileBuilding;

                            // 기존 타일 컴포넌트 제거
                            GameObject clickedObject = clickedTile.gameObject;
                            HexagonInfo hexagon = clickedTile.MyHexagonInfo;
                            DecorationInfo decorationInfo = clickedTile.MyDecorationInfo;
                            int range = clickedTile.Range;
                            TileMap.Remove(hexagon.hexPos);
                            Destroy(clickedTile);

                            // 빌딩 또는 도시 컴포넌트로 교체
                            if (SelectedTile is BuildingTile)
                            {
                                // 격자 표기, 타일 소유권 이전
                                if (SelectedTile is CityTile)
                                {
                                    clickedTile = clickedObject.AddComponent<CityTile>();
                                    clickedTile.InitInfo(hexagon, decorationInfo, range);
                                    ((CityTile)clickedTile).SetRangeGrids();
                                    ((CityTile)clickedTile).SetOwnerInRange();
                                }
                                else
                                {
                                    clickedTile = clickedObject.AddComponent<BuildingTile>();
                                    clickedTile.InitInfo(hexagon, decorationInfo, range);
                                }
                            }
                            // 원더 컴포넌트로 교체
                            else if (SelectedTile is WonderTile)
                            {
                                clickedTile = (Tile)clickedObject.AddComponent(SelectedTile.GetType());
                                clickedTile.InitInfo(hexagon, decorationInfo, range);
                            }

                            TileMap.Add(hexagon.hexPos, clickedTile);

                            // 이웃 타일들의 범위 내 타일과 이웃 타일을
                            // 교체한 타일 컴포넌트로 바꿈.
                            foreach (Tile rangeTile in clickedTile.RangeTiles)
                            {
                                rangeTile.UpdateNeighborRange();
                            }

                            // 타일 타입을 건설한 건물로 변경
                            clickedTile.MyTileBuilding = tileBuilding;

                            // 건물 보너스 갱신
                            if (clickedTile is BuildingTile)
                            {
                                ((BuildingTile)clickedTile).RefreshBonus();
                            }
                            // 불가사의로 인한 보너스 추가, 보너스 출력
                            MyWonderBonus?.Invoke(clickedTile, tileBuilding);

                            if (clickedTile is WonderTile)
                            {
                                // 딜리케이트 추가
                                ((WonderTile)clickedTile).AddToDelegate();
                            }

                            GameManager.Instance.RefreshPoint(clickedTile.Bonus);

                            // 타일 위에 얹혀져 있는 데코레이션 삭제
                            if (clickedTile.transform.childCount > 1)
                            {
                                Destroy(clickedTile.transform.GetChild(0).gameObject);
                            }

                            Transform building = SelectedTile.transform.GetChild(0);
                            building.SetParent(clickedTile.transform, true);
                            building.localPosition = Vector3.zero;

                            Destroy(SelectedTile.gameObject);
                            SelectedTile = null;
                        }
                    }
                }

                yield return null;
            }
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
                    // 소유 도시에 이미 해당 건물이 설치 되었는지 검사
                    if (currentTile.OwnerCity.HasThatTile(SelectedTile.MyTileBuilding))
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
    }
}