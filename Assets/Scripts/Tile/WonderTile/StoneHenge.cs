namespace TilePuzzle
{
    public class StoneHenge : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderCost += WonderFunction;
        }

        // 초원이나 평원에 성지를 지으면 비용 - wonderBonus
        public override void WonderFunction(Tile currentTile, Tile selectedTile)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Grassland ||
                currentTile.MyTileTerrain == TileTerrain.Plains)
            {
                if (selectedTile.MyTileType == TileType.HolyLand)
                {
                    TileManager.Instance.SelectTileCost -= wonderBonus;
                }
            }
        }

        // 성지 옆에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            foreach (var neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.HolyLand)
                {
                    return true;
                }
            }

            return false;
        }
    }
}