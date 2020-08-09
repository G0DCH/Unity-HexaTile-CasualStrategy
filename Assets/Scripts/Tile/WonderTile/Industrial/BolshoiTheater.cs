namespace TilePuzzle
{
    // 볼쇼이 극장
    public class BolshoiTheater: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 극장가 건설 시 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.TheaterSquare)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 극장가 옆 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.TheaterSquare)
                {
                    return true;
                }
            }

            return false;
        }
    }
}