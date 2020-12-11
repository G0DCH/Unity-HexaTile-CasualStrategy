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

        public TileModel land;
        public TileModel water;

        private GameObject decorationObject;
        private bool isVisible;

        public TileInfo TileInfo { get; set; }
        public DecorationInfo? DecorationInfo { get; private set; }
        public bool IsVisible
        {
            get { return isVisible; }
            set
            {
                isVisible = value;
                if (decorationObject != null)
                {
                    decorationObject.SetActive(value);
                }
            }
        }

        #region IReusable 인터페이스
        public void OnPooling()
        {
            land.Clear();
            water.Clear();

            DestroyDecoration();
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
            this.decorationObject.transform.parent = land.transform;
            this.decorationObject.transform.localPosition = Vector3.zero;
            DecorationInfo = decorationInfo;
        }

        public void DestroyDecoration()
        {
            if (decorationObject != null)
            {
                Destroy(decorationObject);
                DecorationInfo = null;
            }
        }

        [Serializable]
        public struct TileModel
        {
            [Required] public Transform transform;
            [SerializeField, Required] private MeshFilter meshFilter;
            [SerializeField, Required] private MeshRenderer meshRenderer;

            public Mesh Mesh
            {
                set
                {
                    meshFilter.sharedMesh = value;
                }
            }

            public Material Material
            {
                set
                {
                    meshRenderer.sharedMaterial = value;
                }
            }

            public void Clear()
            {
                meshFilter.sharedMesh = null;
                meshRenderer.sharedMaterial = null;
            }
        }
    }
}
