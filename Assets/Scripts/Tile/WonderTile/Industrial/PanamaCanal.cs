namespace TilePuzzle
{
    // 파나마 운하
    public class PanamaCanal: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 운하 건설시 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.Carnal)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // TODO : 조건 구현
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            return false;
        }
    }
}