using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class TerrainRenderingSettings : ScriptableObject
    {
        [Title("Land")]
        public Material landMaterial;
        [Min(0.1f)] public float cliffDepth = 1.5f;
        public bool enableBrightNoise = true;

        [Title("Water")]
        public Material seaMaterial;
        public Material lakeMaterial;
        [PropertyRange(0.05f, 1f)]
        public float riverSize = 0.2f;
        [PropertyRange(0, nameof(cliffDepth))]
        public float waterDepth = 0.3f;

        [Title("Prefabs")]
        [Required] public Hexagon hexagonPrefab;
        [Required] public GameObject mountainPrefab;
        [Required] public GameObject forestPrefab;
    }
}
