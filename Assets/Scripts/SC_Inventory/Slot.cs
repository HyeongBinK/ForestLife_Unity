using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image m_ItemImage; //�̹���
    [SerializeField] private Text QuantityText; //����
    [SerializeField] private ToolTip tooltip; //����
    private DraggedObject dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ
    private int m_SlotNumber; //���Թ�ȣ    
    private bool IsData; //�����Ͱ��ִ°��� ����
    private string m_ToolTipText; //�������ؽ�Ʈ

    public void SetDefaultSetting(int NewSlotNumber) //���Ի����ÿ� ������ü�� ������ȣ ����
    {
        m_SlotNumber = NewSlotNumber;
        IsData = false;
        m_ItemImage.enabled = false;
        tooltip = UIManager.Instance.GetToolTipBox;
        dragobject = UIManager.Instance.GetDraggedObject;
    }
  
    public void SetSlotData(string NewTooltipData) //���Գѹ��� ������� ���Ե����ͼ���(ó��, ���ŵ ���)
    {
        if (Player.instance.GetSlotIsData(m_SlotNumber))
        {
            var SlotData = Player.instance.GetSlotItemData(m_SlotNumber);
            IsData = true;
            int ItemNumber = SlotData.ItemNumber;

            if (DataTableManager.instance.GetItemData(ItemNumber) != null)
            {
                var ItemData = DataTableManager.instance.GetItemData(ItemNumber);
                m_ItemImage.enabled = true;
                m_ItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
                switch (ItemData.ItemType) //�������� ���������� ����ǥ�⿩�ο� ���������� �ƴϸ� ��������
                {
                    case ITEM_TYPE.EQUIPMENT:
                        {
                            QuantityText.gameObject.SetActive(false);
                        }
                        break;
                    default:
                        {
                            if (!QuantityText.gameObject.activeSelf)
                                QuantityText.gameObject.SetActive(true);
                            QuantityText.text = SlotData.Quantity.ToString();
                        }
                        break;
                }
                m_ToolTipText = NewTooltipData;
                tooltip.SetToolTipText(m_ToolTipText);
            }
        }
        else
            ClearSlotData();
    }

    public void ClearSlotData()
    {
        if (tooltip.gameObject.activeSelf)
            tooltip.gameObject.SetActive(false);
        m_ItemImage.enabled = false;
        QuantityText.text = "0";
        QuantityText.gameObject.SetActive(false); ;
        IsData = false;
        m_ToolTipText = "";
        tooltip.SetToolTipText(m_ToolTipText);
    }

    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ�� �׸��� �巡������ ������Ʈ�� �������� ��������
    {
        if (dragobject.IsDrag)
            dragobject.SetDraggingSlotData(DRAGGEDOBJECTTYPE.INVENTORYSLOT, m_SlotNumber);
        if (IsData)
        {
            var Position = gameObject.transform.position;
            tooltip.SetToolTipPosition(Position.x, Position.y);
            tooltip.SetToolTipText(m_ToolTipText);
            tooltip.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        if (IsData)
        {
            tooltip.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        if (IsData)
        {
            var ItemUniqueNumber = Player.instance.GetSlotItemData(m_SlotNumber).ItemNumber;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left: //���콺����Ŭ��(�������̵�(�巡�׻���)), ����UI���½ÿ� �ǸŹ�ư����
                    if(Player.instance.m_IsStoreUIActive)
                    {
                        var SellUI = UIManager.Instance.GetSellButton;
                        var Position = gameObject.transform.position;
                        SellUI.gameObject.SetActive(true);
                        SellUI.SetOriginSlotNumber(m_SlotNumber, DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType, Player.instance.GetSlotItemData(m_SlotNumber).Quantity);
                        SellUI.SetScreenPosition(Position.x, Position.y);
                        return;
                    }
                    dragobject.SetStartSlotData(DRAGGEDOBJECTTYPE.INVENTORYSLOT, m_SlotNumber, m_ItemImage.sprite, m_ToolTipText);
                    break;
                case PointerEventData.InputButton.Right: //���콺������ Ŭ��(������ ���/����)
                    {
                        switch (DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType)
                        {
                            case ITEM_TYPE.EQUIPMENT:
                                Player.instance.EquipItem(m_SlotNumber, m_ToolTipText);
                                break;
                            case ITEM_TYPE.CONSUMPTION:
                                Player.instance.UseConsumptionItem(m_SlotNumber, null);
                                break;
                            case ITEM_TYPE.SCROLL:
                                Player.instance.UseConsumptionItem(m_SlotNumber, null);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData) //�巡�� ����� ��Ӻκ�
    {
        if (dragobject.IsDrag)
        {
            dragobject.EndDrag();
        }
    }
}
