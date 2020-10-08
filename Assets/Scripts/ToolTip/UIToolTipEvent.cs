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

                    tileBuilding = (TileBuilding)System.Enum.Parse(typeof(TileBuilding), buildingName);
                }

                return tileBuilding;
            }
        }

        // 마우스 포인터가 UI 위로 올라오면 호출
        public void OnPointerEnter(PointerEventData eventData)
        {
            string tooltip = DataTableManager.Instance.GetBuildingToolTip(AgeManager.Instance.WorldAge, TileBuilding);

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