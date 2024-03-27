using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[System.Serializable]
public class SlotItemData
{
    public int ItemNumber; //아이템의 고유번호
    public int Quantity; //아이템의 양(갯수)
    public int SlotNumber; //담긴슬롯번호
}

[System.Serializable]
public class SlotEquipmentData
{
    public EQUIPMENT_TYPE Type; //장비아이템의 부위
    public int ItemNumber = -1; //아이템고유번호
    public int Cur_ReinforceNum; //현재남은강화가능횟수
    public int Reinforce; //현재 강화된 횟수 
    public int SlotNumber = -1; //담긴슬롯번호
    public Status ItemStatus;
}

enum InventoryData
{
    MAXQUANTITY = 99, //최대아이템적재가능량
    MAXGOLD = 99999999, //최대골드적재가능량
    BASESLOT = 10, //기본슬롯의 갯수
    MAXSLOT = 40, //최대 확장가능한슬롯의 갯수
}

[System.Serializable]
public class InventorySaveData
{
    public int InventoryActiveSlotNumberData; //현재활성화된 슬롯의갯수
    public int GoldData; //보유중인 금액 데이터
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
    private List<bool> IsSlotDatas = new List<bool>(); //슬롯마다 데이터가있는지 없는지의 여부
    private Dictionary<int, SlotEquipmentData> EquipmentDatas = new Dictionary<int, SlotEquipmentData>(); //Dictionary<슬롯번호/ 아이템의 능력치>
    private Dictionary<int, SlotItemData> SlotDatas = new Dictionary<int, SlotItemData>(); //Dictionary<슬롯번호/ 아이템고유번호+갯수> 
    public int Gold { get; private set; }

    public void DefaultSetting() //게임 새로시작(초기화)시 인벤토리 초기 셋팅
    {
        GetGold(0);
        UpgradeSlotNumber((int)InventoryData.BASESLOT);
    }

    public void GetGold(int PlusGold) //골드획득시 인벤토리내 골드상승
    {
        // Gold = Mathf.Min(Gold + PlusGold, MAXGOLD);
        int result = (int)InventoryData.MAXGOLD - Gold;
        if (PlusGold > result)
        {
            Gold = (int)InventoryData.MAXGOLD;
            GameManager.instance.AddNewLog("최대보유가능 골드에 도달하여서 추가로 골드를 획득할수없습니다.");
            return;
        }
        Gold += PlusGold;

        GameManager.instance.AddNewLog(PlusGold.ToString() + "Gold 획득");
        UIManager.Instance.SetInventoryGoldText(Gold);
    }

    public bool UseGold(int Price) //골드사용시 인벤토리내 골드감소
    {
        Price = Mathf.Abs(Price);
        if (Gold >= Price)
        {
            GameManager.instance.AddNewLog(Price.ToString() + "Gold 사용");
            Gold -= Price;
            UIManager.Instance.SetInventoryGoldText(Gold);
            GameManager.instance.AddNewLog("남은 Gold :" + Gold.ToString() + "Gold");
            return true;
        }
        GameManager.instance.AddNewLog("골드가 부족합니다");
        return false;
    }

    public void UpgradeSlotNumber(int NewSlotsNumber) //최대슬롯갯수 확장기능
    {
        if (0 >= NewSlotsNumber) return;

        if(GetInventorySlotNumber >= (int)InventoryData.MAXSLOT)
        {
            GameManager.instance.AddNewLog("현재 슬롯이 최대로 확장되어있습니다");
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

    //플레이어가부여한 인벤토리내 고유번호, 아이템고유번호, 아이템갯수, 아이템 습득에 성공하면 true, 실패시 false 반환
    //ItemNumber는 적재 최대치이상으론 들어올수없게끔 
    public bool AddItem(int ItemUniqueNumber, int ItemQuantity = 1) //아이템고유번호, 아이템갯수
    {
        if (ItemQuantity > (int)InventoryData.MAXQUANTITY) //획득한 아이템의 갯수가 비정상적이면 알림메시지와 함께 종료
        {
            GameManager.instance.AddNewLog("한번에 습득할수 있는 아이템의한도를넘었습니다!");
            return false;
        }

        int SlotNumber = -1; //아이템이 새로 들어갈 슬롯의 넘버
        //bool IsSpareSlot = false; //여유슬롯이있는지 있으면 true 없으면 false

        SlotItemData NewSlotItemData = new SlotItemData();
        //중복아이템체크
        if (DataTableManager.instance.GetItemData(ItemUniqueNumber).ItemType != ITEM_TYPE.EQUIPMENT) //장비아이템외의 아이템은 지정한최대치만큼 함께 적재(보관)할수있으므로 동일한 타입의 아이템이 있는지 판별
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

        for (int i = 0; i < IsSlotDatas.Count; i++) //아이템데이터가 들어있는 슬롯의 수를 체크하여 들어갈 공간이있는지 체크
        {
            if (IsSlotDatas[i] == false)
            {
                SlotNumber = i;
                //IsSpareSlot = true;
                break;
            }
        }

       
        if (0 > SlotNumber) //여유슬롯이없으면 알림메시지와 함께 종료
        {
            //IsInventoryFull = true;
            GameManager.instance.AddNewLog("인벤토리가 가득차있어서 아이템을 습득할수 없습니다.");
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

    //장비해제 or 교환 시 기존장비를 되돌리는 함수
    public bool ReturnEquipmentItem(SlotEquipmentData Equipment) 
    {
        int SlotNumber = -1; //아이템이 새로 들어갈 슬롯의 넘버
        SlotItemData NewSlotItemData = new SlotItemData();
     
        for (int i = 0; i < IsSlotDatas.Count; i++) //아이템데이터가 들어있는 슬롯의 수를 체크하여 들어갈 공간이있는지 체크
        {
            if (IsSlotDatas[i] == false)
            {
                SlotNumber = i;
                //IsSpareSlot = true;
                break;
            }
        }

        if (0 > SlotNumber) //여유슬롯이없으면 알림메시지와 함께 종료
        {
            GameManager.instance.AddNewLog("인벤토리가 가득차있어서 아이템을 추가할수 없습니다.");
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

    //인벤토리내 슬롯의 데이터를 비우기위한 함수
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

    //아이템버리기기능
    public bool ThrowAwayItem(int SlotNumber, int ItemQuantity) //해당슬롯번호, 아이템갯수. 빈칸의경우 사용불능 장비아이템의경우 ItemNumber = 1 
    {
        if(!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("사용할수없습니다(빈슬롯)");
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

    //아이템사용/장착처리(데이터의 제거/ 데이터의 변경)
    public bool UseItem(int SlotNumber) //소모아이템의 소모아이템감소처리, 빈슬롯의 경우 당연히 해당함수사용불능, 장비아이템의경우 장착처리, 그외엔 사용불능
    {
        if (!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("사용할수없습니다(빈슬롯)");
            return false;
        }

        var SlotData = SlotDatas[SlotNumber];

        //퀵슬롯에 등록된아이템인지 확인

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
                            GameManager.instance.AddNewLog("Critical Error!!! 보유한갯수가 0개인 데이터가들어있습니다! Inventory/UseItem");
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
    public void SwitchQuickSlotOriginNumber(int Slot1, int Slot2) //시작 = Slot1, 대상 = Slot2
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

    //아이템판매기능
    public void SellItem(int SlotNumber, int ItemQuantity) // 판매할 아이템의 해당슬롯, 판매할갯수(장비아이템의경우 1), 빈슬롯의경우 해당함수가 쓰일수없게 막을예정
    {
        if (!IsSlotDatas[SlotNumber])
        {
            GameManager.instance.AddNewLog("판매할수없습니다(빈슬롯)");
            return;
        }

        var SlotData = SlotDatas[SlotNumber];
        if (SlotData.Quantity < ItemQuantity)
        {
            GameManager.instance.AddNewLog("보유갯수보다 많은수량을 판매할 수 없습니다");
            return;
        }

        var ItemData = DataTableManager.instance.GetItemData(SlotData.ItemNumber);
        int Price = (int)(ItemData.Price * 0.3f);

        switch (ItemData.ItemType)
        {
            case ITEM_TYPE.EQUIPMENT: //장비 아이템
                {
                    GetGold(Price);
                    ClearSlotData(SlotNumber);
                }
                break;
            default: //소모, 재료 아이템
                {
                    int CurItemQuanity = SlotData.Quantity;

                    if (CurItemQuanity == ItemQuantity) //판매결과 갯수가 0이 되어서 슬롯이 비었을 경우
                    {
                        ClearSlotData(SlotNumber);
                    }
                    else //그외
                    {
                        SlotData.Quantity = Mathf.Clamp(CurItemQuanity - ItemQuantity, 0, (int)InventoryData.MAXQUANTITY);
                        SlotDatas[SlotNumber] = SlotData; 
                    }
                    GetGold(Price * ItemQuantity); //골드 획득
                }
                break;
        }

        IsOnQuickSlotChangeQuantityData(SlotNumber); //퀵슬롯에 등록된 아이템의 경우 갯수 갱신
        UpdateSlotUI(SlotNumber); // 인벤토리 슬롯 갱신
    }

    //슬롯이동/교환기능
    public bool SwitchSlotdata(int Slot1, int Slot2) //Slot1 : 드래그드롭한 슬롯의 슬롯넘버, Slot2 : 대상슬롯넘버, 빈슬롯은 드래그하지못하게 설계 //슬롯데이터변경에성공하면 true 실패하면 false
    {
        if (0 > Slot1 || Slot1 >= GetInventorySlotNumber || 0 > Slot2|| Slot2 >= GetInventorySlotNumber) //만약존재할수없는 슬롯번호가 들어오면 실패반환
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

    //외부에서 인벤토리내 슬롯정보를 가져가기위한 함수들
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
        
        Debug.LogError("Error! 슬롯번호가 최대슬롯수치보다 클수없습니다");
        return false;
    }

    private void UpdateSlotUI(int slotnumber) //슬롯UI갱신
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
            Debug.LogError("플레이어 인벤토리 세이브파일을 찾지못했습니다.");
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
