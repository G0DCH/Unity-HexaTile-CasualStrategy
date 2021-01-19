#pragma warning disable CS0649

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TilePuzzle.Procedural;
using TilePuzzle.Entities;
using TilePuzzle.Entities.AI;
using System.Collections.Generic;

namespace TilePuzzle
{
    public class GameManager : Utility.Singleton<GameManager>
    {
        /// <summary>
        /// 현재 TurnEntity의 점수
        /// </summary>
        public int Score { get { return TurnEntity.Score; }}
        /// <summary>
        /// 현재 TurnEntity의 건설점수
        /// </summary>
        public int BuildPoint { get { return TurnEntity.BuildPoint ; } }

        public Entity TurnEntity
        {
            get
            {
                if (turnEntity == null)
                {
                    InitEntities();
                }

                turnEntity.IsMyTurn = true;
                return turnEntity;
            }
            set
            {
                turnEntity.IsMyTurn = false;
                turnEntity = value;
                turnEntity.IsMyTurn = true;
            }
        }
        [SerializeField]
        private Entity turnEntity = null;

        public HexagonTerrain MyHexagonTerrain;

        public Queue<Entity> Entitys { get; } = new Queue<Entity>();

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

            InitEntities();
        }

        private void Start()
        {
            UIManager.Instance.RefreshPointText();

            MakeMap();
        }

        private void InitEntities()
        {
            if (turnEntity == null)
            {
                turnEntity = FindObjectOfType(typeof(Player)) as Player;
            }

            EnemyAI[] entitys = FindObjectsOfType(typeof(EnemyAI)) as EnemyAI[];

            foreach (var ai in entitys)
            {
                Entitys.Enqueue(ai);
            }
        }

        /// <summary>
        /// 다음 엔티티에게 턴을 넘겨줌
        /// </summary>
        public void NextTurn()
        {
            var prevEntity = TurnEntity;

            TurnEntity = Entitys.Dequeue();

            Entitys.Enqueue(prevEntity);

            if (TurnEntity is EnemyAI ai)
            {
                ai.Action();
            }
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
        public void UpdatePoint(int point)
        {
            TurnEntity.UpdatePoint(point);
            UIManager.Instance.RefreshPointText();
            UIManager.Instance.UpdateAgeText();

            if (BuildPoint < 3)
            {
                UIManager.Instance.ActiveGameOver();
            }
        }
    }
}