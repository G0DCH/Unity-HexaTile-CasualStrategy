using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
                enemy.StartCoroutine(PutCityTile(enemy));
            }
            // 도시가 있다면 내 도시와 가까운 무작위 위치에 설치
            else
            {
                // 무시할 타일들
                HashSet<Tile> ignoreTiles = new HashSet<Tile>();
                // 무시하지 않을 타일들
                HashSet<Tile> tiles = new HashSet<Tile>();

                // 타일 셋 초기화
                foreach (var ownCity in enemy.ownCitys)
                {
                    ignoreTiles.UnionWith(ownCity.RangeTiles);
                    tiles.UnionWith(TileManager.Instance.GetRangeTiles(ownCity, ownCity.Range + 1));
                }

                // 무시할 타일 제거
                tiles.ExceptWith(ignoreTiles);

                var tileList = new List<Tile>(tiles);

                enemy.StartCoroutine(PutCityTile(enemy, tileList));
            }
        }

        public override void Exit(EnemyAI enemy)
        {
            if (!CheckEnemyTurn(enemy))
            {
                return;
            }

            StateMessage(false, enemy.NickName, nameof(CityState));
        }

        // 무작위 위치에 도시 타일을 설치함.
        // 만약 tiles가 null이 아닌 경우
        // tiles 내의 타일 중 하나에 설치함.
        private IEnumerator PutCityTile(EnemyAI enemy, List<Tile> tiles = null)
        {
            Tile randomTile;

            HashSet<Tile> ignoreTiles = new HashSet<Tile>();

            while (true)
            {
                while (true)
                {
                    if (tiles == null)
                    {
                        randomTile = TileManager.Instance.GetRandomEmptyTile();
                    }
                    else
                    {
                        if (ignoreTiles.Count >= tiles.Count)
                        {
                            Debug.LogError("도시 설치 실패. \n Ready 상태로 변경합니다.");
                            enemy.MyState = Ready.Instance;
                            enemy.IsExcuteState = false;
                            yield break;
                        }

                        randomTile = TileManager.Instance.GetRandomEmptyTile(tiles);
                        ignoreTiles.Add(randomTile);
                    }

                    if (randomTile.OwnerCity == null)
                    {
                        break;
                    }
                }

                // 설치 성공 시 루프 탈출
                if (TileManager.Instance.PutBuildingAtTile(TileBuilding.City, randomTile))
                {
                    break;
                }

                yield return null;
            }

            enemy.MyState = Idle.Instance;
            enemy.IsExcuteState = false;
            yield break;
        }
    }
}