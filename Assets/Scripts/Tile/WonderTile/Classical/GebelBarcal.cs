namespace TilePuzzle
{
    // 게벨 바르칼
    public class GebelBarcal : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 사막 타일에 짓는 성지 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.HolySite)
            {
                if (currentTile.MyTileTerrain == TileTerrain.Desert)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // 사막에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                return true;
            }

            return false;
        }
    }
}