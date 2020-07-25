using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class World : MonoBehaviour
    {
        [Required]
        public Terrain terrain;
        [Required]
        public TerrainGenerateSettings generateSettings;
        [Required]
        public TerrainRenderingSettings renderingSettings;

        private void Awake()
        {
            Debug.Assert(generateSettings != null);
            Debug.Assert(renderingSettings != null);
        }

        private void Start()
        {
            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(generateSettings);
            terrain.Initialize(terrainData, renderingSettings);
        }
    }
}
