#pragma warning disable CS0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        private void Start()
        {
            GameOverPanel.SetActive(false);
            Score = 0;
            BuildPoint = 6;
            pointText.text = string.Format("Score : {0}\n BuildPoint : {1}", Score, BuildPoint);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        // 점수 갱신
        public void RefreshPoint(int point)
        {
            // 타일 배치로 인한 포인트 감소
            BuildPoint -= 3;

            Score += point;
            BuildPoint += point;
            pointText.text = string.Format("Score : {0}\n BuildPoint : {1}", Score, BuildPoint);

            if (BuildPoint < 3)
            {
                ActiveGameOver();
            }
        }

        // 게임 오버 패널 켜기
        private void ActiveGameOver()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("GameOver!!\nTotalPoint : {0}", Score);
        }
    }
}