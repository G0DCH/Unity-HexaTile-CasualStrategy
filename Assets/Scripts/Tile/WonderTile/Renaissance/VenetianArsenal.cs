namespace TilePuzzle
{
    // 베네치아 군수창고
    public class VenetianArsenal : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 물타일 옆에 산업구역, 주둔지, 항만을 지으면 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.IndustrialZone ||
                tileBuilding == TileBuilding.Encampment ||
                tileBuilding == TileBuilding.Harbor)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileType == TileType.Water)
                    {
                        currentTile.ChangeBonus(wonderBonus);
                    }
                }
            }
        }

        // 산업구역과 인접한 물 타일
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType != TileType.Water)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.IndustrialZone)
                {
                    return true;
                }
            }

            return false;
        }
    }
}