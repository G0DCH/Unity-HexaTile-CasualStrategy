using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TilePuzzle.Procedural
{
    public abstract class DecorationSpawner : ScriptableObject
    {
        public string saltString;

        public abstract DecorationData Spawn(int seed, TerrainData terrainData, DecorationData inputDecorationData);

        [Serializable]
        public struct DecorationPrefabData
        {
            public string name;
            [ListDrawerSettings(Expanded = true)]
            public GameObject[] prefabs;
            [ListDrawerSettings(Expanded = true)]
            public string[] spawnableBiomeNames;
        }
    }
}
