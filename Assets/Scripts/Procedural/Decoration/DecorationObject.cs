using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class DecorationObject : MonoBehaviour, IReuseable
    {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public void OnPooling()
        {

        }

        public void OnReuse()
        {

        }
    }
}
