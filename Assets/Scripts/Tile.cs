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

        private void ChangeTileType(TileType tileType)
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
    }
}