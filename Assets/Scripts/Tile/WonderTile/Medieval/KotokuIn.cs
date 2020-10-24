namespace TilePuzzle
{
    // 고토쿠인
    public class KotokuIn: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 성지가 범위 내 주둔지 타일이 있으면 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.HolySite)
            {
                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileBuilding == TileBuilding.Encampment)
                    {
                        currentTile.ChangeBonus(wonderBonus);
                        return;
                    }
                }
            }
        }

        // 성지 옆에 건설 가능
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
                    return true;
                }
            }

            return false;
        }
    }
}