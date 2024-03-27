using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DRAGGEDOBJECTTYPE
{
    START,
    NONE = 0,
    INVENTORYSLOT,
    SKILLUISLOT,
    QUICKSLOT,
    END
}

public class DraggedObject : MonoBehaviour
{
    public DRAGGEDOBJECTTYPE m_starttype { get; private set; } //드래그된 오브젝트의 위치(인벤토리, 스킬, 퀵 슬롯 중 어디서 시작됬는지)
    public DRAGGEDOBJECTTYPE m_endtype { get; private set; } //드래그된 오브젝트가 끝난 위치
    [SerializeField] private Image m_image; //드래그되는 물체의 이미지
    public int m_StartSlotNumber { get; private set; } //드래그되는 물체의 번호(인벤토리의 경우 슬롯번호, 스킬의 경우 스킬고유번호)
    public int m_EndSlotNumber { get; private set; } //드래그가 끝나는 물체의 번호(인벤토리의 경우 슬롯번호, 스킬의 경우 스킬고유번호)
    public string m_ToolTipText { get; private set; } //툴팁정보를 기존슬롯에서 받아와서 가지고있다가 옮길대상에 통쨰로 넘긴다
    public bool IsDrag { get; private set; } //드래그중인가
   
    public void SetScreenPostion(float ScreenX, float ScreenY)
    {
        gameObject.transform.position = new Vector2(ScreenX, ScreenY);
    }

    public void SetStartSlotData(DRAGGEDOBJECTTYPE type, int SlotNumber, Sprite image, string ToolTipText)
    { 
        gameObject.SetActive(true);
        m_starttype = type;
        m_StartSlotNumber = SlotNumber;
        m_image.sprite = image;
        m_ToolTipText = ToolTipText;
        IsDrag = true;
        m_endtype = type;
        m_EndSlotNumber = SlotNumber;
        StartCoroutine(DragObject());
    }

    public void SetDraggingSlotData(DRAGGEDOBJECTTYPE type, int SlotNumber)
    {
        m_endtype = type;
        m_EndSlotNumber = SlotNumber;
    }

/*    public void OffDrag()
    {
        IsDrag = false;
        gameObject.SetActive(false);
    }*/

    IEnumerator DragObject()
    {
        while(IsDrag)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
                gameObject.transform.position = Camera.main.WorldToScreenPoint(raycastHit.point);
  
            yield return null;
        }
        yield return null;
    }

    public void EndDrag()
    {
        switch (m_starttype)
        {
            case DRAGGEDOBJECTTYPE.INVENTORYSLOT:
                {
                    switch (m_endtype)
                    {
                        case DRAGGEDOBJECTTYPE.NONE:
                            {
                                Player.instance.ThrowAwayInventoryItem(m_StartSlotNumber, 1);
                            }
                            break;
                        case DRAGGEDOBJECTTYPE.INVENTORYSLOT:
                            {
                                Player.instance.ChangeInventoryData(m_StartSlotNumber, m_EndSlotNumber);
                            }
                            break;
                        case DRAGGEDOBJECTTYPE.QUICKSLOT:
                            {
                                var SameItemOnQuickSlotNumber = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(m_StartSlotNumber, m_starttype);
                                if (SameItemOnQuickSlotNumber != -1)
                                {
                                    UIManager.Instance.GetQuickSlotsData.GetQuickSlotData(SameItemOnQuickSlotNumber).Clear();
                                }
                                if(DataTableManager.instance.GetItemData(Player.instance.GetSlotItemData(m_StartSlotNumber).ItemNumber).ItemType != ITEM_TYPE.EQUIPMENT)
                                UIManager.Instance.GetQuickSlotsData.SetQuickSlotData(m_EndSlotNumber, m_starttype, m_StartSlotNumber);
                            }
                            break;
                        default:
                            return;
                    }

                }
                break;
            case DRAGGEDOBJECTTYPE.SKILLUISLOT :
                {
                    if(m_endtype == DRAGGEDOBJECTTYPE.QUICKSLOT)
                    {
                        var SameItemOnQuickSlotNumber = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(m_StartSlotNumber, m_starttype);
                        if (SameItemOnQuickSlotNumber != -1)
                        {
                            UIManager.Instance.GetQuickSlotsData.GetQuickSlotData(SameItemOnQuickSlotNumber).Clear();
                        }
                        UIManager.Instance.GetQuickSlotsData.SetQuickSlotData(m_EndSlotNumber, m_starttype, m_StartSlotNumber);
                    }
                }
                break;
            case DRAGGEDOBJECTTYPE.QUICKSLOT:
                {
                    switch (m_endtype)
                    {
                        case DRAGGEDOBJECTTYPE.NONE:
                            {
                                UIManager.Instance.GetQuickSlotsData.ClearQuickSlot(m_StartSlotNumber);
                            }
                            break;
                        case DRAGGEDOBJECTTYPE.QUICKSLOT:
                            {
                                UIManager.Instance.GetQuickSlotsData.SwitchQuickSlotData(m_StartSlotNumber, m_EndSlotNumber);
                            }
                            break;
                        default: //그외일떈 아무 이벤트없게끔
                            return;
                    }
                }
                break;

            default: //그외 예외처리
                return;
        }

        IsDrag = false;
        gameObject.SetActive(false);
    }

}
