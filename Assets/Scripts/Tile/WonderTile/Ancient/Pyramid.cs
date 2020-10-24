namespace TilePuzzle
{
    // 피라미드
    public class Pyramid : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateCost += WonderFunction;
        }

        // 사막 타일에 짓는 건물 비용 -WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                TileManager.Instance.SelectTileCost -= wonderBonus;
            }
        }

        // 사막, 강 옆에만 지을 수 있음.
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                if (currentTile.MyTileType == TileType.River)
                {
                    return true;
                }
            }

            return false;
        }
    }
}