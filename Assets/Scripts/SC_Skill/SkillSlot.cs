using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber;
    [SerializeField] private string m_SkillName; //스킬이름
    [SerializeField] private Image m_ItemImage; //스킬이미지
    [SerializeField] private Text m_SkillNameText; //스킬이름(이름으로 스킬데이터와 연결시킬예정)
    [SerializeField] private Text m_SkillLevelText; //스킬레밸
    [SerializeField] private Button m_SkillLevelUpButton; //스킬레밸업 버튼
    [SerializeField] private Image m_SkillBlindBox; //스킬레밸이 0일떄(스킬포인트 미투자시 스킬을 흐려지게 블라인드박스)
    private ToolTip m_ToolTipBox;
    private DraggedObject dragobject; //드래그 이동, 교환 기능 이용시 활성화되는 오브젝트
    private int m_SkillLevel;
    private int m_MaxSkillLevel;
    private string m_ToolTipText;
    private bool m_IsData = false; //데이터가있는지에 대한 변수

    private void Awake()
    {
        SetSkillSlotData();
        ClickSkillLevelUpButton();
    }

    public void SetSkillSlotData() //한번만호출
    {
        m_SkillNameText.text = m_SkillName;
        var SkillData = DataTableManager.instance.GetSkillData(m_SkillName);
        m_ItemImage.sprite = Resources.Load<Sprite>("UI/skill_image/" + SkillData.SkillImageFileName);
        m_MaxSkillLevel = SkillData.SkillMaxLevel;
        m_ToolTipBox = UIManager.Instance.GetToolTipBox;
        dragobject = UIManager.Instance.GetDraggedObject;
        UpdateSkillLevelUI();
        m_ToolTipText = DataTableManager.instance.MakeSkillToolTip(m_SlotNumber, Player.instance.GetPlayerStatus.GetSkillLevelByName(m_SkillName));
        m_IsData = true;
        EraseImageBlindBox();
    }

    public void EraseImageBlindBox() //스킬레벨이 1이상이 되어 활성화 되었을 때 블라인드박스 제거
    {
        if(m_SkillLevel > 0)
        {
            if(m_SkillBlindBox.gameObject)
            m_SkillBlindBox.gameObject.SetActive(false);
        }
    }

    public void UpdateSkillLevelUI() //스킬레벨 정보 갱신시에 따로 사용하기위한 용도
    {
        m_SkillLevel = Player.instance.GetPlayerStatus.GetSkillLevelByName(m_SkillName);
        m_SkillLevelText.text = DataTableManager.instance.MakeSkillLevelAndMaxLevelText(m_SkillLevel, m_MaxSkillLevel);
    }

    public void IsOnQuickSlotUpdateQuickSlotToolTip() //퀵슬롯에 등록된 경우 퀵슬롯 툴팁도 갱신
    {
        int QuickSlotNumber = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(m_SlotNumber, DRAGGEDOBJECTTYPE.SKILLUISLOT);
        if (QuickSlotNumber != -1)
            UIManager.Instance.GetQuickSlotsData.SetQuickSlotToolTipText(QuickSlotNumber, m_ToolTipText);
    }

    public void ClickSkillLevelUpButton()
    {
        m_SkillLevelUpButton.onClick.AddListener(SkillLevelUpButtonOnClickHandler);
    }

    public void SkillLevelUpButtonOnClickHandler() //스킬레벨업 버튼을 눌럿을 때
    {
        if (Player.instance.GetPlayerStatus.SkillLevelUpByName(m_SkillName, m_SkillLevel))
        {
            UpdateSkillLevelUI(); //스킬슬롯의 레벨 UI 갱신
            m_ToolTipText = DataTableManager.instance.MakeSkillToolTip(m_SlotNumber, Player.instance.GetPlayerStatus.GetSkillLevelByName(m_SkillName));
            m_ToolTipBox.SetToolTipText(m_ToolTipText); //스킬의 툴팁 텍스트 갱신
            IsOnQuickSlotUpdateQuickSlotToolTip(); //퀵슬롯에 등록된 스킬의 경우 퀵슬롯의 툴팁 텍스트 갱신
            EraseImageBlindBox(); //레벨이 1 이상일 경우 스킬의 블라인드 박스 제거
        }
    }


    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시
    {
        if (m_IsData)
        {
            var Position = gameObject.transform.position;
            m_ToolTipBox.SetToolTipPosition(Position.x, Position.y);

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
            if (DataTableManager.instance.GetSkillData(m_SkillName).SkillType == SKILLTYPE.SKILLVALUE_ACTIVE && m_SkillLevel > 0)
            {
                switch ((int)eventData.button)
                {
                    case 0: //마우스왼쪽클릭(스킬드래그생성(드래그생성))
                        dragobject.SetStartSlotData(DRAGGEDOBJECTTYPE.SKILLUISLOT, m_SlotNumber, m_ItemImage.sprite, m_ToolTipText);
                        break;
                    case 1: //마우스오른쪽 클릭(액티브 스킬일 경우 사용(패시브는 아무영향없음))
                        Player.instance.UseSkill(m_SkillName);
                        break;
                }
            }
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (dragobject.IsDrag)
        {
            dragobject.EndDrag();
        }
        
    }
}
