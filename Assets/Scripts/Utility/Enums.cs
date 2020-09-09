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
        Campus, IndustrialZone, GovernmentPlaza, HolySite,
        TheaterSquare, Aqueduct, City, Harbor,
        CommercialHub, Encampment, Wonder, EntertainmentComplex,
        Carnal, Empty
    }

    [System.Serializable]
    public enum TileFeature { Forest, RainForest, Empty }

    [System.Serializable]
    public enum TileTerrain { Grassland, Plains, Desert, Snow, Tundra, RainForest }

    [System.Serializable]
    public enum Age { Ancient, Classical, Medieval, Renaissance, Industrial, Modern, Atomic, Future }
}