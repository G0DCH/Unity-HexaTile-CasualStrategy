namespace TilePuzzle
{
    // 앙코르 와트
    public class AngkorWat : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 도시 건설시 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.City)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 송수로 옆 건설 가능
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