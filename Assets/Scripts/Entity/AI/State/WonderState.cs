namespace TilePuzzle.Entities.AI
{
    public class WonderState : State
    {
        public static State Instance { get; } = new WonderState();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(WonderState));
            enemy.MyActionState = ActionState.Wonder;
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

            StateMessage(false, enemy.NickName, nameof(WonderState));
        }
    }
}