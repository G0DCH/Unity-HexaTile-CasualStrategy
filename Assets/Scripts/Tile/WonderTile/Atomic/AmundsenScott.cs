namespace TilePuzzle
{
    // 아문센-스콧 연구 기지
    public class AmundsenScott: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 캠퍼스 범위 내 설원 1 타일당 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Campus)
            {
                int totalBonus = 0;

                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileTerrain == TileTerrain.Snow)
                    {
                        totalBonus += wonderBonus;
                    }
                }

                currentTile.ChangeBonus(totalBonus);
            }
        }

        // 캠퍼스 옆 설원에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain != TileTerrain.Snow)
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