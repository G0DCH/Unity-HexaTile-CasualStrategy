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
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(Ready));
        }
    }
}