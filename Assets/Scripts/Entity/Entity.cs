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
        public bool IsMyTurn { get; set; } = true;
        public string NickName { get; set; } = "Default";
    }
}