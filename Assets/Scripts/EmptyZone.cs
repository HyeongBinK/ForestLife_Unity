using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyZone : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private DraggedObject m_DraggedObject;

    public void OnPointerEnter(PointerEventData eventData) 
    {
        UIManager.Instance.IsMouseOnUI = false;
        if (m_DraggedObject.IsDrag)
        {
            m_DraggedObject.SetDraggingSlotData(DRAGGEDOBJECTTYPE.NONE, -1);
        }
    }
}
