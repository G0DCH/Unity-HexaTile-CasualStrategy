namespace TilePuzzle
{
    // 상코레 대학
    public class SankoreUniv : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 사막 타일에 짓는 상업 중심지, 캠퍼스 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Campus ||
                tileBuilding == TileBuilding.CommercialHub)
            {
                if (currentTile.MyTileTerrain == TileTerrain.Desert)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // 캠퍼스 옆 사막에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileBuilding == TileBuilding.Campus)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}