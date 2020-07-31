namespace TilePuzzle
{
    // 아르테미스 신전
    public class ArtemisTemple : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.MyWonderBonus += WonderFunction;
        }

        // 초원이나 평원에 성지를 지으면 +wonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Grassland ||
                currentTile.MyTileTerrain == TileTerrain.Plains)
            {
                if (tileBuilding == TileBuilding.HolySite)
                {
                    currentTile.ChangeBonus(wonderBonus);
                }
            }
        }

        // 초원이나 평원에 지을 수 있음
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Grassland)
            {
                return true;
            }
            else if (currentTile.MyTileTerrain == TileTerrain.Plains)
            {
                return true;
            }

            return false;
        }
    }
}