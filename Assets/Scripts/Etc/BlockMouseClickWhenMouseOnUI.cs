using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMouseClickWhenMouseOnUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.IsMouseOnUI = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.IsMouseOnUI = false;
    }
}
