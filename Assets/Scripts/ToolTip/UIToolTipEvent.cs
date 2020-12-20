using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TilePuzzle
{
    public class UIToolTipEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private bool isEmpty = true;
        private TileBuilding tileBuilding = TileBuilding.Empty;
        private TileBuilding TileBuilding
        {
            get
            {
                if (isEmpty)
                {
                    string buildingName = gameObject.name.Replace("Button", "");

                    // 건물 이라면
                    if(System.Enum.TryParse(buildingName, out TileBuilding result))
                    {
                        tileBuilding = result;
                        isEmpty = false;
                    }
                    // 불가사의 라면
                    else
                    {
                        tileBuilding = TileBuilding.Wonder;
                        wonderName = buildingName;
                        isEmpty = false;
                    }
                }

                return tileBuilding;
            }
        }

        private string wonderName = string.Empty;

        // 마우스 포인터가 UI 위로 올라오면 호출
        public void OnPointerEnter(PointerEventData eventData)
        {
            string tooltip;
            if (TileBuilding == TileBuilding.Wonder)
            {
                tooltip = DataTableManager.Instance.GetWonderToolTip(AgeManager.Instance.WorldAge, wonderName);
            }
            else
            {
                tooltip = DataTableManager.Instance.GetBuildingToolTip(AgeManager.Instance.WorldAge, TileBuilding);
            }

            if (tooltip != string.Empty)
            {
                UIManager.Instance.SetToolTipText(tooltip);
                UIManager.Instance.ShowToolTip(true, transform.position);
            }
        }

        // 마우스 포인터가 UI 바깥으로 나가면 호출
        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.ShowToolTip(false, transform.position);
        }
    }
}