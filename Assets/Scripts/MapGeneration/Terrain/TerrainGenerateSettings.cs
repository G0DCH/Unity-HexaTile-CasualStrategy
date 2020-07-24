using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class TerrainGenerateSettings : ScriptableObject
    {
        public Vector2Int terrainSize;
        public NoiseSettings terrainShapeNoiseSettings;
        public FalloffSettings terrainShapeFalloffSettings;
        // falloff settings
        public float landRatio = 0.6f;
        public float lakeThreshold = 0.3f;
        public float peakMultiplier = 1.1f;
        public int riverSeed;
        public Vector2 riverSpawnRange;
        public float riverSpawnMultiplier;
        public BiomeTableSettings biomeTableSettings;
    }
}
