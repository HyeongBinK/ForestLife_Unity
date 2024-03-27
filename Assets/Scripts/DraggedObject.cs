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
    public DRAGGEDOBJECTTYPE m_starttype { get; private set; } //�巡�׵� ������Ʈ�� ��ġ(�κ��丮, ��ų, �� ���� �� ��� ���ۉ����)
    public DRAGGEDOBJECTTYPE m_endtype { get; private set; } //�巡�׵� ������Ʈ�� ���� ��ġ
    [SerializeField] private Image m_image; //�巡�׵Ǵ� ��ü�� �̹���
    public int m_StartSlotNumber { get; private set; } //�巡�׵Ǵ� ��ü�� ��ȣ(�κ��丮�� ��� ���Թ�ȣ, ��ų�� ��� ��ų������ȣ)
    public int m_EndSlotNumber { get; private set; } //�巡�װ� ������ ��ü�� ��ȣ(�κ��丮�� ��� ���Թ�ȣ, ��ų�� ��� ��ų������ȣ)
    public string m_ToolTipText { get; private set; } //���������� �������Կ��� �޾ƿͼ� �������ִٰ� �ű��� �뤊�� �ѱ��
    public bool IsDrag { get; private set; } //�巡�����ΰ�
   
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
                        default: //�׿��ϋ� �ƹ� �̺�Ʈ���Բ�
                            return;
                    }
                }
                break;

            default: //�׿� ����ó��
                return;
        }

        IsDrag = false;
        gameObject.SetActive(false);
    }

}
