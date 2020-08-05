using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class HexagonObject : MonoBehaviour, IReuseable
    {
        public const float Size = 1;

        [ReadOnly] 
        public HexagonPos hexPos;

        [SerializeField, Required] private MeshFilter meshFilter;
        [SerializeField, Required] private MeshRenderer meshRenderer;

        public void SetMesh(Mesh mesh)
        {
            meshFilter.sharedMesh = mesh;
        }

        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material;
        }

        public void OnPooling()
        {
            throw new NotImplementedException();
        }

        public void OnReuse()
        {
            throw new NotImplementedException();
        }
    }
}
