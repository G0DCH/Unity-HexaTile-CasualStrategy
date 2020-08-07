namespace TilePuzzle
{
    // 치첸 이트사
    public class ChichenItza : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 열대 우림 타일이 범위 내 모든 상업 중심지, 극장가, 산업 구역에 점수 +wonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.CommercialHub ||
                tileBuilding == TileBuilding.TheaterSquare ||
                tileBuilding == TileBuilding.IndustrialZone)
            {
                int bonusCount = 0;

                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileFeature == TileFeature.RainForest)
                    {
                        bonusCount++;
                    }
                }

                currentTile.ChangeBonus(bonusCount * wonderBonus);
            }
        }

        // 열대 우림에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileFeature == TileFeature.RainForest)
            {
                return true;
            }

            return false;
        }
    }
}