using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;

[System.Serializable]
public struct EquipmentSaveData
{
    public int SlotNumber; //���Թ�ȣ
    public SlotEquipmentData EquipmentData;
}

[System.Serializable]
public class EquipmentsSaveData
{
    public List<EquipmentSaveData> EquipmentSaveDatas = new List<EquipmentSaveData>();
}

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private List<EquipmentSlot> Equipments; //������� �����۸���Ʈ
    private Status TotalEquipmentStatus = new Status(); //������� �����۵��� �������ͽ��հ�
    public Status GetTotalEquipmentStatus { get { return TotalEquipmentStatus; } } //�ܺο��� �����۵��� �������ͽ��հ������� �ҷ��;��ҋ� �����ִ� �Լ�
    
    //���� ������ ������ �÷��̾��� ���������� ���� ������Ʈ�� ���� ĳ���� ���� ���ΰ� Ȱ��ȭ/��Ȱ��ȭ �ϴ� ��� 
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


    public bool EquipItem(SlotEquipmentData EquipmentItem, string TooltipText) //�������� ����ó��
    {
        for(int i =0; i < Equipments.Count; i++)
        {
            if(Equipments[i].GetSlotNumber == (int)EquipmentItem.Type) //������ ��ȣ�� Ÿ���� ��ȣ�� �������Բ� �����صξ����ϴ�.
            {
                Equipments[i].SetSlotData(EquipmentItem, TooltipText); 
                SoundManager.Instance.PlayPickUpSound(); //������� ȿ���� ���
                UpdateTotalEquipmentStatus(); //������ �����۵��� �հ�ɷ�ġ ����
                UpdataPlayerWeaponObject(); //���� ������ ��� ��������� ��������
                return true;
            }
        }

        GameManager.instance.AddNewLog("�ش�������� ����(����)�� ����Ȯ�մϴ�. �ڵ带 �����ϼ���");
        return false;
    }

    public void UpdateTotalEquipmentStatus()
    {
        TotalEquipmentStatus = CalculateTotalEquipmentStatus();
    }

    public void UpdataPlayerWeaponObject() //�÷��̾� ������� ���� ����������
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
    //��������� �ѽ������ͽ��� ���
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
            Debug.LogError("�÷��̾� ��� ���̺������� ã�����߽��ϴ�.");
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
