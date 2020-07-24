using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [Serializable]
    public class FalloffSettings
    {
        public Vector2 falloffParameter = new Vector2(3f, 2.2f);
    }
}
