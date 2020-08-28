using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    // 시대 관리
    public class AgeManager: Utility.Singleton<AgeManager>
    {
        /// <summary>
        /// 현재 시대
        /// </summary>
        public Age WorldAge = Age.Ancient;

        private void Start()
        {
            WorldAge = Age.Ancient;
            UIManager.Instance.UpdateAgeText();
        }

        // 다음 시대로 바꿈
        public void NextAge()
        {
            if (WorldAge < Age.Ancient)
            {
                Debug.LogError(string.Format("잘못된 시대임.\nWorldAge : {0}", WorldAge));
            }

            // 원자력 시대 이전이라면 다음 시대로 나아감
            if (WorldAge < Age.Atomic)
            {
                WorldAge++;
            }
            else
            {
                Debug.LogError(string.Format("현재 시대가 원자력 시대 이상임.\nWorldAge : {0}", WorldAge));
            }

            // TODO : 다음 시대로 넘어가면 새 건물 해금, 기존 건물 효과 업그레이드를 진행함.

            // 시대 텍스트 갱신
            UIManager.Instance.UpdateAgeText();
        }
    }
}