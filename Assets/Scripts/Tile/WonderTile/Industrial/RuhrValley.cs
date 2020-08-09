namespace TilePuzzle
{
    // 루르 밸리
    public class RuhrValley : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 강 타일에 산업구역 건설 시 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.IndustrialZone)
            {
                if (currentTile.MyTileType == TileType.River)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // 산업 구역 옆 강가에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileType == TileType.River)
            {
                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileBuilding == TileBuilding.IndustrialZone)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}