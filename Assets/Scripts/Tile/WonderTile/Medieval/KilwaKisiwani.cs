namespace TilePuzzle
{
    // 킬와 키시와니
    public class KilwaKisiwani: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateRange += WonderFunction;
        }

        // 물 타일 옆에 건물을 지으면 건물 범위 +1
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            bool nearWater = false;

            foreach(Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.Water)
                {
                    nearWater = true;
                    break;
                }
            }

            currentTile.ChangeRange(this, nearWater);
        }

        // 물 옆 땅에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileType == TileType.Water)
                {
                    return true;
                }
            }

            return false;
        }
    }
}