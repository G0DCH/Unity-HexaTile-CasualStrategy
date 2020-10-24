namespace TilePuzzle
{
    // 거신상
    public class ColossusOfRhodes : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 상업 중심지 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.CommercialHub)
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