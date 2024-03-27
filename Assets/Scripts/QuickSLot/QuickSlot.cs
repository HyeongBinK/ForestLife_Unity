using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class QuickSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private int m_SlotNumber; //�����԰�����ȣ �տ������� 0~
    public int GetQuickSlotUniqueNumber { get { return m_SlotNumber; } }
    [SerializeField] private Image m_QuickSlotImage; //�̹���
    public Sprite GetQuickSlotImage { get { return m_QuickSlotImage.sprite; } }
    [SerializeField] private Text QuantityText; //����(�κ��丮���Կ����� �����۵������� ������� �׿ܿ� off)
    [SerializeField] private Text KeyText; //���Ű���������� ���Ǵ� Ű�������� ǥ��
    [SerializeField] public string Key; //�ش�Ű
    private ToolTip m_ToolTipBox; //Ŀ�����÷����� ������ �����ڽ�
    private DraggedObject m_dragobject; //�巡���ҋ� ������ ������Ʈ

    public DRAGGEDOBJECTTYPE m_type { get; private set; } //������ ������Ÿ��(��ų���� �κ��丮���Ժ��Ϳ� ����������)
    public bool m_IsData { get; private set; } //�����Ͱ��ִ°�
    public string m_ToolTipText { get; private set; } //ToolTip�� �ؽ�Ʈ
    public int m_OriginSlotNumber { get; private set; } //���� ���Թ�ȣ
  
    private void Awake()
    {
        DefaultSetting();
    }

    public void DefaultSetting() //�ѹ��� ȣ��Ǵ� �����Ϳ����Լ�
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

    public void SetSlotData(DRAGGEDOBJECTTYPE type, int SlotNumber) //�κ��丮. ��ų���Կ��� �����Ͱ������� �����ͼ���
    {
        if (m_IsData) //������ �����Ͱ� ������
            Clear(); //���� ������ ���ֱ�

        m_type = type; //������Կ��� �Դ°�?(�κ��丮? ��ų?)
        m_OriginSlotNumber = SlotNumber; //���� �ִ� ���� ��ȣ
        m_QuickSlotImage.enabled = true; //�̹��� Ȱ��ȭ
        if (type == DRAGGEDOBJECTTYPE.INVENTORYSLOT) //�κ��丮���� ������� 
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
        OnOffQuantityText(); //�����ؽ�Ʈ Ȱ��ȭ �� Ȱ��ȭ
      
        
        m_IsData = true;
    }

    public void ChangeKey(string NewKey)
    {
        Key = NewKey;
        KeyText.text = Key;
    }

    public void SetSlotToolTipText(string NewTooltip) //�����ؽ�Ʈ ����
    {
       // Debug.Log(m_OriginSlotNumber);
        m_ToolTipBox.SetToolTipText(NewTooltip);
        m_ToolTipText = NewTooltip;
    }

    public void SetSlotQuantityText(string NewQuantity) //�����ؽ�Ʈ ����
    {
        QuantityText.text = NewQuantity;

        if (NewQuantity == "0")
            Clear();
    }

    public void ChangeOriginSlotNumber(int NewSlotNumber) //�κ��丮�� �������� ��ġ�� ����Ǿ������ ���Ǵ� �Լ�
    {
        if(m_type == DRAGGEDOBJECTTYPE.INVENTORYSLOT) //�ٸ���쿣 �������������� Ȥ�ó� ����ó��
        m_OriginSlotNumber = NewSlotNumber;
    }

    public void OnOffQuantityText() //�����ڷ���ó(��ġ)�� ���� �����ؽ�Ʈ�� �¿���
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

    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ��
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