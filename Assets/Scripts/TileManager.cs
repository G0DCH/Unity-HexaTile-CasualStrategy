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
        public List<RowList> TileMap;
        // 타일 프리팹들
        public List<GameObject> TilePrefabs;

        [Space]
        [Header("Selected Tile")]
        public GameObject SelectedTile;

        [Space]
        [Header("Tile Numbers")]
        public int rowNum;
        public int columnNum;

        private void Start()
        {
            InitMap();
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

        // 타일 선택
        public void ButtonSelect(GameObject tilePrefab)
        {
            SelectedTile = tilePrefab;
        }
    }
}