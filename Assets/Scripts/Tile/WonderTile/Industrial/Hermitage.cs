namespace TilePuzzle
{
    // 예르미타시 미술관
    public class Hermitage: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 극장가 건설 시 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.TheaterSquare)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 강에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileType == TileType.River)
            {
                return true;
            }

            return false;
        }
    }
}