using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TilePuzzle.Entities.AI
{
    // 1. 도시 타일을 설치하는 경우
    // 1-1. 설치할 수 있는 건물이 없다면 반드시 도시 타일을 설치한다.
    // 1-2. 도시 타일의 설치 위치는 기존 도시와 최대한 가깝게 설치다.
    // 1-3. 만약 기존 도시가 없다면 무작위 위치에 설치한다.
    // 2. 건물 타일을 설치하는 경우
    // 2-1. 모든 위치에 모든 건물을 설치해보고
    //      가장 높은 점수를 획득할 수 있는 곳에 그 건물을 설치한다.
    // 2-2. 일정 점수 이상 획득할 수 없다면 도시 또는 불가사의 타일을 설치한다.
    // 3. 불가사의 타일을 설치하는 경우
    // 3-1. 조건이 되는 위치 아무 곳에나 설치한다.
    public class EnemyAI : Entity
    {
        public State MyState
        {
            get { return myState; }
            set
            {
                myState.Exit(this);
                myState = value;
                myState.Enter(this);
            }
        }
        private State myState = new Idle();

        public State PrevBuild { get; set; } = new Idle();
        public ActionState MyActionState { get { return myActionState; } set { myActionState = value; } }
        [SerializeField, ReadOnly]
        private ActionState myActionState = ActionState.Idle;
        [ReadOnly]
        public bool IsExcuteState = false;
        /// <summary>
        /// 선택된 건물을 짓기에 포인트가 부족한가?
        /// </summary>
        [HideInInspector]
        public bool IsLackPoint { get; set; } = false;

        private Coroutine excuteState = null;

        [Button]
        // 위에 기술한 행동 중 하나를 수행한다.
        public void Action()
        {
            if (!IsMyTurn)
            {
                Debug.LogError("내 턴이 아님");
                return;
            }

            if (excuteState == null)
            {
                excuteState = StartCoroutine(ExcuteState());
            }
        }

        private IEnumerator ExcuteState()
        {
            do
            {
                if (!IsExcuteState)
                {
                    IsExcuteState = true;
                    MyState.Excute(this);
                }

                yield return null;
            }
            while (MyState != Idle.Instance);

            excuteState = null;
        }
    }
}