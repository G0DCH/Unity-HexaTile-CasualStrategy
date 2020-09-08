using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TilePuzzle
{
    public class UIToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // 마우스 포인터가 UI 위로 올라오면 호출
        public void OnPointerEnter(PointerEventData eventData)
        {
            UIManager.Instance.ShowToolTip(true, transform.position);
        }

        // 마우스 포인터가 UI 바깥으로 나가면 호출
        public void OnPointerExit(PointerEventData eventData)
        {
            UIManager.Instance.ShowToolTip(false, transform.position);
        }
    }
}