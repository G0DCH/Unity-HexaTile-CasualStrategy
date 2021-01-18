using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    [System.Serializable]
    public class WonderData
    {
        public Age MyAge { get { return myAge; } }
        [SerializeField]
        private Age myAge = Age.Ancient;

        /// <summary>
        /// 불가사의 영문 명
        /// </summary>
        public string WonderName { get { return wonderName; } }
        [SerializeField]
        private string wonderName = string.Empty;

        /// <summary>
        /// 불가사의 한글 명
        /// </summary>
        public string KoreanName { get { return koreanName; } }
        [SerializeField]
        private string koreanName = string.Empty;
                
        public string ToolTipText { get { return toolTipText; } }
        [SerializeField, TextArea, Space]
        private string toolTipText = string.Empty;

        /// <summary>
        /// 불가사의 설치 비용
        /// </summary>
        public int Cost { get { return cost; } }
        [SerializeField, Space]
        private int cost = 10;

        /// <summary>
        /// 불가사의의 지속 보너스
        /// </summary>
        public int Bonus { get { return bonus; } }
        [SerializeField]
        private int bonus = 2;

        public GameObject MyPrefab { get { return myPrefab; } }
        [SerializeField]
        private GameObject myPrefab = null;

        public WonderData(Age age, string wonderName, int cost, int bonus)
        {
            myAge = age;
            this.wonderName = wonderName;
            this.cost = cost;
            this.bonus = bonus;
        }
    }
}