using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class QuickSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber; //퀵슬롯고유번호 앞에서부터 0~
    public int GetQuickSlotUniqueNumber { get { return m_SlotNumber; } }
    [SerializeField] private Image m_QuickSlotImage; //이미지
    public Sprite GetQuickSlotImage { get { return m_QuickSlotImage.sprite; } }
    [SerializeField] private Text QuantityText; //갯수(인벤토리슬롯에서온 아이템데이터의 경우존재 그외엔 off)
    [SerializeField] private Text KeyText; //어느키를눌렀을때 사용되는 키슬롯인지 표시
    [SerializeField] public string Key; //해당키
    private ToolTip m_ToolTipBox; //커서를올렸을떄 생성될 툴팁박스
    private DraggedObject m_dragobject; //드래그할떄 생성될 오브젝트

    public DRAGGEDOBJECTTYPE m_type { get; private set; } //슬롯의 데이터타입(스킬인지 인벤토리슬롯부터온 아이템인지)
    public bool m_IsData { get; private set; } //데이터가있는가
    public string m_ToolTipText { get; private set; } //ToolTip의 텍스트
    public int m_OriginSlotNumber { get; private set; } //원래 슬롯번호
  
    private void Awake()
    {
        DefaultSetting();
    }

    public void DefaultSetting() //한번만 호출되는 데이터연결함수
    {
        m_QuickSlotImage.enabled = false;
        QuantityText.enabled = false;
        m_ToolTipBox = UIManager.Instance.GetToolTipBox;
        m_dragobject = UIManager.Instance.GetDraggedObject;
        KeyText.text = Key;
    }
    public void Clear()
    {
        if (m_ToolTipBox.gameObject.activeSelf)
            m_ToolTipBox.gameObject.SetActive(false);
        m_QuickSlotImage.enabled = false;
        QuantityText.enabled = false;
        m_IsData = false;
        m_OriginSlotNumber = -1;
        m_type = DRAGGEDOBJECTTYPE.NONE;
        m_ToolTipText = "";
    }

    public void SetSlotData(DRAGGEDOBJECTTYPE type, int SlotNumber) //인벤토리. 스킬슬롯에서 데이터가왔을때 데이터셋팅
    {
        if (m_IsData) //기존에 데이터가 있으면
            Clear(); //기존 데이터 없애기

        m_type = type; //어느슬롯에서 왔는가?(인벤토리? 스킬?)
        m_OriginSlotNumber = SlotNumber; //원래 있던 곳의 번호
        m_QuickSlotImage.enabled = true; //이미지 활성화
        if (type == DRAGGEDOBJECTTYPE.INVENTORYSLOT) //인벤토리에서 왔을경우 
        {
            var ItemQuantity = Player.instance.GetSlotItemData(m_OriginSlotNumber).Quantity; 
            if (ItemQuantity > 0) 
            {
                SetSlotQuantityText(ItemQuantity.ToString());
            }
            SetSlotToolTipText(ItemManager.instance.MakeItemToolTipText(m_OriginSlotNumber, null));
            m_QuickSlotImage.sprite = Resources.Load<Sprite>("UI/Item/" + DataTableManager.instance.GetItemData(Player.instance.GetSlotItemData(m_OriginSlotNumber).ItemNumber).ImageName);
        }
        else if (type == DRAGGEDOBJECTTYPE.SKILLUISLOT)
        {
            SetSlotToolTipText(DataTableManager.instance.MakeSkillToolTip(SlotNumber, Player.instance.GetPlayerStatus.GetSkillLevelByName(DataTableManager.instance.SkillUniqueNumberToSkillnName[m_OriginSlotNumber])));
            m_QuickSlotImage.sprite = Resources.Load<Sprite>("UI/skill_image/" + DataTableManager.instance.GetSkillData(DataTableManager.instance.GetSkillNameBySkillUniqueNumber(m_OriginSlotNumber)).SkillImageFileName);
        }
        OnOffQuantityText(); //갯수텍스트 활성화 비 활성화
      
        
        m_IsData = true;
    }

    public void ChangeKey(string NewKey)
    {
        Key = NewKey;
        KeyText.text = Key;
    }

    public void SetSlotToolTipText(string NewTooltip) //툴팁텍스트 변경
    {
       // Debug.Log(m_OriginSlotNumber);
        m_ToolTipBox.SetToolTipText(NewTooltip);
        m_ToolTipText = NewTooltip;
    }

    public void SetSlotQuantityText(string NewQuantity) //갯수텍스트 변경
    {
        QuantityText.text = NewQuantity;

        if (NewQuantity == "0")
            Clear();
    }

    public void ChangeOriginSlotNumber(int NewSlotNumber) //인벤토리내 아이템의 위치가 변경되었을경우 사용되는 함수
    {
        if(m_type == DRAGGEDOBJECTTYPE.INVENTORYSLOT) //다른경우엔 못쓰게할테지만 혹시나 예외처리
        m_OriginSlotNumber = NewSlotNumber;
    }

    public void OnOffQuantityText() //들어온자료출처(위치)에 따라 갯수텍스트를 온오프
    {
        switch (m_type)
        {
            case DRAGGEDOBJECTTYPE.INVENTORYSLOT:
                {
                    QuantityText.enabled = true;
                }
                break;
            case DRAGGEDOBJECTTYPE.SKILLUISLOT:
                {
                    QuantityText.enabled = false;
                }
                break;
            default:
                return;
        }
    }

    public void UseQuickSlot()
    {
        switch (m_type)
        {
            case DRAGGEDOBJECTTYPE.INVENTORYSLOT:
                {
                    Player.instance.UseConsumptionItem(m_OriginSlotNumber, this);
                }
                break;
            case DRAGGEDOBJECTTYPE.SKILLUISLOT:
                {
                    Player.instance.UseSkill(DataTableManager.instance.GetSkillNameBySkillUniqueNumber(m_OriginSlotNumber));
                }
                break;
            default:
                return;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시
    {
        if (m_dragobject.IsDrag)
            m_dragobject.SetDraggingSlotData(DRAGGEDOBJECTTYPE.QUICKSLOT, m_SlotNumber);

        if (m_IsData)
        {
            var Position = gameObject.transform.position;
            m_ToolTipBox.SetToolTipPosition(Position.x, Position.y + 30);
            m_ToolTipBox.SetToolTipText(m_ToolTipText);
            m_ToolTipBox.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (m_IsData)
        {
            m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        if (m_IsData)
        {
            if((int)eventData.button == 0)
                m_dragobject.SetStartSlotData(DRAGGEDOBJECTTYPE.QUICKSLOT, m_SlotNumber, m_QuickSlotImage.sprite, m_ToolTipText);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_dragobject.IsDrag)
        {
            m_dragobject.EndDrag();
        }
    }
}