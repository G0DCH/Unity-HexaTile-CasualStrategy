namespace TilePuzzle
{
    // 자유의 여신상
    public class StatueOfLiberty: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 물 타일 옆에 도시를 지으면 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.City)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileType == TileType.Water)
                    {
                        currentTile.ChangeBonus(wonderBonus);
                        return;
                    }
                }
            }
        }

        // 항만 옆 물에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileBuilding == TileBuilding.Harbor)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}