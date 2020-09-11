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
    public class MobileTestWorld : HexagonTerrain
    {
        [Required]
        public TerrainGenerateSettings terrainGenerateSettings;
        public DecorationSpawner[] decorationSpawners;

        protected override void Awake()
        {
            base.Awake();
            //Application.targetFrameRate = 60;
        }

        protected override void Update()
        {
            base.Update();
        }

        [Button]
        public void CreateRandomWorld()
        {
            int seed = (int)DateTime.Now.Ticks;
            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(seed, terrainGenerateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorationData(seed, terrainData, decorationSpawners);

            BuildTerrain(terrainData, decorationData);
        }
    }
}
