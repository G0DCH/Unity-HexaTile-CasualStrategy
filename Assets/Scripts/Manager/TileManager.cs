using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public TileBuilding StringToType(string tileType)
        {

            if (System.Enum.TryParse(tileType, out TileBuilding enumType))
            {
                return enumType;
            }
            else
            {
                Debug.LogError("타일 타입이 아님");
                return TileBuilding.Empty;
            }
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

                        SelectTileCost = SelectedTile.Cost;
                        MyWonderCost?.Invoke(overTile, SelectedTile.MyTileBuilding);

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

                            if (SelectedTile is BuildingTile)
                            {
                                // 기존 타일 컴포넌트 제거하고
                                // 빌딩 혹은 도시 타일 컴포넌트로 교체
                                GameObject clickedObject = clickedTile.gameObject;
                                Procedural.Hexagon hexagon = clickedTile.MyHexagon;
                                Destroy(clickedTile);

                                // 격자 표기, 타일 소유권 이전
                                if (SelectedTile is CityTile)
                                {
                                    clickedTile = clickedObject.AddComponent<CityTile>();
                                    clickedTile.MyHexagon = hexagon;

                                    ((CityTile)clickedTile).SetRangeGrids();
                                    ((CityTile)clickedTile).SetOwnerInRange();
                                }
                                else
                                {
                                    clickedTile = clickedObject.AddComponent<BuildingTile>();
                                    clickedTile.MyHexagon = hexagon;
                                }
                            }

                            // 건물 보너스 갱신
                            if (clickedTile is BuildingTile)
                            {
                                // 타일 타입을 건설한 건물로 변경
                                clickedTile.MyTileBuilding = tileBuilding;
                                ((BuildingTile)clickedTile).RefreshBonus();
                            }
                            // 불가사의로 인한 보너스 추가, 보너스 출력
                            MyWonderBonus?.Invoke(clickedTile, tileBuilding);

                            GameManager.Instance.RefreshPoint(clickedTile.Bonus);

                            // 타일 위에 얹혀져 있는 데코레이션 삭제
                            if(clickedTile.transform.childCount > 1)
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
            else if (currentTile.MyTileType != TileType.Ground)
            {
                // 땅과 강 타일이 아니라면 false return
                if (currentTile.MyTileType != TileType.River)
                {
                    return false;
                }
            }

            // 소유 도시가 없고, 선택 타일이 도시 타일인지 검사
            if (currentTile.OwnerCity == null)
            {
                if (!(SelectedTile is CityTile))
                {
                    return false;
                }
            }
            else
            {
                // 소유 도시에 이미 해당 건물이 설치 되었는지 검사
                if (currentTile.OwnerCity.HasThatTile(SelectedTile.MyTileBuilding))
                {
                    return false;
                }

                // 송수로의 경우 주변에 도시와 산이 있는지 검사
                if (SelectedTile.MyTileBuilding == TileBuilding.Aqueduct)
                {
                    bool nearCity = false;
                    bool nearMountain = false;

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

                        if (nearMountain && nearCity)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            // 다른 타일은 그냥 배치 가능
            return true;
        }
    }
}