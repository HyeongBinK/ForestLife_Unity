using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;

[System.Serializable]
public struct EquipmentSaveData
{
    public int SlotNumber; //슬롯번호
    public SlotEquipmentData EquipmentData;
}

[System.Serializable]
public class EquipmentsSaveData
{
    public List<EquipmentSaveData> EquipmentSaveDatas = new List<EquipmentSaveData>();
}

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private List<EquipmentSlot> Equipments; //장비중인 아이템리스트
    private Status TotalEquipmentStatus = new Status(); //장비중인 아이템들의 스테이터스합계
    public Status GetTotalEquipmentStatus { get { return TotalEquipmentStatus; } } //외부에서 아이템들의 스테이터스합계정보를 불러와야할떄 보내주는 함수
    
    //무기 아이템 장착시 플레이어의 외형변경을 위한 오브젝트들 전부 캐릭터 위에 얹어두고 활성화/비활성화 하는 방식 
    [SerializeField] private GameObject SwordObject;
    [SerializeField] private GameObject CutlassObject;
    [SerializeField] private GameObject ClaymoreObject;
    [SerializeField] private GameObject EternalSwordObject;


    private void Awake()
    {
        for(int i=0; i < Equipments.Count; i++)
        {
            Equipments[i].DefaultSetting();
        }

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }


    public bool EquipItem(SlotEquipmentData EquipmentItem, string TooltipText) //아이템의 장착처리
    {
        for(int i =0; i < Equipments.Count; i++)
        {
            if(Equipments[i].GetSlotNumber == (int)EquipmentItem.Type) //슬롯의 번호와 타입의 번호를 같아지게끔 설정해두었습니다.
            {
                Equipments[i].SetSlotData(EquipmentItem, TooltipText); 
                SoundManager.Instance.PlayPickUpSound(); //장비장착 효과음 재생
                UpdateTotalEquipmentStatus(); //장착한 아이템들의 합계능력치 변경
                UpdataPlayerWeaponObject(); //만약 장착한 장비가 무기었을시 외형변경
                return true;
            }
        }

        GameManager.instance.AddNewLog("해당아이템의 파츠(부위)가 불정확합니다. 코드를 검토하세요");
        return false;
    }

    public void UpdateTotalEquipmentStatus()
    {
        TotalEquipmentStatus = CalculateTotalEquipmentStatus();
    }

    public void UpdataPlayerWeaponObject() //플레이어 장착장비에 따라 장비외형변경
    {
        var SlotData = Equipments[0];
        if (SlotData.GetSlotData.Type == EQUIPMENT_TYPE.WEAPON)
        {
            SwordObject.gameObject.SetActive(false);
            CutlassObject.gameObject.SetActive(false);
            ClaymoreObject.gameObject.SetActive(false);
            EternalSwordObject.gameObject.SetActive(false);

            if (!SlotData.GetIsData)
            {
                SwordObject.gameObject.SetActive(true);
                return;
            }

            switch (SlotData.GetSlotData.ItemNumber)
            {
                case 20:
                    {
                        SwordObject.gameObject.SetActive(true);
                    }
                    break;
                case 21:
                    {
                        CutlassObject.gameObject.SetActive(true);
                    }
                    break;
                case 22:
                    {
                        ClaymoreObject.gameObject.SetActive(true);
                    }
                    break;
                case 23:
                    {
                        EternalSwordObject.gameObject.SetActive(true);
                    }
                    break;
                default:
                    return;

            }
        }
    }
    //장착장비의 총스테이터스합 계산
    public Status CalculateTotalEquipmentStatus()
    {
        Status NewStatus = new Status();

        for (int i = 0; i < Equipments.Count; i++)
        {
            if (Equipments[i].GetSlotData.ItemNumber != -1)
            {
                NewStatus.MaxHP += Equipments[i].GetSlotData.ItemStatus.MaxHP;
                NewStatus.MaxMP += Equipments[i].GetSlotData.ItemStatus.MaxMP;
                NewStatus.Atk += Equipments[i].GetSlotData.ItemStatus.Atk;
                NewStatus.Def += Equipments[i].GetSlotData.ItemStatus.Def;
                NewStatus.Str += Equipments[i].GetSlotData.ItemStatus.Str;
                NewStatus.Dex += Equipments[i].GetSlotData.ItemStatus.Dex;
                NewStatus.Health += Equipments[i].GetSlotData.ItemStatus.Health;
                NewStatus.Int += Equipments[i].GetSlotData.ItemStatus.Int;
                NewStatus.Critical += Equipments[i].GetSlotData.ItemStatus.Critical;
            }
        }

        return NewStatus;
    }

    public void SaveData()
    {
        EquipmentsSaveData SaveData = new EquipmentsSaveData();

        foreach (EquipmentSlot Slot in Equipments)
        {
            EquipmentSaveData data;
            data.SlotNumber = Slot.GetSlotNumber;
            data.EquipmentData = Slot.GetSlotData;
            SaveData.EquipmentSaveDatas.Add(data);
        }
        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("EquipmentsData", "/SaveData/EquipmentData/"), json);
    }

    public bool LoadData()
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath("EquipmentsData", "/SaveData/EquipmentData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("플레이어 장비 세이브파일을 찾지못했습니다.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        EquipmentsSaveData SaveData = JsonUtility.FromJson<EquipmentsSaveData>(SaveFile);
        var EquipmentsData = SaveData.EquipmentSaveDatas;
        
        
        for (int i =0; i < Equipments.Count; i++)
        {
            Equipments[i].ClearSlotData();
            if (EquipmentsData[i].SlotNumber == Equipments[i].GetSlotNumber)
            {
                if(EquipmentsData[i].EquipmentData.ItemNumber != -1)
                Equipments[i].SetSlotData(EquipmentsData[i].EquipmentData, ItemManager.instance.MakeItemToolTipText(-1, EquipmentsData[i].EquipmentData));
            }
        }

        UpdataPlayerWeaponObject();
        return true;
    }
}
