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
        public TerrainRenderer terrainRenderer;
        [Required]
        public DecorationRenderer decorationRenderer;

        public TerrainGenerateSettings generateSettings;
        public TerrainRenderingSettings renderingSettings;
        public DecorationSpawnSettings decorationSpawnSettings;

        private Vector2Int mapSize;
        private TerrainData terrainData;
        private DecorationData decorationData;

        private void Awake()
        {
            Debug.Assert(terrainRenderer != null, $"Missing {nameof(terrainRenderer)}");
            Debug.Assert(decorationRenderer != null, $"Missing {nameof(decorationRenderer)}");
        }

        private void Start()
        {
            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(generateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorations(generateSettings.globalSeed, terrainData, decorationSpawnSettings);
            BuildWorld(terrainData.terrainSize, terrainData, decorationData);
        }

        [Button]
        public void Test()
        {
            TerrainData terrainData = TerrainGenerator.GenerateTerrainData(generateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorations(generateSettings.globalSeed, terrainData, decorationSpawnSettings);
            BuildWorld(terrainData.terrainSize, terrainData, decorationData);
        }

        public void BuildWorld(Vector2Int mapSize, TerrainData terrainData, DecorationData decorationData)
        {
            this.mapSize = mapSize;
            this.terrainData = terrainData ?? throw new ArgumentNullException(nameof(terrainData));
            this.decorationData = decorationData ?? throw new ArgumentNullException(nameof(decorationData));

            terrainRenderer.Build(terrainData, renderingSettings);
            decorationRenderer.Build(mapSize, decorationData.renderDatas);
        }

        public Decoration? GetDecorationAt(HexagonPos hexPos)
        {
            Vector2Int arrayXY = hexPos.ToArrayXY();
            return decorationData.decorations[arrayXY.x + arrayXY.y * mapSize.x];
        }
    }
}
