using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle.Entities.AI
{
    public class Ready : State
    {
        public static State Instance { get; } = new Ready();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(Ready));
            enemy.MyActionState = ActionState.Ready;
        }

        public override void Excute(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            // TODO : City, Building, Wonder, Trade 상태 중 하나에 도달해야함.

            // 건물을 건설 할 수 없다면
            // 도시 설치
            if (!CanBuildBuilding(enemy.ownCitys))
            {
                SetEnemyState(enemy, ActionState.City);

                return;
            }

            // 도시, 건물, 불가사의 건설 중 하나를 무작위로 수행
            // TODO : Trade도 포함해서 구현
            ActionState randomState = (ActionState)Random.Range((int)ActionState.City, (int)ActionState.Trade);

            SetEnemyState(enemy, randomState);
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(Ready));
        }

        // enemy의 상태를 주어진 state로 변경
        private void SetEnemyState(EnemyAI enemy, ActionState state)
        {
            enemy.MyActionState = state;

            switch(state)
            {
                case ActionState.City:
                    enemy.MyState = CityState.Instance;
                    break;
                // TODO : 다른 State 스크립트 작성해서 구현하기.
                case ActionState.Building:
                    break;
                case ActionState.Wonder:
                    break;
                case ActionState.Trade:
                    break;
                default:
                    Debug.LogError(string.Format("올바르지 않은 State인 {0}가 입력됐습니다.", state));
                    break;
            }
        }

        private bool CanBuildBuilding(List<CityTile> ownCitys)
        {
            bool canBuildBuilding = true;

            foreach (var ownCity in ownCitys)
            {
                canBuildBuilding = ownCity.IsAllBuild && canBuildBuilding;
            }

            canBuildBuilding = canBuildBuilding && ownCitys.Count != 0;

            return canBuildBuilding;
        }
    }
}