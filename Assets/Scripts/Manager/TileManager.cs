using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    public class TileManager : Utility.Singleton<TileManager>
    {
        [System.Serializable]
        public struct RowList
        {
            [SerializeField]
            public List<Tile> rowList;

            public RowList(List<Tile> myRowList)
            {
                rowList = myRowList;
            }
        }

        // 게임에 존재하는 타일
        [ReadOnly]
        public List<RowList> TileMap;
        // 타일 프리팹들
        public List<GameObject> TilePrefabs;
        // 범위 표기 프리팹
        public GameObject GridPrefab;

        [Space]
        [Header("Selected Tile")]
        [ReadOnly]
        public Tile SelectedTile;

        [Space]
        [Header("Tile Numbers")]
        public int rowNum;
        public int columnNum;

        [Space]
        [Header("Tile Material")]
        public Material SelectedMaterial;
        public Material NormalMaterial;

        // 이전에 마우스가 위에 올라가있던 타일
        private Tile prevOverTile = null;

        private int pointerID = -1;

        private void Start()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            pointerID = -1;
#endif

#if UNITY_IOS || UNITY_ANDROID
            pointerID = 0;
#endif

            InitMap();

            StartCoroutine(MouseOverCheck());
            StartCoroutine(TileClickCheck());
        }

        // 테스트 용
        private void InitMap()
        {
            // 타일 생성
            for (int i = 0; i < rowNum; i++)
            {
                TileMap.Add(new RowList(new List<Tile>()));
                for (int j = 0; j < columnNum; j++)
                {
                    MakeTile(i, j);
                }
            }

            // 이웃 타일 입력
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < columnNum; j++)
                {
                    AssignNeighbors(i, j);
                }
            }

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < columnNum; j++)
                {
                    TileMap[i].rowList[j].InitRangeTiles();
                }
            }
        }

        public void MakeTile(int row, int column)
        {
            int prefabLength = TilePrefabs.Count;
            // z축 증가량
            float zOffset = Mathf.Sin(Mathf.PI / 3);

            float zPosition = row * zOffset;

            // x축 시작 지점
            // row가 홀수면 0.5, 아니면 0
            float xStartPosition = 0f;

            if (row % 2 == 1)
            {
                xStartPosition = 0.5f;
            }

            int index = Random.Range(0, prefabLength);

            GameObject tileInstance = Instantiate(TilePrefabs[index], transform);

            tileInstance.transform.position = new Vector3(xStartPosition + column, 0, zPosition);

            Tile tile = tileInstance.GetComponent<Tile>();

            if (tileInstance.name.Contains("Mountain"))
            {
                tile.InitTile(TileType.Mountain, row, column);
            }
            else
            {
                tile.InitTile(TileType.Ground, row, column);
            }

            TileMap[row].rowList.Add(tile);
            tile.MakeGrid(GridPrefab);
            tile.TurnGrid(false);
        }

        public void AssignNeighbors(int row, int column)
        {
            bool isEven = false;

            if (row % 2 == 0)
            {
                isEven = true;
            }

            bool isLeft = false;
            bool isRight = false;
            bool isBottom = false;
            bool isTop = false;

            if (row == 0)
            {
                isBottom = true;
            }
            else if (row == rowNum - 1)
            {
                isTop = true;
            }

            if (column == 0)
            {
                isLeft = true;
            }
            else if (column == columnNum - 1)
            {
                isRight = true;
            }

            Tile pivotTile = TileMap[row].rowList[column];
            List<Tile> neighborList = new List<Tile>();

            // 좌 우 추가
            if (isLeft)
            {
                neighborList.Add(TileMap[row].rowList[column + 1]);
            }
            else if (isRight)
            {
                neighborList.Add(TileMap[row].rowList[column - 1]);
            }
            else
            {
                neighborList.Add(TileMap[row].rowList[column - 1]);
                neighborList.Add(TileMap[row].rowList[column + 1]);
            }

            // 위 아래 추가
            if (isBottom)
            {
                // 맨 왼쪽에 있는 경우를 제외하고 그냥 추가 하면 됨
                if (isLeft)
                {
                    neighborList.Add(TileMap[row + 1].rowList[column]);
                }
                else
                {
                    neighborList.Add(TileMap[row + 1].rowList[column - 1]);
                    neighborList.Add(TileMap[row + 1].rowList[column]);
                }
            }
            else if (isTop)
            {
                if (isEven)
                {
                    if (isLeft)
                    {
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                    else
                    {
                        neighborList.Add(TileMap[row - 1].rowList[column - 1]);
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                }
                else
                {
                    if (isRight)
                    {
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                    else
                    {
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                        neighborList.Add(TileMap[row - 1].rowList[column + 1]);
                    }
                }
            }
            else
            {
                if (isEven)
                {
                    if (isLeft)
                    {
                        neighborList.Add(TileMap[row + 1].rowList[column]);
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                    else
                    {
                        neighborList.Add(TileMap[row + 1].rowList[column - 1]);
                        neighborList.Add(TileMap[row + 1].rowList[column]);

                        neighborList.Add(TileMap[row - 1].rowList[column - 1]);
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                }
                else
                {
                    if (isRight)
                    {
                        neighborList.Add(TileMap[row + 1].rowList[column]);
                        neighborList.Add(TileMap[row - 1].rowList[column]);
                    }
                    else
                    {
                        neighborList.Add(TileMap[row + 1].rowList[column]);
                        neighborList.Add(TileMap[row + 1].rowList[column + 1]);

                        neighborList.Add(TileMap[row - 1].rowList[column]);
                        neighborList.Add(TileMap[row - 1].rowList[column + 1]);
                    }
                }

            }

            pivotTile.InitNeighborTiles(neighborList);
        }

        public TileType StringToType(string tileType)
        {

            if (System.Enum.TryParse(tileType, out TileType enumType))
            {
                return enumType;
            }
            else
            {
                Debug.LogError("타일 타입이 아님");
                return TileType.Empty;
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
                            if (!CanPutTile(hit.transform.GetComponent<Tile>()))
                            {
                                yield return new WaitForSeconds(0.02f);
                                continue;
                            }

                            // 타일 설치
                            Position clickPosition = hit.transform.GetComponent<Tile>().MyPosition;

                            Tile clickedTile = TileMap[clickPosition.Row].rowList[clickPosition.Column];

                            string[] tileName = SelectedTile.name.Split('(');
                            TileType tileType = StringToType(tileName[0]);

                            // 타일 초기화
                            SelectedTile.InitTile(tileType, clickPosition.Row, clickPosition.Column);

                            // 이웃 타일 갱신
                            SelectedTile.InitNeighborTiles(clickedTile.NeighborTiles);
                            SelectedTile.InitRangeTiles(clickedTile.RangeTiles);
                            clickedTile.UpdateNeighborTile(clickedTile, SelectedTile);

                            // 모든 타일의 범위 내 타일 갱신
                            UpdateRangeTiles(clickedTile, SelectedTile);

                            // 타일 맵 갱신
                            TileMap[clickPosition.Row].rowList.Remove(clickedTile);
                            TileMap[clickPosition.Row].rowList.Insert(clickPosition.Column, SelectedTile);

                            // 위치 갱신, 매터리얼 변경, 컬라이더 활성화
                            SelectedTile.transform.position = clickedTile.transform.position;
                            SelectedTile.ChangeMaterial(false);
                            SelectedTile.GetComponent<MeshCollider>().enabled = true;
                            SelectedTile.transform.parent = transform;
                            prevOverTile = SelectedTile;

                            //SelectedTile.RefreshBonus();

                            // 보너스 갱신
                            if (SelectedTile is BuildingTile)
                            {
                                // 격자 표기, 타일 소유권 이전
                                if (SelectedTile is CityTile)
                                {
                                    ((CityTile)SelectedTile).SetRangeGrids();
                                    ((CityTile)SelectedTile).SetOwnerInRange();
                                }
                                else
                                {
                                    // 소유한 도시 설정
                                    SelectedTile.SetCityTile(clickedTile.OwnerCity);
                                }

                                ((BuildingTile)SelectedTile).RefreshBonus();
                                GameManager.Instance.RefreshPoint(SelectedTile.Bonus);
                            }

                            Destroy(clickedTile.gameObject);
                            SelectedTile = null;

                            // 모든 타일 보너스 갱신 후 총 점수 표기
                            //int totalBonus = RefreshAllTileBonus();

                            //GameManager.Instance.RefreshPoint(totalBonus);
                        }
                    }
                }

                yield return null;
            }
        }

        private void UpdateRangeTiles(Tile clickedTile, Tile SelectedTile)
        {
            // 모든 타일의 범위 내 타일 갱신
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < columnNum; j++)
                {
                    TileMap[i].rowList[j].UpdateRangeTile(clickedTile, SelectedTile);
                }
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
                if (currentTile.OwnerCity.HasThatTile(SelectedTile.MyTileType))
                {
                    return false;
                }

                // 송수로의 경우 주변에 도시와 산이 있는지 검사
                if (SelectedTile.MyTileType == TileType.WaterPipe)
                {
                    bool nearCity = false;
                    bool nearMountain = false;

                    for (int i = 0; i < currentTile.NeighborTiles.Count; i++)
                    {
                        if (currentTile.NeighborTiles[i].MyTileType == TileType.City)
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

        /*
        // 모든 타일의 보너스를 갱신 후 총 점수 return
        private int RefreshAllTileBonus()
        {
            int totalBonus = 0;

            for (int i = 0; i < rowNum; i++)
            {
                // 타일 보너스 갱신 후 총 점수 갱신
                for (int j = 0; j < columnNum; j++)
                {
                    Tile tile = TileMap[i].rowList[j];
                    tile.RefreshBonus();
                    totalBonus += tile.Bonus;
                }
            }

            return totalBonus;
        }
        */
    }
}