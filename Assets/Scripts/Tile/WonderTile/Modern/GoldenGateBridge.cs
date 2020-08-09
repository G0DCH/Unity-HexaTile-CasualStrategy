namespace TilePuzzle
{
    // 금문교
    public class GoldenGateBridge : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 물 옆에 건물 건설 시 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
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

        // TODO : 나중에 구현
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            return false;
        }
    }
}