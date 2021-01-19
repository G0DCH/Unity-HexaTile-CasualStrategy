using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle.Entities.AI
{
    public class BuildingState : State
    {
        public static State Instance { get; } = new BuildingState();

        public override void Enter(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(true, enemy.NickName, nameof(BuildingState));
            enemy.MyActionState = ActionState.Building;
        }

        // 도시 범위 내 무작위 위치에 건물 설치
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

            enemy.StartCoroutine(PutRandomBuildingTile(enemy, tileList));

            //// 실패시 도시 상태로 전환해서
            //// 도시 설치
            //if (PutRandomBuildingTile(enemy, tileList))
            //{
            //    enemy.MyState = Idle.Instance;
            //}
            //else
            //{
            //    enemy.MyState = CityState.Instance;
            //}
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(BuildingState));
        }

        // 무작위 건물을 무작위 타일에 설치
        private IEnumerator PutRandomBuildingTile(EnemyAI enemy, List<Tile> tiles)
        {
            int enumCount = System.Enum.GetValues(typeof(TileBuilding)).Length;

            // 설치하지 않을 건물들
            HashSet<TileBuilding> ignoreBuildings = new HashSet<TileBuilding>
                                { TileBuilding.City, TileBuilding.Empty, TileBuilding.Carnal, TileBuilding.Wonder };
            // 현재 시대에 설치할 수 있는 건물들
            HashSet<TileBuilding> tileBuildingsOnAge = DataTableManager.Instance.TileBuildingsOnAge;
            TileBuilding randomBuilding;

            while (true)
            {
                List<Tile> checkList = new List<Tile>(tiles);

                if (ignoreBuildings.Count >= enumCount)
                {
                    Debug.LogError("설치할 수 있는 건물이 없습니다.\n 대신 도시를 설치합니다");

                    enemy.MyState = CityState.Instance;
                    enemy.IsExcuteState = false;
                    yield break;
                }

                // 무작위 빌딩 뽑기
                while (true)
                {
                    randomBuilding = (TileBuilding)Random.Range(0, enumCount);

                    // 현재 시대에 설치할 수 없는 건물이라면 무시할 건물에 추가.
                    if (!tileBuildingsOnAge.Contains(randomBuilding))
                    {
                        ignoreBuildings.Add(randomBuilding);
                        continue;
                    }

                    if (!ignoreBuildings.Contains(randomBuilding))
                    {
                        break;
                    }
                }

                while (true)
                {
                    var randomTile = TileManager.Instance.GetRandomEmptyTile(checkList);
                    checkList.Remove(randomTile);

                    // 설치 성공 시 함수 종료
                    if (TileManager.Instance.PutBuildingAtTile(randomBuilding, randomTile))
                    {
                        enemy.MyState = Idle.Instance;
                        enemy.IsExcuteState = false;
                        yield break;
                    }
                    // 실패시 설치하지 않을 건물에 무작위 건물 추가하고
                    // 다시 루프를 돔.
                    else if (checkList.Count == 0)
                    {
                        ignoreBuildings.Add(randomBuilding);
                        break;
                    }
                    yield return null;
                }

                yield return null;
            }
        }
    }
}