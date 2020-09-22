using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    /// <summary>
    /// 주변 건물 당 보너스
    /// </summary>
    [System.Serializable]
    public class BonusPerBuilding
    {
        /// <summary>
        /// Empty인 경우 모든 건물이라는 뜻임.
        /// </summary>
        public TileBuilding MyBuilding { get { return myBuilding; } }
        [SerializeField]
        private TileBuilding myBuilding = TileBuilding.Empty;

        public float Bonus { get { return bonus; } }
        [SerializeField]
        private float bonus = 0;
    }

    /// <summary>
    /// 주변 지형 당 보너스
    /// </summary>
    [System.Serializable]
    public class BonusPerFeature
    {
        public TileFeature MyFeature { get { return myFeature; } }
        [SerializeField]
        private TileFeature myFeature = TileFeature.Empty;

        public float Bonus { get { return bonus; } }
        [SerializeField]
        private float bonus = 0;
    }

    /// <summary>
    /// 한 시대 당 보너스
    /// </summary>
    [System.Serializable]
    public class BonusPerAge
    {
        public Age MyAge { get { return myAge; } }
        [SerializeField]
        private Age myAge = Age.Ancient;

        /// <summary>
        /// 건물 건설 시 지급되는 기본 보너스
        /// </summary>
        public int BaseBonus { get { return baseBonus; } }
        [SerializeField]
        private int baseBonus = 0;

        /// <summary>
        /// 주변 건물 당 보너스, 건물이 Empty인 경우 모든 건물이라는 뜻임.
        /// </summary>
        public List<BonusPerBuilding> BonusPerBuildings { get { return bonusPerBuildings; } }
        [SerializeField]
        private List<BonusPerBuilding> bonusPerBuildings = new List<BonusPerBuilding>();

        /// <summary>
        /// 주변 지형 당 보너스
        /// </summary>
        public List<BonusPerFeature> BonusPerFeatures { get { return bonusPerFeatures; } }
        [SerializeField]
        private List<BonusPerFeature> bonusPerFeatures = new List<BonusPerFeature>();
    }
}