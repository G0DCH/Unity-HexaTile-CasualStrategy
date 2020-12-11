using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilePuzzle.Procedural
{

    [Serializable]
    public struct TileInfo
    {
        public HexagonPos hexPos;
        public bool isWater;
        public bool isSea;
        public bool isCoast;
        public bool hasRiver;
        public Biome biome;

        public bool IsLake => isWater && isSea == false;
    }
}
