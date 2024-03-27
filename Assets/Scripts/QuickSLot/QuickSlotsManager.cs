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
    [SerializeField] private List<QuickSlot> QuickSlots; //�����Ե�

    public int GetQuickSlotsCount()
    {
        return QuickSlots.Count;
    }
    public QuickSlot GetQuickSlotData(int SlotNumber)
    {
        return QuickSlots[SlotNumber];
    }

    public void SetQuickSlotData(int SlotNumber, DRAGGEDOBJECTTYPE type, int OriginSlotNumber) //��ų,�κ��丮���� ������ ������ ������ �������� �����ͼ���
    {
        QuickSlots[SlotNumber].SetSlotData(type, OriginSlotNumber);
    }

    public void SetQuickSlotToolTipText(int SlotNumber, string ToolTipText) //��ų���뺯���
    {
        QuickSlots[SlotNumber].SetSlotToolTipText(ToolTipText);
    }

    public void SetQuickSlotQuantityText(int SlotNumber, string Quantity) //�����۰������������ 
    {
        QuickSlots[SlotNumber].SetSlotQuantityText(Quantity);
    }

    public void SetQuickSlotOriginSlotNumber(int SlotNumber, int NewOriginNumber) //�κ��丮�� ��������ġ �̵���
    {
        QuickSlots[SlotNumber].ChangeOriginSlotNumber(NewOriginNumber);
    }

    public void ClearQuickSlot(int SlotNumber) //�������ǹ�ȣ���޾ƿͼ� �ش������Ե����ͺ��
    {
        QuickSlots[SlotNumber].Clear();
    }

    public int GetQuickSlotNumberByOriginNumber(int OriginNumber, DRAGGEDOBJECTTYPE type) //�������ǹ�ȣ�� ��ġ�� �����Կ���ϵ� �������Թ�ȣ�� ���Թ�ȣ�� ������� ã�ƿ���
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

    public void SwitchQuickSlotData(int From, int Target) //���۵Ƚ����ǹ�ȣ, Ÿ�ٽ����ǹ�ȣ�� �޾ƿ� �� ������ �������� ����
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
            Debug.LogError("������ ���̺������� ã�����߽��ϴ�.");
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
