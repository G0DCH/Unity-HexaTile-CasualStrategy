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

        public int rowNum;
        public int columnNum;

        private void Start()
        {
            InitMap();
        }

        // 테스트 용
        private void InitMap()
        {
            int prefabLength = TilePrefabs.Count;
            // z축 증가량
            float zOffset = Mathf.Sin(Mathf.PI / 3);

            // 타일 생성
            for (int i = 0; i < rowNum; i++)
            {
                TileMap.Add(new RowList(new List<Tile>()));

                float zPosition = i * zOffset;

                // x축 시작 지점
                // i가 홀수면 0.5, 아니면 0
                float xStartPosition = 0f;

                if (i % 2 == 1)
                {
                    xStartPosition = 0.5f;
                }

                for (int j = 0; j < columnNum; j++)
                {
                    int index = Random.Range(0, prefabLength);

                    GameObject tileInstance = Instantiate(TilePrefabs[index], transform);

                    tileInstance.transform.position = new Vector3(xStartPosition + j, 0, zPosition);

                    Tile tile = tileInstance.GetComponent<Tile>();

                    if (tileInstance.name.Contains("Mountain"))
                    {
                        tile.InitTile(TileType.Mountain, i, j);
                    }
                    else
                    {
                        tile.InitTile(TileType.Ground, i, j);
                    }

                    TileMap[i].rowList.Add(tile);
                }
            }

            // 이웃 타일 입력
            for (int i = 0; i< rowNum; i++)
            {
                bool isEven = false;

                if (i % 2 == 0)
                {
                    isEven = true;
                }

                for (int j = 0; j < columnNum; j++)
                {
                    bool isLeft = false;
                    bool isRight = false;
                    bool isBottom = false;
                    bool isTop = false;

                    if (i == 0)
                    {
                        isBottom = true;
                    }
                    else if(i == rowNum - 1)
                    {
                        isTop = true;
                    }

                    if (j == 0)
                    {
                        isLeft = true;
                    }
                    else if(j == columnNum - 1)
                    {
                        isRight = true;
                    }

                    Tile pivotTile = TileMap[i].rowList[j];
                    List<Tile> neighborList = new List<Tile>();
                    
                    // 좌 우 추가
                    if (isLeft)
                    {
                        neighborList.Add(TileMap[i].rowList[j + 1]);
                    }
                    else if (isRight)
                    {
                        neighborList.Add(TileMap[i].rowList[j - 1]);
                    }
                    else
                    {
                        neighborList.Add(TileMap[i].rowList[j - 1]);
                        neighborList.Add(TileMap[i].rowList[j + 1]);
                    }

                    // 위 아래 추가
                    if (isBottom)
                    {
                        // 맨 왼쪽에 있는 경우를 제외하고 그냥 추가 하면 됨
                        if (isLeft)
                        {
                            neighborList.Add(TileMap[i + 1].rowList[j]);
                        }
                        else
                        {
                            neighborList.Add(TileMap[i + 1].rowList[j - 1]);
                            neighborList.Add(TileMap[i + 1].rowList[j]);
                        }
                    }
                    else if (isTop)
                    {
                        if ( isEven )
                        {
                            if (isLeft)
                            {
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                            else
                            {
                                neighborList.Add(TileMap[i - 1].rowList[j - 1]);
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                        }
                        else
                        {
                            if (isRight)
                            {
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                            else
                            {
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                                neighborList.Add(TileMap[i - 1].rowList[j + 1]);
                            }
                        }
                    }
                    else
                    {
                        if (isEven)
                        {
                            if (isLeft)
                            {
                                neighborList.Add(TileMap[i + 1].rowList[j]);
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                            else
                            {
                                neighborList.Add(TileMap[i + 1].rowList[j - 1]);
                                neighborList.Add(TileMap[i + 1].rowList[j]);

                                neighborList.Add(TileMap[i - 1].rowList[j - 1]);
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                        }
                        else
                        {
                            if (isRight)
                            {
                                neighborList.Add(TileMap[i + 1].rowList[j]);
                                neighborList.Add(TileMap[i - 1].rowList[j]);
                            }
                            else
                            {
                                neighborList.Add(TileMap[i + 1].rowList[j]);
                                neighborList.Add(TileMap[i + 1].rowList[j + 1]);

                                neighborList.Add(TileMap[i - 1].rowList[j]);
                                neighborList.Add(TileMap[i - 1].rowList[j + 1]);
                            }
                        }
                    }

                    pivotTile.InitNeighborTiles(neighborList);
                }
            }
        }
    }
}