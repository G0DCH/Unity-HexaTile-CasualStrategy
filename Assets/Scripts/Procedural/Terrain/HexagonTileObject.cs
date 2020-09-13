#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class HexagonTileObject : MonoBehaviour, IReusable
    {
        public const float TileSize = 1;

        [SerializeField, Required] private MeshFilter meshFilter;
        [SerializeField, Required] private MeshRenderer meshRenderer;
        private GameObject decorationObject;

        public TileInfo TileInfo { get; set; }
        public DecorationInfo? DecorationInfo { get; private set; }

        public Mesh TileMesh
        {
            set
            {
                meshFilter.sharedMesh = value;
            }
        }
        public Material TileMaterial
        {
            set
            {
                meshRenderer.sharedMaterial = value;
            }
        }

        #region IReusable 인터페이스
        public void OnPooling()
        {
            TileMesh = null;
            TileMaterial = null;

            if (decorationObject != null)
            {
                Destroy(decorationObject);
                DecorationInfo = null;
            }
        }

        public void OnReuse()
        {

        }
        #endregion

        public void SetDecoration(GameObject decorationObject, DecorationInfo decorationInfo)
        {
            if (decorationObject == null)
            {
                throw new ArgumentNullException(nameof(decorationObject));
            }

            this.decorationObject = decorationObject;
            this.decorationObject.transform.parent = transform;
            this.decorationObject.transform.localPosition = Vector3.zero;
            DecorationInfo = decorationInfo;
        }
    }
}
