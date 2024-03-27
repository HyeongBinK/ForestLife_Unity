using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    private List<Slot> Inventory_Slots = new List<Slot>(); //���Ե���List
    [SerializeField] private Transform SlotsPosition; //���Ե��� �����ġ
    [SerializeField] private int Cur_ActivatedSlotNumber = 0; //����Ȱ��ȭ�Ƚ����Ǽ� 
    [SerializeField] private Slot Slot_prefab; //������ ������ ������
    [SerializeField] private Text Gold;
    private int m_iSlotNumber = 0; //������ ������ȣ���ɹ�ȣ(ó����0, makeslot ����� ���� 1

    public void SetGoldText(int NewGold)
    {
        if (!Gold)
        {
            GameManager.instance.AddNewLog("Error!!, InventoryUI/Text Gold ����ȵ�");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.Append(NewGold.ToString());
        sb.Append(" Gold");
        Gold.text = sb.ToString();
    }

    public void SetActivatedSlotNumber(int NewNumber) 
    {
        Cur_ActivatedSlotNumber = Player.instance.CurActivatedSlotNumber();
        int ExistingSlotNumber = Cur_ActivatedSlotNumber;
        Cur_ActivatedSlotNumber = NewNumber;
        if(ExistingSlotNumber != Cur_ActivatedSlotNumber)
        {
            MakeSlot(Cur_ActivatedSlotNumber - ExistingSlotNumber);
        }
    }


    public void MakeSlot(int NewSlotNumber) //���鰹���޾ƿ�
    {
        for (int i = Cur_ActivatedSlotNumber; i < Cur_ActivatedSlotNumber + NewSlotNumber; i++)
        {
            Slot newSlot = Instantiate(Slot_prefab, SlotsPosition.transform);
            newSlot.name = m_iSlotNumber.ToString();
            newSlot.SetDefaultSetting(Inventory_Slots.Count);
            Inventory_Slots.Add(newSlot);
            m_iSlotNumber++;
        }

    }
    public void SetSlotData(int SlotNumber)
    {
        Inventory_Slots[SlotNumber].SetSlotData(ItemManager.instance.MakeItemToolTipText(SlotNumber, Player.instance.GetSlotEquipmentData(SlotNumber)));
    }
}
