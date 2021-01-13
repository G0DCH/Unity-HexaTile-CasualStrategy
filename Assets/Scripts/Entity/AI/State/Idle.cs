using UnityEngine;

namespace TilePuzzle.Entities.AI
{
    public class Idle : State
    {
        public static State Instance { get; } = new Idle();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(Idle));
            enemy.MyActionState = ActionState.Idle;
        }

        public override void Excute(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            enemy.MyState = Ready.Instance;
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(Idle));
        }
    }
}