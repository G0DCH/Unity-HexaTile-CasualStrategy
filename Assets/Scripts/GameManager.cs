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

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        // 점수 갱신
        public void RefreshPoint(int point)
        {
            pointText.text = string.Format("Point : {0}", point);
        }
    }
}