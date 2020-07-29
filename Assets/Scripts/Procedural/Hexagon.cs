using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class Hexagon : MonoBehaviour
    {
        public const float Size = 1;

        [ReadOnly]
        public HexagonPos hexPos;
        public MeshFilter meshFilter;

        public bool IsWater { get; private set; }
        public bool IsLand => !IsWater;
        public bool HasRiver { get; private set; }
        public bool HasMountain { get; private set; }
        public bool HasForest { get; private set; }
        public int BiomeId { get; private set; }

        public void SetProperties(Center center)
        {
            IsWater = center.isWater;
            HasRiver = center.NeighborCorners.Any(x => x.river > 0);
            HasMountain = center.hasMountain;
            HasForest = center.hasForest;
            BiomeId = center.biomeId;
        }
    }
}
