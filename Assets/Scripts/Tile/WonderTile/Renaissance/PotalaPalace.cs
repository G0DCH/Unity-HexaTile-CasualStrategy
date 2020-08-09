namespace TilePuzzle
{
    // 포탈라 궁
    public class PotalaPalace: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 건물 범위 내에 산이 있으면 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            foreach(Tile rangeTile in currentTile.RangeTiles)
            {
                if (rangeTile.MyTileType == TileType.Mountain)
                {
                    currentTile.ChangeBonus(wonderBonus);
                    return;
                }
            }
        }

        // 산 옆에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.Mountain)
                {
                    return true;
                }
            }

            return false;
        }
    }
}