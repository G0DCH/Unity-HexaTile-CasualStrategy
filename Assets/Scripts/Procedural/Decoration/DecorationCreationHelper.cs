#if UNITY_EDITOR

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    [ExecuteInEditMode]
    public class DecorationCreationHelper : MonoBehaviour
    {
        [Title("General")]
        public int seed;
        [Range(0.1f, Hexagon.Size / 2f)]
        public float objectRadius;
        [Range(0.1f, Hexagon.Size / 2f)]
        public float spawnRadius;
        public bool useRandomRotation;
        [Range(0f, 0.5f)]
        public float scaleVariant;
        public Transform sampleObjectHolder;
        [FolderPath]
        public string savePath;
        public string saveName;

        [Title("Neighbor")]
        public bool drawNeighbor;
        public bool useNeighborRandomRotation;

        [Title("Debug")]
        public bool autoUpdate;
        private bool settingsUpdated;
        private GameObject decorationSample;

        private void Update()
        {
            if (settingsUpdated)
            {
                settingsUpdated = false;
                RandomPlacement();
            }
        }

        private void OnValidate()
        {
            if (autoUpdate)
            {
                settingsUpdated = true;
            }
        }

        [Button]
        public void RandomPlacement()
        {
            DestoryAllSampleObjects();
            decorationSample = new GameObject("Decoration sample");
            decorationSample.transform.parent = transform;

            System.Random random = new System.Random(seed);
            IEnumerable<Vector2> samplePoints = PoissonDiscSampling.GeneratePoints(random, objectRadius, new Vector2(Hexagon.Size, Hexagon.Size))
                .Select(x => x - Vector2.one * (Hexagon.Size / 2f))
                .Where(x => x.magnitude <= spawnRadius);
            foreach (Vector2 samplePoint in samplePoints)
            {
                GameObject randomSampleObject = sampleObjectHolder.GetChild(random.Next(sampleObjectHolder.childCount)).gameObject;
                GameObject newObject = CreateClone(randomSampleObject, decorationSample.transform);

                newObject.transform.position = new Vector3(samplePoint.x, 0, samplePoint.y);
                if (useRandomRotation)
                {
                    Vector3 randomDirection = Quaternion.AngleAxis((float)random.NextDouble() * 360f, Vector3.up) * Vector3.forward;
                    newObject.transform.LookAt(randomDirection);
                }
                newObject.transform.localScale += newObject.transform.localScale * ((float)random.NextDouble() * scaleVariant * 2 - scaleVariant);
            }

            if (drawNeighbor)
            {
                for (int hexZ = -1; hexZ <= 1; hexZ++)
                {
                    for (int hexX = -1; hexX <= 1; hexX++)
                    {
                        HexagonPos neighborHexPos = new HexagonPos(hexX, hexZ);
                        if (neighborHexPos.HexagonDistance > 1 || (hexX == 0 && hexZ == 0))
                        {
                            continue;
                        }

                        GameObject neighborSample = Instantiate(decorationSample, transform);
                        neighborSample.transform.position = neighborHexPos.ToWorldPos();
                        if (useNeighborRandomRotation)
                        {
                            Vector3 randomDirection = Quaternion.AngleAxis((float)random.NextDouble() * 360f, Vector3.up) * Vector3.forward;
                            neighborSample.transform.LookAt(randomDirection);
                        }
                    }
                }
            }
        }

        [Button]
        private void SaveToPrefab()
        {
            if (decorationSample == null)
            {
                Debug.LogError($"저장할 Decoration이 없음");
                return;
            }

            GameObject newPrefab = Instantiate(decorationSample);
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{savePath}/{saveName}.prefab");
            PrefabUtility.SaveAsPrefabAsset(newPrefab, uniquePath);
            DestroyImmediate(newPrefab);
        }

        private GameObject CreateClone(GameObject originObject, Transform parent)
        {
            GameObject newObject = Instantiate(originObject, parent);
            return newObject;
        }

        private void DestoryAllSampleObjects()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}
#endif