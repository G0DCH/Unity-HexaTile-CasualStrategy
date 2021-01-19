using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle.Entities
{
    // 플레이어, AI의 기본 단위
    // 이 Entity가 소유한 타일들을 알고 있어야 함.
    public abstract class Entity : MonoBehaviour
    {
        public List<CityTile> ownCitys = new List<CityTile>();
        public bool IsMyTurn { get; set; } = false;
        public string NickName { get; set; } = "Default";

        public int Score { get { return score; } private set { score = value; } }
        [SerializeField]
        private int score = 0;
        public int BuildPoint { get { return buildPoint; } private set { buildPoint = value; } }
        [SerializeField]
        private int buildPoint = 6;

        /// <summary>
        /// 현재 시대
        /// </summary>
        public Age WorldAge { get; set; } = Age.Ancient;

        public void UpdatePoint(int point)
        {
            // 타일 배치로 인한 포인트 감소
            BuildPoint -= TileManager.Instance.SelectTileCost;

            Score += point;
            BuildPoint += point;
        }
    }
}