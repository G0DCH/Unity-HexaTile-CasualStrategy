using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace TilePuzzle
{
    public class DataTableManager : Utility.Singleton<DataTableManager>
    {
        [SerializeField]
        private BuildingDataTable buildingDataTable;
        [SerializeField]
        private List<WonderDataTable> wonderDataTables;

        /// <summary>
        /// 시대와 빌딩을 입력하면
        /// <para>해당 시대의 빌딩 데이터를 return함.</para>
        /// </summary>
        /// <param name="age"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        public InfoPerAge GetBuildingData(Age age, TileBuilding building)
        {
            // 나중에 Dictionary로 구현하기
            foreach (var buildingData in buildingDataTable.BuildingDatas)
            {
                if (buildingData.MyBuilding == building)
                {
                    foreach (var info in buildingData.InfoPerAges)
                    {
                        if (info.MyAge == age)
                        {
                            return info;
                        }
                    }
                }
            }

            Debug.LogError(string.Format("빌딩이나 시대가 존재하지 않음. Building : {0}, Age : {1}", building, age));

            return null;
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
            foreach (var buildingData in buildingDataTable.BuildingDatas)
            {
                if (buildingData.MyBuilding == building)
                {
                    if (building == TileBuilding.City ||
                        building == TileBuilding.GovernmentPlaza ||
                        building == TileBuilding.Aqueduct)
                    {
                        string toolTip = string.Empty;
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

                    foreach (var info in buildingData.InfoPerAges)
                    {
                        if (info.MyAge == age)
                        {
                            string toolTip = string.Empty;
                            List<object> toolTipArgs = new List<object>();
                            toolTipArgs.Add(info.Cost);

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

                            return toolTip;
                        }
                    }
                }
            }

            Debug.LogError(string.Format("해당 시대에 건물 정보가 정의되어있지 않음. Building : {0}, Age : {1}", building, age));

            return string.Empty;
        }

        public string GetWonderToolTip(Age age, string wonderName)
        {
            if (wonderName == string.Empty)
            {
                Debug.LogError("빈 불가사의 이름이 들어왔음");
                return string.Empty;
            }

            foreach(var wonderDataTable in wonderDataTables)
            {
                if(wonderDataTable.TableAge <= age)
                {
                    foreach (var wonderData in wonderDataTable.WonderDatas)
                    {                        
                        if (wonderData.WonderName == wonderName)
                        {
                            int costMultiflier = Mathf.Clamp(age - wonderData.MyAge + 1, 1, int.MaxValue);

                            string toolTip = string.Format(wonderData.ToolTipText, wonderData.Cost * costMultiflier, wonderData.Bonus);

                            return toolTip;
                        }
                    }
                }
            }

            Debug.LogError(string.Format("해당 시대에 불가사의 정보가 정의되어있지 않음. Wonder : {0}, Age : {1}", wonderName, age));

            return string.Empty;
        }
    }
}