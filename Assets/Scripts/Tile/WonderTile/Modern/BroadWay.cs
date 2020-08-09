namespace TilePuzzle
{
    // 브로드웨이
    public class BroadWay: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
            TileManager.Instance.MyWonderRange += RangeUp;
        }

        // 극장가 건설 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.TheaterSquare)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 극장가 건물 범위 +1
        public void RangeUp(Tile currentTile, TileBuilding tileBuilding)
        {
            bool isTheater = false;

            if (tileBuilding == TileBuilding.TheaterSquare)
            {
                isTheater = true;
            }

            currentTile.ChangeRange(this, isTheater);
        }

        // 극장가 옆에 건설 가능
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