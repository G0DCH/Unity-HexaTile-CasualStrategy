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
    public enum TileFeature { Forest, Jungle, Empty }

    [System.Serializable]
    public enum TileTerrain { Grassland, Plains, Desert, Snow, Tundra, RainForest }

    [System.Serializable]
    public enum Age { Ancient, Classical, Medieval, Renaissance, Industrial, Modern, Atomic, Future }

    /// <summary>
    /// 순서대로 [대기, 행동 준비, 도시 설치, 건물 설치, 불가사의 설치, 거래]
    /// </summary>
    [System.Serializable]
    public enum ActionState { Idle, Ready, City, Building, Wonder, Trade }
}