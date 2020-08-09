namespace TilePuzzle
{
    // 마라카낭 경기장
    public class EstadioDoMarakana: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 유흥단지의 범위 내 도시 당 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            int totalBonus = 0;

            if (tileBuilding == TileBuilding.EntertainmentComplex)
            {
                foreach(Tile rangeTile in currentTile.RangeTiles)
                {
                    totalBonus += wonderBonus;
                }
            }

            currentTile.ChangeBonus(totalBonus);
        }

        // 유흥단지 옆 건설 가능
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