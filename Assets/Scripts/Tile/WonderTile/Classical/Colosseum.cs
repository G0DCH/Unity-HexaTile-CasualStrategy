namespace TilePuzzle
{
    // 콜로세움
    public class Colosseum : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 유흥단지의 범위 내 도시 보너스 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.EntertainmentComplex)
            {
                int totalBonus = 0;

                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileBuilding == TileBuilding.City)
                    {
                        totalBonus += wonderBonus;
                    }
                }

                currentTile.ChangeBonus(totalBonus);
            }
        }

        // 유흥단지 옆
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.EntertainmentComplex)
                {
                    return true;
                }
            }

            return false;
        }
    }
}