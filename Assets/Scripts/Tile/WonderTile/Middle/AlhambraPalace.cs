namespace TilePuzzle
{
    // 알람브라 궁전
    public class AlhambraPalace : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 주둔지 점수 +WonderBonus(주둔지 기본 점수)
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Encampment)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 주둔지 옆 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.Encampment)
                {
                    return true;
                }
            }

            return false;
        }
    }
}