namespace TilePuzzle
{
    // 공중정원
    public class HangingGarden : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 강 타일에 도시 타일을 배치하면 점수 +wonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileType == TileType.River)
            {
                if (tileBuilding == TileBuilding.City)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // 강 타일에 설치 가능
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