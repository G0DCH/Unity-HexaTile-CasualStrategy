using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    // 시대 관리
    public class AgeManager : Utility.Singleton<AgeManager>
    {
        /// <summary>
        /// 현재 시대
        /// </summary>
        public Age WorldAge
        {
            get
            {
                return GameManager.Instance.TurnEntity.WorldAge;
            }
            set
            {
                GameManager.Instance.TurnEntity.WorldAge = value;
            }
        }

        [Space, Header("시대 별 다음 시대까지 벌어야 하는 점수")]
        public List<int> AgeLimitList = new List<int>();
        public int AgeLimit { get; private set; } = 0;

        private void Start()
        {
            WorldAge = Age.Ancient;
            AgeLimit += AgeLimitList[(int)WorldAge];
            UIManager.Instance.UpdateAgeText();
            UIManager.Instance.ActiveBuildingButtons();
        }

        // 다음 시대로 바꿈
        public void NextAge()
        {
            if (WorldAge < Age.Ancient)
            {
                Debug.LogError(string.Format("잘못된 시대임.\nWorldAge : {0}", WorldAge));
                return;
            }
            else if (AgeLimit > GameManager.Instance.Score)
            {
                Debug.LogError(string.Format("점수가 모자름.\n Score : {0}", GameManager.Instance.Score));
                return;
            }

            // 미래 시대 이전이라면 다음 시대로 나아감
            if (WorldAge < Age.Atomic)
            {
                WorldAge++;
                AgeLimit += AgeLimitList[(int)WorldAge];
            }
            else if(WorldAge == Age.Atomic)
            {
                WorldAge++;
                UIManager.Instance.ActiveWin();
            }
            else
            {
                Debug.LogError(string.Format("현재 시대가 미래 시대 이상임.\nWorldAge : {0}", WorldAge));
                return;
            }

            // 다음 시대로 넘어가면 새 건물 해금함.
            UIManager.Instance.ActiveBuildingButtons();

            // 새로 추가된 불가사의 이름 추가
            DataTableManager.Instance.InitWonderNames();

            // TODO : 
            //        섬의 새 구역을 해금함.

            // 시대 텍스트 갱신
            UIManager.Instance.UpdateAgeText();
        }
    }
}