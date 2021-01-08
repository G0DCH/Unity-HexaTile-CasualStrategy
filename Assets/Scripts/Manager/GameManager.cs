#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TilePuzzle.Procedural;
using TilePuzzle.Entities;

namespace TilePuzzle
{
    public class GameManager : Utility.Singleton<GameManager>
    {
        public int Score { get { return score; } private set { score = value; } }
        [SerializeField]
        private int score = 0;
        public int BuildPoint { get { return buildPoint; } private set { buildPoint = value; } }
        [SerializeField]
        private int buildPoint = 6;

        public Entity TurnEntity { get; private set; } = null;

        public HexagonTerrain MyHexagonTerrain;

        [Required]
        public TerrainGenerateSettings terrainGenerateSettings;
        public DecorationSpawner[] decorationSpawners;

        private void Awake()
        {
            if (MyHexagonTerrain == null)
            {
                MyHexagonTerrain = FindObjectOfType(typeof(HexagonTerrain)) as HexagonTerrain;

                if (MyHexagonTerrain == null)
                {
                    Debug.LogError("HexagonTerrain 스크립트가 씬에 없음.");
                }
            }
        }

        private void Start()
        {
            Score = 0;
            //BuildPoint = 6;

            UIManager.Instance.RefreshPointText();

            MakeMap();
        }

        // 맵 생성
        [Button]
        public void MakeMap()
        {
            int seed = (int)DateTime.Now.Ticks;

            Procedural.TerrainData terrainData = TerrainGenerator.GenerateTerrainData(seed, terrainGenerateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorationData(seed, terrainData, decorationSpawners);

            MyHexagonTerrain.BuildTerrain(terrainData, decorationData);

            TileManager.Instance.InitTileMap();
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        // 점수 갱신
        public void AddPoint(int point)
        {
            // 타일 배치로 인한 포인트 감소
            BuildPoint -= TileManager.Instance.SelectTileCost;

            Score += point;
            BuildPoint += point;
            UIManager.Instance.RefreshPointText();
            UIManager.Instance.UpdateAgeText();

            if (BuildPoint < 3)
            {
                UIManager.Instance.ActiveGameOver();
            }
        }
    }
}