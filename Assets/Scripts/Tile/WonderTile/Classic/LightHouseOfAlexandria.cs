namespace TilePuzzle
{
    // 알렉산드리아의 등대
    public class LightHouseOfAlexandria : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 항만 건설 시 점수 +wonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Harbor)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 항만 옆 육지와 인접한 바다.
        public override bool WonderLimit(Tile currentTile)
        {
            bool nearHarbor = false;
            bool nearLand = false;

            if (currentTile.MyTileType != TileType.Water)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.Harbor)
                {
                    nearHarbor = true;
                }
                else if (neighbor.MyTileType == TileType.Ground)
                {
                    nearLand = true;
                }

                if (nearLand && nearHarbor)
                {
                    return true;
                }
            }

            return false;
        }
    }
}