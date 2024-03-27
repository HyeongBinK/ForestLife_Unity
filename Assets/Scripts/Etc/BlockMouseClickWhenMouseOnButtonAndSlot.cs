using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMouseClickWhenMouseOnButtonAndSlot : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Instance.IsMouseOnUI = true;
    }
}

