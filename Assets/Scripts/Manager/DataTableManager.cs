#pragma warning disable 0649

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace TilePuzzle
{
    public class DataTableManager : Utility.Singleton<DataTableManager>
    {
        [ShowInInspector]
        public List<string> WonderNames { get; } = new List<string>();
        [SerializeField]
        private BuildingDataTable buildingDataTable;
        [SerializeField]
        private List<WonderDataTable> wonderDataTables;

        // 영어 이름으로 wonderData를 갖고옴.
        private Dictionary<string, WonderData> WonderDataMap { get; } = new Dictionary<string, WonderData>();
        // 영어 이름으로 buildingData를 갖고옴.
        private Dictionary<TileBuilding, BuildingData> BuildingDataMap { get; } = new Dictionary<TileBuilding, BuildingData>();

        /// <summary>
        /// 현재 시대에 설치 가능한 건물들
        /// </summary>
        public HashSet<TileBuilding> TileBuildingsOnAge
        {
            get
            {
                if (age != AgeManager.Instance.WorldAge)
                {
                    age = AgeManager.Instance.WorldAge;
                    foreach (var buildingData in buildingDataTable.BuildingDatas)
                    {
                        if (tileBuildingsOnAge.Contains(buildingData.MyBuilding))
                        {
                            continue;
                        }

                        foreach (var info in buildingData.InfoPerAges)
                        {
                            if (info.MyAge > age)
                            {
                                break;
                            }
                            else
                            {
                                tileBuildingsOnAge.Add(buildingData.MyBuilding);
                            }
                        }
                    }
                }

                return tileBuildingsOnAge;
            }
        }
        private readonly HashSet<TileBuilding> tileBuildingsOnAge = new HashSet<TileBuilding>();

        private Age age = Age.Atomic;

        private void Start()
        {
            InitWonderNames();
        }

        /// <summary>
        /// 시대와 빌딩을 입력하면
        /// <para>해당 시대의 빌딩 데이터를 return함.</para>
        /// </summary>
        /// <param name="age"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        public InfoPerAge GetBuildingInfo(Age age, TileBuilding building)
        {
            if (BuildingDataMap.TryGetValue(building, out BuildingData data))
            {
                return GetBuildingInfo(age, data);
            }

            foreach (var buildingData in buildingDataTable.BuildingDatas)
            {
                if (buildingData.MyBuilding == building)
                {
                    BuildingDataMap.Add(building, buildingData);
                    return GetBuildingInfo(age, buildingData);
                }
            }

            //Debug.LogError(string.Format("빌딩이나 시대가 존재하지 않음. Building : {0}, Age : {1}", building, age));

            return InfoPerAge.EmptyInfo;
        }

        private InfoPerAge GetBuildingInfo(Age age, BuildingData buildingData)
        {
            foreach (var info in buildingData.InfoPerAges)
            {
                if (info.MyAge == age)
                {
                    return info;
                }
            }
            //Debug.LogError(string.Format("빌딩이나 시대가 존재하지 않음. Building : {0}, Age : {1}", building, age));

            return InfoPerAge.EmptyInfo;
        }

        /// <summary>
        /// 시대와 빌딩을 입력하면
        /// <para>해당 시대의 빌딩 툴팁을 return 함</para>
        /// </summary>
        /// <param name="age"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        public string GetBuildingToolTip(Age age, TileBuilding building)
        {
            bool isSuccess = BuildingDataMap.TryGetValue(building, out BuildingData data);

            if (isSuccess)
            {
                return GetBuildingToolTip(age, data);
            }
            else
            {
                foreach (var buildingData in buildingDataTable.BuildingDatas)
                {
                    if (buildingData.MyBuilding == building)
                    {
                        BuildingDataMap.Add(building, buildingData);
                        return GetBuildingToolTip(age, buildingData);
                    }
                }
            }

            Debug.LogError(string.Format("해당 시대에 건물 정보가 정의되어있지 않음. Building : {0}, Age : {1}", building, age));

            return string.Empty;
        }

        private string GetBuildingToolTip(Age age, BuildingData buildingData)
        {
            var building = buildingData.MyBuilding;
            string toolTip = string.Empty;

            if (building == TileBuilding.City ||
                building == TileBuilding.GovernmentPlaza ||
                building == TileBuilding.Aqueduct)
            {
                if (building == TileBuilding.City)
                {
                    toolTip = string.Format(buildingData.ToolTipText, TileManager.Instance.CityNum);
                }
                else if (building == TileBuilding.GovernmentPlaza)
                {
                    toolTip = string.Format(buildingData.ToolTipText, 3);
                }
                else
                {
                    if (AgeManager.Instance.WorldAge < Age.Classical)
                    {
                        return string.Empty;
                    }
                    toolTip = string.Format(buildingData.ToolTipText, 1);
                }

                return toolTip;
            }

            var info = GetBuildingInfo(age, buildingData);

            if (info != InfoPerAge.EmptyInfo)
            {
                List<object> toolTipArgs = new List<object> { info.Cost };

                if (!(building == TileBuilding.City ||
                    building == TileBuilding.GovernmentPlaza ||
                    building == TileBuilding.Aqueduct))
                {
                    toolTipArgs.Add(info.BaseBonus);

                    foreach (var bonusData in info.BonusPerBuildings)
                    {
                        if (bonusData.MyBuilding == TileBuilding.Empty)
                        {
                            toolTipArgs.Add("모든 건물");
                        }
                        else
                        {
                            toolTipArgs.Add(bonusData.MyBuilding);
                        }

                        if (bonusData.Bonus == 0.5)
                        {
                            toolTipArgs.Add(2);
                            toolTipArgs.Add(1);
                        }
                        else
                        {
                            toolTipArgs.Add(1);
                            toolTipArgs.Add(bonusData.Bonus);
                        }
                    }
                    foreach (var bonusData in info.BonusPerFeatures)
                    {
                        toolTipArgs.Add(bonusData.MyFeature);

                        if (bonusData.Bonus == 0.5)
                        {
                            toolTipArgs.Add(2);
                            toolTipArgs.Add(1);
                        }
                        else
                        {
                            toolTipArgs.Add(1);
                            toolTipArgs.Add(bonusData.Bonus);
                        }
                    }
                    foreach (var bonusData in info.BonusPerTypes)
                    {
                        toolTipArgs.Add(bonusData.MyType);
                        if (bonusData.Bonus == 0.5)
                        {
                            toolTipArgs.Add(2);
                            toolTipArgs.Add(1);
                        }
                        else
                        {
                            toolTipArgs.Add(1);
                            toolTipArgs.Add(bonusData.Bonus);
                        }
                    }
                }

                toolTip = string.Format(buildingData.ToolTipText, toolTipArgs.ToArray());
            }

            return toolTip;
        }

        // 해당 이름의 불가사의 툴팁을 return
        public string GetWonderToolTip(string wonderName)
        {
            if (wonderName == string.Empty)
            {
                Debug.LogError("빈 불가사의 이름이 들어왔음");
                return string.Empty;
            }

            Age age = AgeManager.Instance.WorldAge;

            var wonderData = GetWonderData(wonderName);

            if (wonderData != null)
            {
                int costMultiflier = Mathf.Clamp(age - wonderData.MyAge + 1, 1, int.MaxValue);
                string toolTip = string.Format(wonderData.ToolTipText, wonderData.Cost * costMultiflier, wonderData.Bonus);

                return toolTip;
            }

            Debug.LogError(string.Format("해당 시대에 불가사의 정보가 정의되어있지 않음. Wonder : {0}, Age : {1}", wonderName, age));

            return string.Empty;
        }

        // 해당 이름의 불가사의 데이터를 return
        public WonderData GetWonderData(string wonderName)
        {
            if (wonderName == string.Empty)
            {
                Debug.LogError("빈 불가사의 이름이 들어왔음");
                return null;
            }

            Age age = AgeManager.Instance.WorldAge;

            if (WonderDataMap.TryGetValue(wonderName, out WonderData data))
            {
                return data;
            }

            foreach (var wonderDataTable in wonderDataTables)
            {
                if (wonderDataTable.TableAge <= age)
                {
                    foreach (var wonderData in wonderDataTable.WonderDatas)
                    {
                        if (wonderData.WonderName == wonderName)
                        {
                            WonderDataMap.Add(wonderName, wonderData);
                            return wonderData;
                        }
                    }
                }
            }

            Debug.LogError(string.Format("해당 시대에 불가사의 정보가 정의되어있지 않음. Wonder : {0}, Age : {1}", wonderName, age));

            return null;
        }

        // 현재 시대의 것만 추가
        public void InitWonderNames()
        {
            foreach (var wonderDataTable in wonderDataTables)
            {
                if (wonderDataTable.TableAge != AgeManager.Instance.WorldAge)
                {
                    continue;
                }

                foreach (var wonderData in wonderDataTable.WonderDatas)
                {
                    if (!WonderNames.Contains(wonderData.WonderName))
                    {
                        WonderNames.Add(wonderData.WonderName);
                    }
                }

                break;
            }
        }
    }
}