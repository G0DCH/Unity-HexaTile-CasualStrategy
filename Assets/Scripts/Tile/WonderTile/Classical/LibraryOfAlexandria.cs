namespace TilePuzzle
{
    // 알렉산드리아 도서관
    public class LibraryOfAlexandria : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 캠퍼스 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Campus)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 캠퍼스 옆에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.Campus)
                {
                    return true;
                }
            }

            return false;
        }
    }
}