namespace TilePuzzle
{
    // 병마용
    public class TerracottaWarriors : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 주둔지 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Aqueduct)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 주둔지 옆만 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.Aqueduct)
                {
                    return true;
                }
            }

            return false;
        }
    }
}