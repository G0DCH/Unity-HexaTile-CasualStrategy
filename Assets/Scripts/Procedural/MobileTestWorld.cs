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
        public DecorationSpawner[] decorationSpawners;

        private void Awake()
        {
            //Application.targetFrameRate = 60;
        }

        private void Update()
        {

        }

        [Button]
        public void CreateRandomWorld()
        {
            int seed = (int)DateTime.Now.Ticks;
            WorldSize = terrainGenerateSettings.terrainSize;

            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(seed, terrainGenerateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorationData(seed, terrainData, decorationSpawners);

            InitializeWorld(WorldSize, terrainData, decorationData);
        }
    }
}
