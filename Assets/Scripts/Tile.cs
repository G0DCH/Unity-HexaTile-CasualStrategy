using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public enum TileType { Water, Mountain, Ground, Campus, Factory, GovernmentBuilding, HolyLand, Theator, WaterPipe, City, Empty }

    [System.Serializable]
    public struct Position
    {
        public int Row;
        public int Column;

        public Position(int myRow, int myColumn)
        {
            Row = myRow;
            Column = myColumn;
        }
    }

    public class Tile : MonoBehaviour
    {
        // 이 타일의 타입
        public TileType MyTileType { get { return myTileType; } private set { myTileType = value; } }
        [SerializeField]
        private TileType myTileType = TileType.Empty;

        // 이웃한 타일
        public List<Tile> NeighborTiles { get { return neighborTiles; } private set { neighborTiles = value; } }
        [SerializeField]
        private List<Tile> neighborTiles;

        // 이 타일이 받는 보너스
        public int Bonus { get { return bonus; } private set { bonus = value; } }
        [SerializeField]
        private int bonus = 0;

        // 이 타일의 위치
        public Position MyPosition { get { return myPosition; } private set { myPosition = value; } }
        [SerializeField]
        private Position myPosition = new Position(0, 0);

        public void ChangeTileType(TileType tileType)
        {
            MyTileType = tileType;
        }

        public void InitTile(TileType tileType, int row, int column)
        {
            ChangeTileType(tileType);
            InitPosition(row, column);
        }

        public void InitNeighborTiles(List<Tile> neighbors)
        {
            neighborTiles = neighbors;
        }

        private void InitPosition(int row, int column)
        {
            MyPosition = new Position(row, column);
        }

        public void ChangeMaterial(bool isSelected)
        {
            MeshRenderer myRenderer = GetComponent<MeshRenderer>();
            MeshRenderer childRenderer = transform.GetChild(0).GetComponent<MeshRenderer>();

            if (isSelected)
            {
                myRenderer.material = TileManager.Instance.SelectedMaterial;
                childRenderer.material = TileManager.Instance.SelectedMaterial;
            }
            else
            {
                myRenderer.material = TileManager.Instance.NormalMaterial;
                childRenderer.material = TileManager.Instance.NormalMaterial;
            }
        }

        // 이웃 타일들에게서 prev를 제거하고 current를 넣음
        public void ChangeNeighborTile(Tile prev, Tile current)
        {
            for (int i = 0; i<NeighborTiles.Count; i++)
            {
                NeighborTiles[i].changeNeighborTile(prev, current);
            }
        }

        // 내 이웃 타일에서 prev를 제거하고 current를 넣음
        private void changeNeighborTile(Tile prev, Tile current)
        {
            NeighborTiles.Remove(prev);
            NeighborTiles.Add(current);
        }

        // 내 타일의 보너스 갱신
        public void RefreshBonus()
        {
            // 포인트가 없는 타일은 패스
            if (MyTileType == TileType.City)
            {
                return;
            }
            else if (MyTileType == TileType.GovernmentBuilding)
            {
                return;
            }
            else if (MyTileType == TileType.Mountain)
            {
                return;
            }
            else if (MyTileType == TileType.Ground)
            {
                return;
            }
            else if (MyTileType == TileType.Water)
            {
                return;
            }
            else if (MyTileType == TileType.WaterPipe)
            {
                return;
            }
            else if (MyTileType == TileType.Empty)
            {
                Debug.LogError(string.Format("Error Tile Exist : {0}, {1}", name, transform.GetSiblingIndex()));
                return;
            }

            // 특수지구 개수. 2개당 +1
            int buildingCount = 0;
            // 특수지구 별 보너스 점수
            int specificBonus = 0;
            
            // 특수지구 개수를 세고, 내 타일 타입의 보너스 추가
            for (int i = 0; i< NeighborTiles.Count; i++)
            {
                if (NeighborTiles[i].IsBuilding())
                {
                    buildingCount += 1;
                }

                specificBonus += NeighborTiles[i].CountSpecificBonus(MyTileType);
            }

            Bonus = buildingCount / 2 + specificBonus;
        }

        // 건물이라면 true를 return
        private bool IsBuilding()
        {
            if (MyTileType == TileType.City)
            {
                return true;
            }
            else if(MyTileType == TileType.Campus)
            {
                return true;
            }
            else if (MyTileType == TileType.Factory)
            {
                return true;
            }
            else if (MyTileType == TileType.GovernmentBuilding)
            {
                return true;
            }
            else if (MyTileType == TileType.HolyLand)
            {
                return true;
            }
            else if (MyTileType == TileType.Theator)
            {
                return true;
            }
            else if (MyTileType == TileType.WaterPipe)
            {
                return true;
            }

            return false;
        }

        // 내 타일이 pivotType의 보너스에 해당하는지 검사하고 해당 점수 return
        private int CountSpecificBonus(TileType pivotType)
        {
            int bonusPoint = 0;

            if (MyTileType == TileType.GovernmentBuilding)
            {
                bonusPoint = 1;
            }
            else if (pivotType == TileType.Campus)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotType == TileType.Factory)
            {
                if (MyTileType == TileType.WaterPipe)
                {
                    bonusPoint = 2;
                }
            }
            else if (pivotType == TileType.HolyLand)
            {
                if (MyTileType == TileType.Mountain)
                {
                    bonusPoint = 1;
                }
            }
            else if (pivotType == TileType.Theator)
            {
                // 나중에 불가사의 추가할 것
            }

            return bonusPoint;
        }
    }
}