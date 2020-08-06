using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace TilePuzzle.Procedural
{
    public class MobileTestWorld : World
    {
        [Required]
        public TerrainGenerateSettings terrainGenerateSettings;
        [Required]
        public DecorationSpawnSettings decorationSpawnSettings;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {

        }

        [Button]
        public void CreateRandomWorld()
        {
            int seed = (int)DateTime.Now.Ticks;
            terrainGenerateSettings.globalSeed = seed;
            WorldSize = terrainGenerateSettings.terrainSize;

            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(terrainGenerateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorationData(seed, terrainData, decorationSpawnSettings);

            InitializeWorld(WorldSize, terrainData, decorationData);
        }
    }
}
