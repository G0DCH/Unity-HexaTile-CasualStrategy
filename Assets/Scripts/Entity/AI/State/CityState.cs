namespace TilePuzzle.Entities.AI
{
    public class CityState : State
    {
        public static State Instance { get; } = new CityState();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(CityState));
            enemy.MyActionState = ActionState.City;
        }

        // 도시가 없다면 무작위 위치에 설치
        // 있다면 기존 도시와 붙여서 설치
        public override void Excute(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            // 도시가 없다면 무작위 위치에 설치
            if (enemy.ownCitys.Count == 0)
            {
                Tile randomTile;

                while(true)
                {
                    randomTile = TileManager.Instance.GetRandomEmptyTile();

                    if (randomTile.OwnerCity == null)
                    {
                        break;
                    }
                }

                TileManager.Instance.PutBuildingAtTile(TileBuilding.City, randomTile);
            }

            enemy.MyState = Idle.Instance;
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(CityState));
        }
    }
}