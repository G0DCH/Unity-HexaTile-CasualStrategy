using UnityEngine;

namespace TilePuzzle.Entities.AI
{
    public abstract class State
    {
        // isEnter가 true면 Enter 메시지
        // false면 Exit 메시지
        protected static void StateMessage(bool isEnter, string name, string state)
        {
            if (isEnter)
            {
                Debug.Log(string.Format("{0}이 {1} 상태에 도달했습니다.", name, state));
            }
            else
            {
                Debug.Log(string.Format("{0}이 {1} 상태를 벗어났습니다.", name, state));
            }
        }

        protected static bool CheckEnemyTurn(EnemyAI enemy)
        {
            bool isRightEnemy = true;

            if (enemy == null)
            {
                Debug.Log(string.Format("{0}가 null 입니다.", nameof(enemy)));
                isRightEnemy = false;
            }
            else if (GameManager.Instance.TurnEntity == enemy)
            {
                Debug.Log(string.Format("{0}의 턴이 아닙니다.", enemy.NickName));
                isRightEnemy = false;
            }

            return isRightEnemy;
        }

        public abstract void Enter(EnemyAI enemy);
        public abstract void Excute(EnemyAI enemy);
        public abstract void Exit(EnemyAI enemy);
    }
}