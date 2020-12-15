using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace TilePuzzle
{
    public class TilePlacementDrawer : MonoBehaviour
    {
        [Required] public Material placeableMaterial;
        [Required] public Material notPlaceableMaterial;
        public bool castShadow = false;

        private bool isTilePlaceable;
        private GameObject placementObject;
        private Dictionary<Renderer, RendererInfo> originRendererInfoMap;

        private void Awake()
        {
            originRendererInfoMap = new Dictionary<Renderer, RendererInfo>();
        }

        public bool IsTilePlaceable
        {
            get { return isTilePlaceable; }
            set
            {
                if (placementObject != null)
                {
                    // Change renderer infos
                    foreach (Renderer renderer in placementObject.GetComponentsInChildren<Renderer>())
                    {
                        renderer.sharedMaterial = value ? placeableMaterial : notPlaceableMaterial;
                        renderer.shadowCastingMode = castShadow ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    }
                }

                isTilePlaceable = value;
            }
        }

        public GameObject PlacementObject
        {
            get { return placementObject; }
            set
            {
                if (placementObject != null)
                {
                    // Restore previous object's renderer infos
                    foreach (Renderer renderer in placementObject.GetComponentsInChildren<Renderer>())
                    {
                        if (originRendererInfoMap.TryGetValue(renderer, out RendererInfo rendererInfo))
                        {
                            renderer.sharedMaterials = rendererInfo.sharedMaterials;
                            renderer.shadowCastingMode = rendererInfo.shadowCastingMode;
                        }
                    }
                    originRendererInfoMap.Clear();
                }

                if (value != null)
                {
                    // Backup renderer infos
                    originRendererInfoMap.Clear();
                    foreach (Renderer renderer in value.GetComponentsInChildren<Renderer>())
                    {
                        originRendererInfoMap.Add(renderer, new RendererInfo(renderer.sharedMaterials, renderer.shadowCastingMode));
                    }

                    // Update renderer info
                    IsTilePlaceable = isTilePlaceable;
                }

                placementObject = value;
            }
        }

        private struct RendererInfo
        {
            public readonly Material[] sharedMaterials;
            public readonly ShadowCastingMode shadowCastingMode;

            public RendererInfo(Material[] sharedMaterials, ShadowCastingMode shadowCastingMode)
            {
                this.sharedMaterials = sharedMaterials;
                this.shadowCastingMode = shadowCastingMode;
            }
        }
    }
}
