namespace TilePuzzle.Entities.AI
{
    public class Trade : State
    {
        public static State Instance { get; } = new Trade();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(Trade));
            enemy.MyActionState = ActionState.Trade;
        }

        // 도시가 없다면 무작위 위치에 설치
        // 있다면 기존 도시와 붙여서 설치
        public override void Excute(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(Trade));
        }
    }
}