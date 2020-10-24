namespace TilePuzzle
{
    // 카사 데 콘트라타시온
    public class CasaDeContratacion: WonderTile
    {
        public override void AddToDelegate()
        {
            TileManager.Instance.CalculateBonusByWonder += WonderFunction;
        }

        // 정부 청사 건설 점수 +WonderBonus
        public override void WonderFunction(Tile currentTile, TileBuilding tileBuilding)
        {
            if (tileBuilding == TileBuilding.GovernmentPlaza)
            {
                currentTile.ChangeBonus(wonderBonus);
            }
        }

        // 정부청사 옆에 건설 가능
        public override bool WonderLimit(Tile currentTile)
        {
            if (currentTile.MyTileType == TileType.Water ||
                currentTile.MyTileType == TileType.Mountain)
            {
                return false;
            }

            foreach (Tile neighbor in currentTile.NeighborTiles)
            {
                if (neighbor.MyTileBuilding == TileBuilding.GovernmentPlaza)
                {
                    return true;
                }
            }

            return false;
        }
    }
}