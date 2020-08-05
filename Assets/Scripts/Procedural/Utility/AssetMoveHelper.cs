using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public class AssetMoveHelper : MonoBehaviour
    {
        public GameObject[] targets;

        [FolderPath]
        public string meshFolder;
        [FolderPath]
        public string materialFolder;

        [Button]
        public void MoveAssets()
        {
            var meshes = new HashSet<Mesh>();
            var materials = new HashSet<Material>();

            foreach (GameObject item in targets)
            {
                foreach (MeshFilter meshFilter in item.GetComponentsInChildren<MeshFilter>())
                {
                    meshes.Add(meshFilter.sharedMesh);
                }

                foreach (MeshRenderer meshRenderer in item.GetComponentsInChildren<MeshRenderer>())
                {
                    materials.AddRange(meshRenderer.sharedMaterials);
                }
            }

            foreach (Mesh mesh in meshes)
            {
                string oldPath = AssetDatabase.GetAssetPath(mesh);
                string name = oldPath.Substring(oldPath.LastIndexOf("/") + 1);
                string newPath = $"{meshFolder}/{name}";

                AssetDatabase.MoveAsset(oldPath, newPath);
            }

            foreach (Material material in materials)
            {
                string oldPath = AssetDatabase.GetAssetPath(material);
                string name = oldPath.Substring(oldPath.LastIndexOf("/") + 1);
                string newPath = $"{materialFolder}/{name}";

                AssetDatabase.MoveAsset(oldPath, newPath);
            }
        }
    }
}
