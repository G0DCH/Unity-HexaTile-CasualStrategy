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
        public MeshRenderer meshRenderer;
    }
}
