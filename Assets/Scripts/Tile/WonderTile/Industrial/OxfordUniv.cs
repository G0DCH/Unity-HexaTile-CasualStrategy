namespace TilePuzzle
{
    // 옥스퍼드 대학교
    public class OxfordUniv: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderRange += WonderFunction;
        }

        // 캠퍼스 범위 증가
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            bool isCampus = false;

            if (tileBuilding == TileBuilding.Campus)
            {
                isCampus = true;
            }

            currentTile.ChangeRange(this, isCampus);
        }

        // 캠퍼스 옆 초원이나 평원 타일에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain == TileTerrain.Grassland ||
                currentTile.MyTileTerrain == TileTerrain.Plains)
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