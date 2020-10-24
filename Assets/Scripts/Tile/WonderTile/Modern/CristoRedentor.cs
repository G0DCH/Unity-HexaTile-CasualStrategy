namespace TilePuzzle
{
    // 리오의 예수상
    public class CristoRedentor: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateRange += WonderFunction;
        }

        // 성지, 극장가 건물 범위 증가
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            bool isRightBuilding = false;

            if (tileBuilding == TileBuilding.HolySite ||
                tileBuilding == TileBuilding.TheaterSquare)
            {
                isRightBuilding = true;
            }

            currentTile.ChangeRange(this, isRightBuilding);
        }

        // 제한 없음.
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            return true;
        }
    }
}