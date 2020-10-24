namespace TilePuzzle
{
    // 성 소피아 대성당
    public class SaintSophia : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateCost += WonderFunction;
        }

        // 성지 건설 비용 -WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.HolySite)
            {
                TileManager.Instance.SelectTileCost -= wonderBonus;
            }
        }

        // 성지 옆 타일에 건설 가능
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