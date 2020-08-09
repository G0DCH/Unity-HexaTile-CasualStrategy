namespace TilePuzzle
{
    // 시드니 오페라 하우스
    public class SydneyOperaHouse: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 물 타일 옆에 극장가를 지으면 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.TheaterSquare)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileType == TileType.Water)
                    {
                        currentTile.ChangeBonus(wonderBonus);
                        return;
                    }
                }
            }
        }

        // 항만 옆 땅 옆 물 타일에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType != TileType.Water)
            {
                return false;
            }

            bool nearGround = false;
            bool nearHarbor = false;

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.Ground )
                {
                    nearGround = true;
                }
                else if (neighbor.MyTileBuilding == TileBuilding.Harbor)
                {
                    nearHarbor = true;
                }

                if (nearGround && nearHarbor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}