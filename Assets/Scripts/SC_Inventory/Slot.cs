using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image m_ItemImage; //이미지
    [SerializeField] private Text QuantityText; //갯수
    [SerializeField] private ToolTip tooltip; //툴팁
    private DraggedObject dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트
    private int m_SlotNumber; //슬롯번호    
    private bool IsData; //데이터가있는가의 여부
    private string m_ToolTipText; //툴팁의텍스트

    public void SetDefaultSetting(int NewSlotNumber) //슬롯생성시에 슬롯자체의 고유번호 셋팅
    {
        m_SlotNumber = NewSlotNumber;
        IsData = false;
        m_ItemImage.enabled = false;
        tooltip = UIManager.Instance.GetToolTipBox;
        dragobject = UIManager.Instance.GetDraggedObject;
    }
  
    public void SetSlotData(string NewTooltipData) //슬롯넘버를 기반으로 슬롯데이터셋팅(처음, 갱신등에 사용)
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
                switch (ItemData.ItemType) //아이템의 종류에따라 갯수표기여부와 장비아이템이 아니면 갯수셋팅
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

    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시 그리고 드래그중인 오브젝트가 들어왔을떄 정보전달
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

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (IsData)
        {
            tooltip.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        if (IsData)
        {
            var ItemUniqueNumber = Player.instance.GetSlotItemData(m_SlotNumber).ItemNumber;

            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left: //마우스왼쪽클릭(아이템이동(드래그생성)), 상점UI오픈시엔 판매버튼생성
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
                case PointerEventData.InputButton.Right: //마우스오른쪽 클릭(아이템 사용/장착)
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

    public void OnPointerUp(PointerEventData eventData) //드래그 드롭중 드롭부분
    {
        if (dragobject.IsDrag)
        {
            dragobject.EndDrag();
        }
    }
}
