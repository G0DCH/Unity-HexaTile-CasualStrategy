namespace TilePuzzle
{
    // 우에이 테오칼리
    public class HueyiTeocalli : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 범위 내 호수 당 건물 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            int bonusCount = 0;
            foreach(Tile rangeTile in currentTile.RangeTiles)
            {
                if (rangeTile.MyTileType == TileType.Water)
                {
                    if (!rangeTile.MyHexagonInfo.isSea)
                    {
                        bonusCount++;
                    }
                }
            }

            currentTile.ChangeBonus(bonusCount * wonderBonus);
        }

        // 땅과 인접한 호수에 설치 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water)
            {
                if (!currentTile.MyHexagonInfo.isSea)
                {
                    foreach (Tile neighbor in currentTile.NeighborTiles)
                    {
                        if (neighbor.MyTileType == TileType.Ground)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}