using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;
using System.IO;

public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private Image m_ItemImage; //�̹���
    private ToolTip tooltip; //����

    //�����Ͱ� �ߵ��Դ��� Ȯ���ϱ����� �ӽ÷� �ø���������ʵ� �޾ҽ��ϴ�.
    [SerializeField] private int m_SlotNumber; //���԰�����ȣ(����������)
    public int GetSlotNumber { get { return m_SlotNumber; } }
    [SerializeField] private string m_ToolTipText; //�����ؽ�Ʈ
    [SerializeField] private bool IsData; //�����Ͱ��ִ°��� ����
    public bool GetIsData { get { return IsData; } }
    private SlotEquipmentData SlotData; //�ش罽�Կ� ����ִ� ���������� ����
    public SlotEquipmentData GetSlotData { get { return SlotData; } }
    bool isInit = false;


    public void DefaultSetting() //�ʱ���������
    {
        IsData = false;
        m_ToolTipText = "�����;���";
        SlotData = new SlotEquipmentData();
        tooltip = UIManager.Instance.GetToolTipBox;
        m_ItemImage.enabled = false;
        isInit = true;
    }

    public bool SetSlotData(SlotEquipmentData NewEquipment, string NewTooltipData) //���Կ� ���� �����͸� ������� ���Ե����ͼ���(���� ������������� �����ϰԲ�)
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

    public void ClearSlotData() //���Կ��� ��� �������� �ʱ�ȭ
    {
        if (tooltip.gameObject.activeSelf)
            tooltip.gameObject.SetActive(false);
        IsData = false;
        isInit = false;
        SlotData = new SlotEquipmentData();
        m_ToolTipText = "�����;���";
        m_ItemImage.enabled = false;
        Player.instance.SetPlayerEquipmentStatus();
    }

    public bool ReturnEquipmentToInventory() //��� �κ��丮�� �ǵ����� ���
    {
        return Player.instance.ReturnEquipment(SlotData);
    }

    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ��
    {
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

    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� �������� �ش罽�Կ� �������� �ִٸ� �κ��丮��
    {
        if (IsData)
        {
            if (eventData.button != PointerEventData.InputButton.Middle) //���콺 ������,����Ŭ���Ѵ� �����̺�Ʈ
            {
                if (!ReturnEquipmentToInventory()) //���â���� �κ��丮�� �������� ������ �۾��� �����ϸ� �״�� ����
                    return;

                ClearSlotData(); //�������� ����
                Player.instance.SetPlayerEquipmentStatus(); //�÷��̾����ɷ�ġ�հ� ����
            }
        }
    }
}