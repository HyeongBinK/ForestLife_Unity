using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;
using System.IO;

public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image m_ItemImage; //이미지
    private ToolTip tooltip; //툴팁

    //데이터가 잘들어왔는지 확인하기위해 임시로 시리어라이즈필드 달았습니다.
    [SerializeField] private int m_SlotNumber; //슬롯고유번호(장비부위별로)
    public int GetSlotNumber { get { return m_SlotNumber; } }
    [SerializeField] private string m_ToolTipText; //툴팁텍스트
    [SerializeField] private bool IsData; //데이터가있는가의 여부
    public bool GetIsData { get { return IsData; } }
    private SlotEquipmentData SlotData; //해당슬롯에 담겨있는 장비아이템의 정보
    public SlotEquipmentData GetSlotData { get { return SlotData; } }
    bool isInit = false;


    public void DefaultSetting() //초기정보셋팅
    {
        IsData = false;
        m_ToolTipText = "데이터없음";
        SlotData = new SlotEquipmentData();
        tooltip = UIManager.Instance.GetToolTipBox;
        m_ItemImage.enabled = false;
        isInit = true;
    }

    public bool SetSlotData(SlotEquipmentData NewEquipment, string NewTooltipData) //슬롯에 들어온 데이터를 기반으로 슬롯데이터셋팅(만약 기존장비가있으면 변경하게끔)
    {
        if (!isInit) DefaultSetting();

        if (IsData)
        {
            if (!ReturnEquipmentToInventory())
                return false;
        }


        IsData = true;
        SlotData = NewEquipment;
        m_ToolTipText = NewTooltipData;
        m_ItemImage.enabled = true;
        var ItemData = DataTableManager.instance.GetItemData(SlotData.ItemNumber);
        m_ItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
        return true;
    }

    public void ClearSlotData() //슬롯에서 장비를 벗었을떄 초기화
    {
        if (tooltip.gameObject.activeSelf)
            tooltip.gameObject.SetActive(false);
        IsData = false;
        isInit = false;
        SlotData = new SlotEquipmentData();
        m_ToolTipText = "데이터없음";
        m_ItemImage.enabled = false;
        Player.instance.SetPlayerEquipmentStatus();
    }

    public bool ReturnEquipmentToInventory() //장비를 인벤토리로 되돌리는 기능
    {
        return Player.instance.ReturnEquipment(SlotData);
    }

    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시
    {
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

    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄 해당슬롯에 아이템이 있다면 인벤토리로
    {
        if (IsData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) //마우스 오른쪽,왼쪽클릭둘다 동일이벤트
            {
                if (!ReturnEquipmentToInventory()) //장비창에서 인벤토리로 아이템을 돌리는 작업이 실패하면 그대로 종료
                    return;

                ClearSlotData(); //슬롯정보 비우기
                Player.instance.SetPlayerEquipmentStatus(); //플레이어장비능력치합계 갱신
            }
        }
    }
}