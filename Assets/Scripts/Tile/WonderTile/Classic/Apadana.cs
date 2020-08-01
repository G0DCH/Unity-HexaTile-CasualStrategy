namespace TilePuzzle
{
    // 아파다나
    public class Apadana : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 불가사의 건설 시 보너스 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Wonder)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
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