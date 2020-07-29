namespace TilePuzzle
{
    // 대욕장
    public class GreatBath : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // currentTile이 강 타일일 때 여기에 성지를 지으면 +wonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileType == TileType.River)
            {
                if (tileBuilding == TileBuilding.HolyLand)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // currentTile이 강 타일이라면 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.River)
            {
                return true;
            }

            return false;
        }
    }
}