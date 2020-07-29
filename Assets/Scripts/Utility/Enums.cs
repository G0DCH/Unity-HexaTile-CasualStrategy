namespace TilePuzzle
{
    [System.Serializable]
    public enum TileType
    {
        Water, River, Mountain, Ground, Empty
    }

    [System.Serializable]
    public enum TileBuilding
    {
        Campus, Factory, GovernmentBuilding, HolyLand,
        Theator, WaterPipe, City, Empty
    }

    [System.Serializable]
    public enum TileFeature { Forest, RainForest, Empty }

    [System.Serializable]
    public enum TileTerrain { Grassland, Plains, Desert, Snow, Tundra }
}