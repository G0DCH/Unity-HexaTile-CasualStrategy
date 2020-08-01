namespace TilePuzzle
{
    // 마추픽추
    public class MachuPicchu : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 산 타일이 인접한 모든 상업 중심지, 극장가, 산업구역에 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.CommercialHub ||
                tileBuilding == TileBuilding.TheaterSquare ||
                tileBuilding == TileBuilding.IndustrialZone)
            {
                int totalBonus = 0;

                foreach (Tile rangeTile in currentTile.RangeTiles)
                {
                    if (rangeTile.MyTileType == TileType.Mountain)
                    {
                        totalBonus += wonderBonus;
                    }
                }

                currentTile.ChangeBonus(totalBonus);
            }
        }

        // 산에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Mountain)
            {
                return true;
            }

            return false;
        }
    }
}