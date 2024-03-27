using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct QuickSlotData
{
    public int SlotNumber;
    public DRAGGEDOBJECTTYPE draggedtype;
    public int OriginerSlotNumber;
    public string Key;
}

[System.Serializable]
public class QuickSlotDatas
{
    public List<QuickSlotData> QuickSlotsData = new List<QuickSlotData>();
}

public class QuickSlotsManager : MonoBehaviour
{
    [SerializeField] private List<QuickSlot> QuickSlots; //퀵슬롯들

    public int GetQuickSlotsCount()
    {
        return QuickSlots.Count;
    }
    public QuickSlot GetQuickSlotData(int SlotNumber)
    {
        return QuickSlots[SlotNumber];
    }

    public void SetQuickSlotData(int SlotNumber, DRAGGEDOBJECTTYPE type, int OriginSlotNumber) //스킬,인벤토리에서 정보가 왔을때 최초의 퀵슬롯의 데이터셋팅
    {
        QuickSlots[SlotNumber].SetSlotData(type, OriginSlotNumber);
    }

    public void SetQuickSlotToolTipText(int SlotNumber, string ToolTipText) //스킬레밸변경시
    {
        QuickSlots[SlotNumber].SetSlotToolTipText(ToolTipText);
    }

    public void SetQuickSlotQuantityText(int SlotNumber, string Quantity) //아이템갯수정보변경시 
    {
        QuickSlots[SlotNumber].SetSlotQuantityText(Quantity);
    }

    public void SetQuickSlotOriginSlotNumber(int SlotNumber, int NewOriginNumber) //인벤토리내 아이템위치 이동시
    {
        QuickSlots[SlotNumber].ChangeOriginSlotNumber(NewOriginNumber);
    }

    public void ClearQuickSlot(int SlotNumber) //퀵슬롯의번호를받아와서 해당퀵슬롯데이터비움
    {
        QuickSlots[SlotNumber].Clear();
    }

    public int GetQuickSlotNumberByOriginNumber(int OriginNumber, DRAGGEDOBJECTTYPE type) //퀵슬롯의번호를 위치와 퀵슬롯에등록된 기존슬롯번호와 슬롯번호를 기반으로 찾아오기
    {
        for(int i=0; i < QuickSlots.Count; i++)
        {
            if (type == QuickSlots[i].m_type)
            {
                if (QuickSlots[i].m_OriginSlotNumber == OriginNumber)
                {
                    return QuickSlots[i].GetQuickSlotUniqueNumber;
                }
            }
        }
        return -1;
    }

    public void SwitchQuickSlotData(int From, int Target) //시작된슬롯의번호, 타겟슬롯의번호를 받아와 두 퀵슬롯 데이터의 변경
    {
        if (QuickSlots[Target].m_IsData)
        {
            var TargetSlotData = QuickSlots[Target];
            DRAGGEDOBJECTTYPE temptype = TargetSlotData.m_type;
            int tempSlotNumber = TargetSlotData.m_OriginSlotNumber;

            QuickSlots[Target].SetSlotData(QuickSlots[From].m_type, QuickSlots[From].m_OriginSlotNumber);
            QuickSlots[From].SetSlotData(temptype, tempSlotNumber);
        }
        else
        {
            QuickSlots[Target].SetSlotData(QuickSlots[From].m_type, QuickSlots[From].m_OriginSlotNumber);
            QuickSlots[From].Clear();
        }
    }

    public string GetQuickSlotKey(int QuickSlotNumber)
    {
        return QuickSlots[QuickSlotNumber].Key;
    }

    public void SaveQuickSlotsData()
    {
        QuickSlotDatas SaveData = new QuickSlotDatas();
        foreach(QuickSlot quickslot in QuickSlots)
        {
            QuickSlotData Data;
            Data.SlotNumber = quickslot.GetQuickSlotUniqueNumber;
            Data.draggedtype = quickslot.m_type; 
            Data.OriginerSlotNumber = quickslot.m_OriginSlotNumber;
            Data.Key = quickslot.Key;
            SaveData.QuickSlotsData.Add(Data);
        }

        string json = JsonUtility.ToJson(SaveData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath("QuickSlotsData", "/SaveData/QuickSlotData/"), json);
    }

    public bool LoadQuickSlotData()
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath("QuickSlotsData", "/SaveData/QuickSlotData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("퀵슬롯 세이브파일을 찾지못했습니다.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        QuickSlotDatas SaveData = JsonUtility.FromJson<QuickSlotDatas>(SaveFile);
        var QuickSlotDatas = SaveData.QuickSlotsData;


        for (int i = 0; i < QuickSlotDatas.Count; i++)
        {
            if (QuickSlotDatas[i].SlotNumber == QuickSlots[i].GetQuickSlotUniqueNumber)
            {
                QuickSlots[i].Clear();

                if (QuickSlotDatas[i].draggedtype != DRAGGEDOBJECTTYPE.NONE)
                    QuickSlots[i].SetSlotData(QuickSlotDatas[i].draggedtype, QuickSlotDatas[i].OriginerSlotNumber);
            }
        }
        return true;
    }
}
