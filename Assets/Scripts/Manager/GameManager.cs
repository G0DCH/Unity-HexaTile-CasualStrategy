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

        public int TotalPoint = 0;
        public int RemainPoint = 6;

        public GameObject GameOverPanel;
        public Text GameOverText;

        private void Start()
        {
            GameOverPanel.SetActive(false);
            TotalPoint = 0;
            RemainPoint = 6;
            pointText.text = string.Format("TotalPoint : {0}\n RemainPoint : {1}", TotalPoint, RemainPoint);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        // 점수 갱신
        public void RefreshPoint(int point)
        {
            // 타일 배치로 인한 포인트 감소
            RemainPoint -= 3;

            TotalPoint += point;
            RemainPoint += point;
            pointText.text = string.Format("TotalPoint : {0}\n RemainPoint : {1}", TotalPoint, RemainPoint);

            if (RemainPoint < 3)
            {
                ActiveGameOver();
            }
        }

        // 게임 오버 패널 켜기
        private void ActiveGameOver()
        {
            GameOverPanel.SetActive(true);
            GameOverText.text = string.Format("GameOver!!\nTotalPoint : {0}", TotalPoint);
        }
    }
}