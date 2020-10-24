namespace TilePuzzle
{
    // 타지마할
    public class TajMahal : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 건물 보너스가 6 이상이면 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.Bonus > 6)
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