namespace TilePuzzle
{
    // 몽생미셸 수도원
    public class MontSaintMichel : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 성지가 범위 내 주둔지 타일마다 +WonderBouns
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.HolySite)
            {
                int bonusCount = 0;

                foreach (Tile neighbor in currentTile.NeighborTiles)
                {
                    if (neighbor.MyTileBuilding == TileBuilding.Encampment)
                    {
                        bonusCount++;
                    }
                }

                currentTile.ChangeBonus(bonusCount * wonderBonus);
            }
        }

        // 물 옆 강에 건설 가능
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
                    if (neighbor.MyTileType == TileType.Water)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}