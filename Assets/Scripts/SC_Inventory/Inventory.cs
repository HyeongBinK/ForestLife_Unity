using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[System.Serializable]
public class SlotItemData
{
    public int ItemNumber; //�������� ������ȣ
    public int Quantity; //�������� ��(����)
    public int SlotNumber; //��佽�Թ�ȣ
}

[System.Serializable]
public class SlotEquipmentData
{
    public EQUIPMENT_TYPE Type; //���������� ����
    public int ItemNumber = -1; //�����۰�����ȣ
    public int Cur_ReinforceNum; //���糲����ȭ����Ƚ��
    public int Reinforce; //���� ��ȭ�� Ƚ�� 
    public int SlotNumber = -1; //��佽�Թ�ȣ
    public Status ItemStatus;
}

enum InventoryData
{
    MAXQUANTITY = 99, //�ִ���������簡�ɷ�
    MAXGOLD = 99999999, //�ִ������簡�ɷ�
    BASESLOT = 10, //�⺻������ ����
    MAXSLOT = 40, //�ִ� Ȯ�尡���ѽ����� ����
}

[System.Serializable]
public class InventorySaveData
{
    public int InventoryActiveSlotNumberData; //����Ȱ��ȭ�� �����ǰ���
    public int GoldData; //�������� �ݾ� ������
    public List<SlotEquipmentData> EquipmentsSaveData = new List<SlotEquipmentData>();
    public List<SlotItemData> SlotItemSaveData = new List<SlotItemData>();

    public void SetSaveData(int SlotNumber, int Gold, List<bool> IsSlotDatas, Dictionary<int, SlotEquipmentData> EquipmentDatas, Dictionary<int, SlotItemData> SlotDatas)
    {
        InventoryActiveSlotNumberData = SlotNumber;
        GoldData = Gold;

        for (int i = 0; i < IsSlotDatas.Count; i++)
        {
            if (IsSlotDatas[i])
            { 
                EquipmentsSaveData.Add(EquipmentDatas[i]);
                SlotItemSaveData.Add(SlotDatas[i]);
            }
        }
    }
}

public class Inventory
{
    public int GetInventorySlotNumber { get { return IsSlotDatas.Count; } }
    private List<bool> IsSlotDatas = new List<bool>(); //���Ը��� �����Ͱ��ִ��� �������� ����
    private Dictionary<int, SlotEquipmentData> EquipmentDatas = new Dictionary<int, SlotEquipmentData>(); //Dictionary<���Թ�ȣ/ �������� �ɷ�ġ>
    private Dictionary<int, SlotItemData> SlotDatas = new Dictionary<int, SlotItemData>(); //Dictionary<���Թ�ȣ/ �����۰�����ȣ+����> 
    public int Gold { get; private set; }

    public void DefaultSetting() //���� ���ν���(�ʱ�ȭ)�� �κ��丮 �ʱ� ����
    {
        GetGold(0);
        UpgradeSlotNumber((int)InventoryData.BASESLOT);
    }

    public void GetGold(int PlusGold) //���ȹ��� �κ��丮�� �����
    {
        // Gold = Mathf.Min(Gold + PlusGold, MAXGOLD);
        int result = (int)InventoryData.MAXGOLD - Gold;
        if (PlusGold > result)
        {
            Gold = (int)InventoryData.MAXGOLD;
            GameManager.instance.AddNewLog("�ִ뺸������ ��忡 �����Ͽ��� �߰��� ��带 ȹ���Ҽ������ϴ�.");
            return;
        }
        Gold += PlusGold;

        GameManager.instance.AddNewLog(PlusGold.ToString() + "Gold ȹ��");
        UIManager.Instance.SetInventoryGoldText(Gold);
    }

    public bool UseGold(int Price) //������ �κ��丮�� ��尨��
    {
        Price = Mathf.Abs(Price);
        if (Gold >= Price)
        {
            GameManager.instance.AddNewLog(Price.ToString() + "Gold ���");
            Gold -= Price;
            UIManager.Instance.SetInventoryGoldText(Gold);
            GameManager.instance.AddNewLog("���� Gold :" + Gold.ToString() + "Gold");
            return true;
        }
        GameManager.instance.AddNewLog("��尡 �����մϴ�");
        return false;
    }

    public void UpgradeSlotNumber(int NewSlotsNumber) //�ִ뽽�԰��� Ȯ����
    {
        if (0 >= NewSlotsNumber) return;

        if(GetInventorySlotNumber >= (int)InventoryData.MAXSLOT)
        {
            GameManager.instance.AddNewLog("���� ������ �ִ�� Ȯ��Ǿ��ֽ��ϴ�");
            return;
        }
        int StartIndex = GetInventorySlotNumber;
        int maxCount = Mathf.Clamp(StartIndex + NewSlotsNumber, (int)InventoryData.BASESLOT, (int)InventoryData.MAXSLOT);

        int Size = maxCount - StartIndex;
        UIManager.Instance.SetInventorySlotNumber(maxCount);
        for (int i = 0; i < Size; i++)
        {
            IsSlotDatas.Add(false);
        }
    }

    //�÷��̾�ο��� �κ��丮�� ������ȣ, �����۰�����ȣ, �����۰���, ������ ���濡 �����ϸ� true, ���н� false ��ȯ
    //ItemNumber�� ���� �ִ�ġ�̻����� ���ü����Բ� 
    public bool AddItem(int ItemUniqueNumber, int ItemQuantity = 1) //�����۰�����ȣ, �����۰���
    {
        if (ItemQuantity > (int)InventoryData.MAXQUANTITY) //ȹ���� �������� ������ ���������̸� �˸��޽����� �Բ� ����
        {
            GameManager.instance.AddNewLog("�ѹ��� �����Ҽ� �ִ� ���������ѵ����Ѿ����ϴ�!");
            return false;
        }

        int SlotNumber = -1; //�������� ���� �� ������ �ѹ�
        //bool IsSpareSlot = false; //�����������ִ��� ������ true ������ false

        SlotItemData NewSlotItemData = new SlotItemData();
        //�ߺ�������üũ
        if (DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType != ITEM_TYPE.EQUIPMENT) //�������ۿ��� �������� �������ִ�ġ��ŭ �Բ� ����(����)�Ҽ������Ƿ� ������ Ÿ���� �������� �ִ��� �Ǻ�
        {
            for (int i = 0; i < GetInventorySlotNumber; i++)
            {
                if (!IsSlotDatas[i])
                    continue;
                if (SlotDatas[i].ItemNumber == ItemUniqueNumber)
                {
                    SlotItemData OriginalItemData = SlotDatas[i];
                    if (OriginalItemData.Quantity >= (int)InventoryData.MAXQUANTITY)
                        continue;
                    
                    NewSlotItemData.ItemNumber = OriginalItemData.ItemNumber;
                    NewSlotItemData.SlotNumber = OriginalItemData.SlotNumber;
                    if (OriginalItemData.Quantity + ItemQuantity > (int)InventoryData.MAXQUANTITY)
                    {
                        ItemQuantity = (int)InventoryData.MAXQUANTITY - OriginalItemData.Quantity;
                        NewSlotItemData.Quantity = (int)InventoryData.MAXQUANTITY;
                        SlotDatas[i] = NewSlotItemData;
                        continue;
                    }
                    else
                    {
                        NewSlotItemData.Quantity = OriginalItemData.Quantity + ItemQuantity;
                        SlotDatas[i] = NewSlotItemData;
                        UpdateSlotUI(i);
                        IsOnQuickSlotChangeQuantityData(i);
                        return true;
                    }
                }
                
            }
        }

        for (int i = 0; i < IsSlotDatas.Count; i++) //�����۵����Ͱ� ����ִ� ������ ���� üũ�Ͽ� �� �������ִ��� üũ
        {
            if (IsSlotDatas[i] == false)
            {
                SlotNumber = i;
                //IsSpareSlot = true;
                break;
            }
        }

       
        if (0 > SlotNumber) //���������̾����� �˸��޽����� �Բ� ����
        {
            //IsInventoryFull = true;
            GameManager.instance.AddNewLog("�κ��丮�� �������־ �������� �����Ҽ� �����ϴ�.");
            return false;
        }
        
        NewSlotItemData.ItemNumber = ItemUniqueNumber;
        NewSlotItemData.Quantity = ItemQuantity;
        NewSlotItemData.SlotNumber = SlotNumber;

        SlotEquipmentData NewEquipmentData = new SlotEquipmentData();
        NewEquipmentData.SlotNumber = SlotNumber;
        if (DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType == ITEM_TYPE.EQUIPMENT)
        {
            var data = DataTableManager.instance.GetEquipmentItemData(ItemUniqueNumber);
            NewEquipmentData.Type = data.Type;
            NewEquipmentData.ItemNumber = ItemUniqueNumber;
            NewEquipmentData.ItemStatus = ItemManager.instance.MakeRandomStatus(data.Min_Status, data.Max_Status);
            NewEquipmentData.Cur_ReinforceNum = data.Max_ReinforceNum;
            NewEquipmentData.Reinforce = 0;
        }
        
        IsSlotDatas[SlotNumber] = true;
        SlotDatas.Add(SlotNumber, NewSlotItemData);
        EquipmentDatas.Add(SlotNumber, NewEquipmentData);
        UpdateSlotUI(SlotNumber);
        return true;
    }

    //������� or ��ȯ �� ������� �ǵ����� �Լ�
    public bool ReturnEquipmentItem(SlotEquipmentData Equipment) 
    {
        int SlotNumber = -1; //�������� ���� �� ������ �ѹ�
        SlotItemData NewSlotItemData = new SlotItemData();
     
        for (int i = 0; i < IsSlotDatas.Count; i++) //�����۵����Ͱ� ����ִ� ������ ���� üũ�Ͽ� �� �������ִ��� üũ
        {
            if (IsSlotDatas[i] == false)
            {
                SlotNumber = i;
                //IsSpareSlot = true;
                break;
            }
        }

        if (0 > SlotNumber) //���������̾����� �˸��޽����� �Բ� ����
        {
            GameManager.instance.AddNewLog("�κ��丮�� �������־ �������� �߰��Ҽ� �����ϴ�.");
            return false;
        }

        NewSlotItemData.ItemNumber = Equipment.ItemNumber;
        NewSlotItemData.Quantity = 1;
        NewSlotItemData.SlotNumber = SlotNumber;
        Equipment.SlotNumber = SlotNumber;

        IsSlotDatas[SlotNumber] = true;
        SlotDatas.Add(SlotNumber, NewSlotItemData);
        EquipmentDatas.Add(SlotNumber, Equipment);
        UpdateSlotUI(SlotNumber);
        return true;
    }

    //�κ��丮�� ������ �����͸� �������� �Լ�
    private void ClearSlotData(int SlotNumber)
    {
        IsSlotDatas[SlotNumber] = false;
        if(SlotDatas.ContainsKey(SlotNumber)) 
            SlotDatas.Remove(SlotNumber);
        if(EquipmentDatas.ContainsKey(SlotNumber))
            EquipmentDatas.Remove(SlotNumber);
      //  IsInventoryFull = false;
        UpdateSlotUI(SlotNumber);
    }

    //�����۹�������
    public bool ThrowAwayItem(int SlotNumber, int ItemQuantity) //�ش罽�Թ�ȣ, �����۰���. ��ĭ�ǰ�� ���Ҵ� ���������ǰ�� ItemNumber = 1 
    {
        if(!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("����Ҽ������ϴ�(�󽽷�)");
            return false;
        }
        
        switch (DataTableManager.instance.GetItemData(SlotDatas[SlotNumber].ItemNumber).ItemType)
        {
            case ITEM_TYPE.EQUIPMENT:
                {
                    ClearSlotData(SlotNumber);
                }
                break;
            default:
                {
                    var SlotData = SlotDatas[SlotNumber];
                    if(ItemQuantity < SlotData.Quantity)
                    {
                        SlotData.Quantity = Mathf.Clamp(SlotData.Quantity - ItemQuantity, 0, (int)InventoryData.MAXQUANTITY);
                        SlotDatas[SlotNumber] = SlotData;
                    }
                    else
                        ClearSlotData(SlotNumber);
                }
                break;
        }
        UpdateSlotUI(SlotNumber);
        return true;
    }

    //�����ۻ��/����ó��(�������� ����/ �������� ����)
    public bool UseItem(int SlotNumber) //�Ҹ�������� �Ҹ�����۰���ó��, �󽽷��� ��� �翬�� �ش��Լ����Ҵ�, ���������ǰ�� ����ó��, �׿ܿ� ���Ҵ�
    {
        if (!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("����Ҽ������ϴ�(�󽽷�)");
            return false;
        }

        var SlotData = SlotDatas[SlotNumber];

        //�����Կ� ��ϵȾ��������� Ȯ��

        switch (DataTableManager.instance.GetItemData(SlotData.ItemNumber).ItemType)
        {
            case ITEM_TYPE.EQUIPMENT:
                {
                    ClearSlotData(SlotNumber);
                }
                break;
            case ITEM_TYPE.SCROLL:
            case ITEM_TYPE.CONSUMPTION:
                {
                    int CurItemQuanity = SlotData.Quantity;

                    switch (CurItemQuanity)
                    {
                        case 0:
                            GameManager.instance.AddNewLog("Critical Error!!! �����Ѱ����� 0���� �����Ͱ�����ֽ��ϴ�! Inventory/UseItem");
                            return false;
                        case 1:
                            {
                                ClearSlotData(SlotNumber);
                            }
                            break;
                        default:
                            {
                                SlotData.Quantity = Mathf.Clamp(CurItemQuanity - 1, 0, (int)InventoryData.MAXQUANTITY);
                                SlotDatas[SlotNumber] = SlotData;

                            }
                            break;
                    }
                }
                break;
            default:
                {
                    return false;
                }

        }
        UpdateSlotUI(SlotNumber);
      //  PlayerAct.instance.SetPlayerIsAct(true);
        return true;
    }
    public void SwitchQuickSlotOriginNumber(int Slot1, int Slot2) //���� = Slot1, ��� = Slot2
    {
        int QuickSlotNUmber1 = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(Slot1, DRAGGEDOBJECTTYPE.INVENTORYSLOT);
        int QuickSlotNUmber2 = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(Slot2, DRAGGEDOBJECTTYPE.INVENTORYSLOT);

        if (QuickSlotNUmber1 != -1)
        {
            if (QuickSlotNUmber2 != -1)
            {
                UIManager.Instance.GetQuickSlotsData.SetQuickSlotOriginSlotNumber(QuickSlotNUmber1, Slot2);
                UIManager.Instance.GetQuickSlotsData.SetQuickSlotOriginSlotNumber(QuickSlotNUmber2, Slot1);
                return;
            }

            UIManager.Instance.GetQuickSlotsData.SetQuickSlotOriginSlotNumber(QuickSlotNUmber1, Slot2);
        }
    }

    public void IsOnQuickSlotChangeQuantityData(int SlotNumber)
    {
        int QuickSlotNUmber = UIManager.Instance.GetQuickSlotsData.GetQuickSlotNumberByOriginNumber(SlotNumber, DRAGGEDOBJECTTYPE.INVENTORYSLOT);
        if (QuickSlotNUmber != -1)
        {
            UIManager.Instance.GetQuickSlotsData.SetQuickSlotQuantityText(QuickSlotNUmber, SlotDatas[SlotNumber].Quantity.ToString());
        }
    }

    //�������Ǹű��
    public void SellItem(int SlotNumber, int ItemQuantity) // �Ǹ��� �������� �ش罽��, �Ǹ��Ұ���(���������ǰ�� 1), �󽽷��ǰ�� �ش��Լ��� ���ϼ����� ��������
    {
        if (!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("�Ǹ��Ҽ������ϴ�(�󽽷�)");
            return;
        }

        var SlotData = SlotDatas[SlotNumber];
        if (SlotData.Quantity < ItemQuantity)
        {
            GameManager.instance.AddNewLog("������������ ���������� �Ǹ��� �� �����ϴ�");
            return;
        }

        var ItemData = DataTableManager.instance.GetItemData(SlotData.ItemNumber);
        int Price = (int)(ItemData.Price * 0.3f);

        switch (ItemData.ItemType)
        {
            case ITEM_TYPE.EQUIPMENT: //��� ������
                {
                    GetGold(Price);
                    ClearSlotData(SlotNumber);
                }
                break;
            default: //�Ҹ�, ��� ������
                {
                    int CurItemQuanity = SlotData.Quantity;

                    if (CurItemQuanity == ItemQuantity) //�ǸŰ�� ������ 0�� �Ǿ ������ ����� ���
                    {
                        ClearSlotData(SlotNumber);
                    }
                    else //�׿�
                    {
                        SlotData.Quantity = Mathf.Clamp(CurItemQuanity - ItemQuantity, 0, (int)InventoryData.MAXQUANTITY);
                        SlotDatas[SlotNumber] = SlotData; 
                    }
                    GetGold(Price * ItemQuantity); //��� ȹ��
                }
                break;
        }

        IsOnQuickSlotChangeQuantityData(SlotNumber); //�����Կ� ��ϵ� �������� ��� ���� ����
        UpdateSlotUI(SlotNumber); // �κ��丮 ���� ����
    }

    //�����̵�/��ȯ���
    public bool SwitchSlotdata(int Slot1, int Slot2) //Slot1 : �巡�׵���� ������ ���Գѹ�, Slot2 : ��󽽷Գѹ�, �󽽷��� �巡���������ϰ� ���� //���Ե����ͺ��濡�����ϸ� true �����ϸ� false
    {
        if (0 > Slot1 || Slot1 >= GetInventorySlotNumber || 0 > Slot2|| Slot2 >= GetInventorySlotNumber) //���������Ҽ����� ���Թ�ȣ�� ������ ���й�ȯ
            return false;

        bool IsSlotDataTemp = IsSlotDatas[Slot2];
        SlotItemData SlotItemDataTemp;
        SlotEquipmentData EquipmentTemp;
        int SlotNumberTemp1 = Slot1;
        int SlotNumberTemp2 = Slot2;

        if (IsSlotDatas[Slot2])
        {
            SlotItemDataTemp = SlotDatas[Slot2];
            EquipmentTemp = EquipmentDatas[Slot2];
        }
        else
        {
            SlotItemDataTemp = new SlotItemData();
            EquipmentTemp = new SlotEquipmentData();
        }

        IsSlotDatas[Slot2] = IsSlotDatas[Slot1];
        SlotDatas[Slot2] = SlotDatas[Slot1];
        EquipmentDatas[Slot2] = EquipmentDatas[Slot1];
        SlotDatas[Slot2].SlotNumber = SlotNumberTemp2;
        EquipmentDatas[Slot2].SlotNumber = SlotNumberTemp2;

        UpdateSlotUI(Slot2);

        if (!IsSlotDataTemp)
        {
            ClearSlotData(Slot1);
            SwitchQuickSlotOriginNumber(Slot1, Slot2);
            return true;
        }

        IsSlotDatas[Slot1] = IsSlotDataTemp; 
        SlotDatas[Slot1] = SlotItemDataTemp;
        EquipmentDatas[Slot1] = EquipmentTemp;
        SlotDatas[Slot1].SlotNumber = SlotNumberTemp1;
        EquipmentDatas[Slot1].SlotNumber = SlotNumberTemp1;
        UpdateSlotUI(Slot1);
        SwitchQuickSlotOriginNumber(Slot2, Slot1);
        return true;
    }

    //�ܺο��� �κ��丮�� ���������� ������������ �Լ���
    public SlotItemData ExportSlotData(int SlotNumber)
    {
        if (IsSlotDatas[SlotNumber])
        {
            if(GetInventorySlotNumber > SlotNumber)
            return SlotDatas[SlotNumber];
        }
     
        return new SlotItemData();
    }

    public SlotEquipmentData ExportEquipmentData(int SlotNumber)
    {
        if (IsSlotDatas.Count > SlotNumber && IsSlotDatas[SlotNumber])
        {
            if (EquipmentDatas.ContainsKey(SlotNumber))
            {
                return EquipmentDatas[SlotNumber];
            }
        }
        return new SlotEquipmentData();
    }

    public bool ExportIsSlotData(int SlotNumber)
    {
        if (IsSlotDatas.Count > SlotNumber)
            return IsSlotDatas[SlotNumber];
        
        Debug.LogError("Error! ���Թ�ȣ�� �ִ뽽�Լ�ġ���� Ŭ�������ϴ�");
        return false;
    }

    private void UpdateSlotUI(int slotnumber) //����UI����
    {
        UIManager.Instance.SetSlotData(slotnumber);
    }

    public void SaveData()
    {
        InventorySaveData SaveData = new InventorySaveData();
        SaveData.SetSaveData(GetInventorySlotNumber, Gold, IsSlotDatas, EquipmentDatas, SlotDatas);
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("InventoryData","/SaveData/InventoryData/")  , json);
    }

    public bool LoadData()
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath("InventoryData", "/SaveData/InventoryData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("�÷��̾� �κ��丮 ���̺������� ã�����߽��ϴ�.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        InventorySaveData SaveData = JsonUtility.FromJson<InventorySaveData>(SaveFile);

        if (SlotDatas.Count != 0 || EquipmentDatas.Count != 0)
        {
            for (int i = 0; i < IsSlotDatas.Count; i++)
            {
                ClearSlotData(i);
            }
        }

        Gold = SaveData.GoldData;

        UpgradeSlotNumber(SaveData.InventoryActiveSlotNumberData - GetInventorySlotNumber);

        if (SaveData.EquipmentsSaveData != null)
        {
            var RegionEquipmentDatas = SaveData.EquipmentsSaveData;
            for (int i = 0; i < RegionEquipmentDatas.Count; i++)
            {
                EquipmentDatas.Add(RegionEquipmentDatas[i].SlotNumber, RegionEquipmentDatas[i]);
            }
        }

        if (SaveData.SlotItemSaveData != null)
        {
            var RegionSlotItemDatas = SaveData.SlotItemSaveData;
            for (int i = 0; i < RegionSlotItemDatas.Count; i++)
            {
               // if (RegionSlotItemDatas[i].SlotNumber) ;
                SlotDatas.Add(RegionSlotItemDatas[i].SlotNumber, RegionSlotItemDatas[i]);
                IsSlotDatas[RegionSlotItemDatas[i].SlotNumber] = true;
            }
        }

        for (int i = 0; i < IsSlotDatas.Count; i++)
        {
            UpdateSlotUI(i);
        }
     
        return true;
    }
}
