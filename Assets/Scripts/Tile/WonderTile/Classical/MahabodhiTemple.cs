namespace TilePuzzle
{
    // 마하보디 사원
    public class MahabodhiTemple : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 성지 건설 시 숲 타일 2개당 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.HolySite)
            {
                int bonusCount = 0;

                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileFeature == TileFeature.Forest)
                    {
                        bonusCount += 1;
                    }
                }

                currentTile.ChangeBonus(bonusCount / 2 * wonderBonus);
            }
        }

        // 성지 옆 숲 타일에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.HolySite)
                {
                    if (currentTile.MyTileFeature == TileFeature.Forest)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}