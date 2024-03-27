using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber;
    [SerializeField] private string m_SkillName; //��ų�̸�
    [SerializeField] private Image m_ItemImage; //��ų�̹���
    [SerializeField] private Text m_SkillNameText; //��ų�̸�(�̸����� ��ų�����Ϳ� �����ų����)
    [SerializeField] private Text m_SkillLevelText; //��ų����
    [SerializeField] private Button m_SkillLevelUpButton; //��ų����� ��ư
    [SerializeField] private Image m_SkillBlindBox; //��ų������ 0�ϋ�(��ų����Ʈ �����ڽ� ��ų�� ������� ����ε�ڽ�)
    private ToolTip m_ToolTipBox;
    private DraggedObject dragobject; //�巡�� �̵�, ��ȯ ��� �̿�� Ȱ��ȭ�Ǵ� ������Ʈ
    private int m_SkillLevel;
    private int m_MaxSkillLevel;
    private string m_ToolTipText;
    private bool m_IsData = false; //�����Ͱ��ִ����� ���� ����

    private void Awake()
    {
        SetSkillSlotData();
        ClickSkillLevelUpButton();
    }

    public void SetSkillSlotData() //�ѹ���ȣ��
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

    public void EraseImageBlindBox() //��ų������ 1�̻��� �Ǿ� Ȱ��ȭ �Ǿ��� �� ����ε�ڽ� ����
    {
        if(m_SkillLevel > 0)
        {
            if(m_SkillBlindBox.gameObject)
            m_SkillBlindBox.gameObject.SetActive(false);
        }
    }

    public void UpdateSkillLevelUI() //��ų���� ���� ���Žÿ� ���� ����ϱ����� �뵵
    {
        m_SkillLevel = Player.instance.GetPlayerStatus.GetSkillLevelByName(m_SkillName);
        m_SkillLevelText.text = DataTableManager.instance.MakeSkillLevelAndMaxLevelText(m_SkillLevel, m_MaxSkillLevel);
    }

    public void IsOnQuickSlotUpdateQuickSlotToolTip() //�����Կ� ��ϵ� ��� ������ ������ ����
    {
        int QuickSlotNumber = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(m_SlotNumber, DRAGGEDOBJECTTYPE.SKILLUISLOT);
        if (QuickSlotNumber != -1)
            UIManager.Instance.GetQuickSlotsData.SetQuickSlotToolTipText(QuickSlotNumber, m_ToolTipText);
    }

    public void ClickSkillLevelUpButton()
    {
        m_SkillLevelUpButton.onClick.AddListener(SkillLevelUpButtonOnClickHandler);
    }

    public void SkillLevelUpButtonOnClickHandler() //��ų������ ��ư�� ������ ��
    {
        if (Player.instance.GetPlayerStatus.SkillLevelUpByName(m_SkillName, m_SkillLevel))
        {
            UpdateSkillLevelUI(); //��ų������ ���� UI ����
            m_ToolTipText = DataTableManager.instance.MakeSkillToolTip(m_SlotNumber, Player.instance.GetPlayerStatus.GetSkillLevelByName(m_SkillName));
            m_ToolTipBox.SetToolTipText(m_ToolTipText); //��ų�� ���� �ؽ�Ʈ ����
            IsOnQuickSlotUpdateQuickSlotToolTip(); //�����Կ� ��ϵ� ��ų�� ��� �������� ���� �ؽ�Ʈ ����
            EraseImageBlindBox(); //������ 1 �̻��� ��� ��ų�� ����ε� �ڽ� ����
        }
    }


    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ��
    {
        if (m_IsData)
        {
            var Position = gameObject.transform.position;
            m_ToolTipBox.SetToolTipPosition(Position.x, Position.y);

            m_ToolTipBox.SetToolTipText(m_ToolTipText);
            m_ToolTipBox.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        if (m_IsData)
        {
            m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        if (m_IsData)
        {
            if (DataTableManager.instance.GetSkillData(m_SkillName).SkillType == SKILLTYPE.SKILLVALUE_ACTIVE && m_SkillLevel > 0)
            {
                switch ((int)eventData.button)
                {
                    case 0: //���콺����Ŭ��(��ų�巡�׻���(�巡�׻���))
                        dragobject.SetStartSlotData(DRAGGEDOBJECTTYPE.SKILLUISLOT, m_SlotNumber, m_ItemImage.sprite, m_ToolTipText);
                        break;
                    case 1: //���콺������ Ŭ��(��Ƽ�� ��ų�� ��� ���(�нú�� �ƹ��������))
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
