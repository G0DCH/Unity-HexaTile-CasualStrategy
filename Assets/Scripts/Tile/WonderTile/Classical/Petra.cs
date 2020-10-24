namespace TilePuzzle
{
    // 페트라
    public class Petra : WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 사막에 짓는 건물 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 사막에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            if (currentTile.MyTileTerrain == TileTerrain.Desert)
            {
                return true;
            }

            return false;
        }
    }
}