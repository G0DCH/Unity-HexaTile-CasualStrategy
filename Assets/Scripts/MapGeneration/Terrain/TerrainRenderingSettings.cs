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
        [Min(0.1f)]
        public float cliffDepth = 1.5f;
        [PropertyRange(0.05f, 1f)]
        public float riverSize = 0.2f;
        public Color seaColor;
        public Color lakeColor;
        public bool enableBrightNoise = true;
    }
}
