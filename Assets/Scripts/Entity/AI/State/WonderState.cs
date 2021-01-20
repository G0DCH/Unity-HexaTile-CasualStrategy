using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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

        // 무작위 설치할 수 있는 위치에 무작위 불가사의 설치.
        public override void Excute(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            HashSet<Tile> tiles = new HashSet<Tile>();

            foreach (var ownCity in enemy.ownCitys)
            {
                tiles.UnionWith(ownCity.RangeTiles);
            }

            List<Tile> tileList = new List<Tile>(tiles);

            enemy.StartCoroutine(PutRandomWonder(enemy, tileList));
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(WonderState));
        }

        private IEnumerator PutRandomWonder(EnemyAI enemy, List<Tile> tiles)
        {
            var wonderNames = DataTableManager.Instance.WonderNames;
            int wonderCount = wonderNames.Count;
            HashSet<string> ignoreWonder = new HashSet<string>();

            string randomWonderName;
            
            Tile randomTile;

            while (true)
            {
                List<Tile> checkList = new List<Tile>(tiles);

                if (ignoreWonder.Count >= wonderCount)
                {
                    Debug.LogError("설치할 수 있는 불가사의가 없습니다.\n 준비 단계로 돌아갑니다");
                    enemy.MyState = Ready.Instance;
                    enemy.IsExcuteState = false;
                    yield break;
                }
                while (true)
                {
                    int randomIndex = Random.Range(0, wonderCount);

                    randomWonderName = wonderNames[randomIndex];

                    if (!ignoreWonder.Contains(randomWonderName))
                    {
                        break;
                    }
                }

                while (true)
                {
                    randomTile = TileManager.Instance.GetRandomEmptyTile(checkList);
                    checkList.Remove(randomTile);

                    // 설치 성공 시 함수 종료
                    if (TileManager.Instance.PutWonderAtTile(randomWonderName, randomTile))
                    {
                        enemy.MyState = Idle.Instance;
                        enemy.IsExcuteState = false;
                        yield break;
                    }
                    else if (enemy.IsLackPoint)
                    {
                        Debug.LogError(string.Format("{0}을 짓기에 포인트가 부족합니다.\n현재 포인트 : {1}", randomWonderName, enemy.BuildPoint));
                        enemy.IsLackPoint = false;
                        ignoreWonder.Add(randomWonderName);
                        break;
                    }
                    // 실패시 설치하지 않을 불가사의에 무작위 불가사의 이름
                    // 다시 루프를 돔.
                    else if (checkList.Count == 0)
                    {
                        ignoreWonder.Add(randomWonderName);
                        break;
                    }
                    yield return null;
                }

                yield return null;
            }
        }
    }
}