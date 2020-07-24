using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [Serializable]
    public class NoiseSettings
    {
        public NoiseType noiseType;
        public int seed;
        public Vector3 offset;
        [Range(1, 10)] public int octaves = 4;
        [Min(0)] public float lacunarity = 2;
        [Min(0)] public float persistance = 0.5f;
        [Min(0)] public float scale = 1;
        [Min(0)] public float strength = 1;
        [Min(0)] public float weightMultiplier = 1;

        public enum NoiseType
        {
            Perlin, Simplex
        }
    }
}
