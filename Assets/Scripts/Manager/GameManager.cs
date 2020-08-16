#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TilePuzzle.Procedural;

namespace TilePuzzle
{
    public class GameManager : Utility.Singleton<GameManager>
    {
        // 점수
        [SerializeField]
        private Text pointText;

        public int Score = 0;
        public int BuildPoint = 6;

        public GameObject GameOverPanel;
        public Text GameOverText;

        public World World;

        [SerializeField]
        private Vector2Int WorldSize;

        [Required]
        public TerrainGenerateSettings terrainGenerateSettings;
        public DecorationSpawner[] decorationSpawners;

        private void Awake()
        {
            if (World == null)
            {
                World = FindObjectOfType(typeof(Procedural.World)) as Procedural.World;

                if (World == null)
                {
                    Debug.LogError("World 스크립트이 씬에 없음.");
                }
            }
        }

        private void Start()
        {
            GameOverPanel.SetActive(false);
            Score = 0;
            BuildPoint = 6;
            pointText.text = string.Format("Score : {0}\n BuildPoint : {1}", Score, BuildPoint);

            MakeMap();
        }

        // 맵 생성
        [Button]
        public void MakeMap()
        {
            int seed = (int)DateTime.Now.Ticks;

            Procedural.TerrainData terrainData = TerrainGenerator.GenerateTerrainData(seed, terrainGenerateSettings);
            DecorationData decorationData = DecorationGenerator.GenerateDecorationData(seed, terrainData, decorationSpawners);

            World.InitializeWorld(WorldSize, terrainData, decorationData);

            TileManager.Instance.InitTileMap();
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        // 점수 갱신
        public void RefreshPoint(int point)
        {
            // 타일 배치로 인한 포인트 감소
            BuildPoint -= TileManager.Instance.SelectTileCost;

            Score += point;
            BuildPoint += point;
            pointText.text = string.Format("Score : {0}\nBuildPoint : {1}", Score, BuildPoint);

            if (BuildPoint < 3)
            {
                ActiveGameOver();
            }
        }

        // 게임 오버 패널 켜기
        private void ActiveGameOver()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("GameOver!!\nScore : {0}", Score);
        }
    }
}