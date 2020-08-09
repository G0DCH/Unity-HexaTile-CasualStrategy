namespace TilePuzzle
{
    // 성 바실리 대성당
    public class SaintBasils: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
            TileManager.Instance.MyWonderRange += RangeUp;
        }

        // 툰드라에 건물 건설 시 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Tundra)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 툰드라에 건물 건설 시 범위 증가
        public void RangeUp(Tile currentTile, TileBuilding tileBuilding)
        {
            bool isTundra = false;

            if (currentTile.MyTileTerrain == TileTerrain.Tundra)
            {
                isTundra = true;
            }

            currentTile.ChangeRange(this, isTundra);
        }

        // 도시 옆 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.City)
                {
                    return true;
                }
            }

            return false;
        }
    }
}