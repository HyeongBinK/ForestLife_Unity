using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class DataTableManager : MonoBehaviour
{
    private static DataTableManager m_instance;
    public static DataTableManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<DataTableManager>();
                m_instance.LoadSkillData();
                m_instance.LoadItemData();
                m_instance.LoadConsumptionItemData();
                m_instance.MakeEquipmentItemDictionary();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    private Dictionary<string, SkillData> SkillDataTable = new Dictionary<string, SkillData>(); //Key : ��ų��, Value : ��ų������
    public Dictionary<string, SkillData> GetSkillDataTable { get { return SkillDataTable; } } //�뤊�ΰ������� ����ҋ� ���������Լ�(�Ⱦ�������)
    public Dictionary<int, string> SkillUniqueNumberToSkillnName = new Dictionary<int, string>(); //��ų��ȣ�� �޾� ��ų�̸��� ��ȯ
    private Dictionary<int, ItemData> ItemDataTable = new Dictionary<int, ItemData>(); //Key : �����۰�����ȣ, Value : �����۵�����
    private Dictionary<int, Item_ConsumptionData> ConsumptionDataTable = new Dictionary<int, Item_ConsumptionData>(); //Key : �����۰�����ȣ, Value : �����۵�����
    private Dictionary<int, Item_EquipmentData> EquipmentDataTable = new Dictionary<int, Item_EquipmentData>(); ////Key : �����۰�����ȣ, Value : �������۵�����
    
    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� DataTableManager ������Ʈ�� �ִٸ�
        if (instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }

    private void LoadSkillData()
    {
        var SkillData = CSV.Read("DataTable/SkillDataTable");
        for (int i = 0; i < SkillData.Count; i++)
        {
            var data = new SkillData();
            data.SkillName = SkillData[i]["SkillName"];
            if (int.TryParse(SkillData[i]["SkillType"], out int type))
            {
                data.SkillType = (SKILLTYPE)type;
            }
            int.TryParse(SkillData[i]["EquiredLevel"], out data.EquiredLevel);
            int.TryParse(SkillData[i]["SkillMaxLevel"], out data.SkillMaxLevel);
            int.TryParse(SkillData[i]["MPCost"], out data.MPCost);
            float.TryParse(SkillData[i]["DammagePercent"], out data.DammagePercent);
            float.TryParse(SkillData[i]["IncreasePerLevel"], out data.IncreasePerLevel);
            int.TryParse(SkillData[i]["SkillNumber"], out data.SkillNumber);
            
            if (int.TryParse(SkillData[i]["EffectType"], out type))
            {
                data.EffectType = (EFFECTTYPE)type;
            }
            data.SkillDiscription = SkillData[i]["SkillDiscription"];
            data.SkillImageFileName = SkillData[i]["SkillImageFileName"];

            SkillDataTable.Add(data.SkillName, data);
            SkillUniqueNumberToSkillnName.Add(data.SkillNumber, data.SkillName);
        }
    }

    private void LoadItemData()
    {
        List<ItemData> itemDatas = XML<ItemData>.Read("DataTable/ItemDataTable");
        for(int i =0; i < itemDatas.Count; i++)
        {
            ItemDataTable.Add(itemDatas[i].ItemNum, itemDatas[i]);
        }
    }

    private void MakeEquipmentItemDictionary()
    {
        Item_EquipmentData[] item_EquipmentDatas = Resources.LoadAll<Item_EquipmentData>("DataTable/EquipmentData");
        
        for(int i = 0; i < item_EquipmentDatas.Length; i++)
        {
            EquipmentDataTable.Add(item_EquipmentDatas[i].ItemNum, item_EquipmentDatas[i]);
        }
    }

    private void LoadConsumptionItemData()
    {
        List<Item_ConsumptionData> ConsumptionDatas = XML<Item_ConsumptionData>.Read("DataTable/ConsumptionDataTable");
        for (int i = 0; i < ConsumptionDatas.Count; i++)
        {
            ConsumptionDataTable.Add(ConsumptionDatas[i].ItemNum, ConsumptionDatas[i]);
        }
    }

    public SkillData GetSkillData(string SkillName)
    {
        if (SkillDataTable[SkillName].SkillName != null)
            return SkillDataTable[SkillName];

        return new SkillData();
    }

    public int GetSkillMPCost(string SkillName)
    {
        if (SkillDataTable[SkillName].SkillName != null)
            return SkillDataTable[SkillName].MPCost;

        Debug.Log("������ġ : ���������̺�Ŵ���-GetSkillMPCost, ��ų�̸���������");
        return 999;
    }

    public ItemData GetItemData(int ItemUniqueNumber)
    {
        if (ItemDataTable[ItemUniqueNumber] != null)
            return ItemDataTable[ItemUniqueNumber];

        return null;
    }

    public Item_EquipmentData GetEquipmentItemData(int ItemUniqueNumber)
    {
        if (EquipmentDataTable[ItemUniqueNumber] != null)
            return EquipmentDataTable[ItemUniqueNumber];

        return null;
    }

    public Item_ConsumptionData GetConsumptionData(int ItemUniqueNumber)
    {
        if (ConsumptionDataTable[ItemUniqueNumber] != null)
            return ConsumptionDataTable[ItemUniqueNumber];

        return null;
    }

    public string GetSkillNameBySkillUniqueNumber(int SkillUniqueNumber)
    {
        return SkillUniqueNumberToSkillnName[SkillUniqueNumber];
    }


    public int GetActiveSkillDamagePercent(string SkillName)
    {
        if (GameManager.instance)
        {
            if (SkillDataTable.ContainsKey(SkillName))
            {
                int DamagePercent = (int)SkillDataTable[SkillName].GetValue(Player.instance.GetPlayerStatus.GetSkillLevelByName(SkillName));
                return DamagePercent;
            }
        }
        
        return 0;
    }

    public string MakeSkillLevelAndMaxLevelText(int CurSkillLevel, int MaxSkillLevel)
    {
        StringBuilder SkillLevelAndMaxLevelText = new StringBuilder();
        SkillLevelAndMaxLevelText.Append(CurSkillLevel.ToString());
        SkillLevelAndMaxLevelText.Append(" / ");
        SkillLevelAndMaxLevelText.Append(MaxSkillLevel);
        return SkillLevelAndMaxLevelText.ToString();
    }

    public string MakeSkillToolTip(int SkillUniqueNumber, int PlayerSkillLevel)
    {
        StringBuilder NewToolTipText = new StringBuilder();
        var SkillData = SkillDataTable[SkillUniqueNumberToSkillnName[SkillUniqueNumber]];
        NewToolTipText.Append("SkillName : ");
        NewToolTipText.AppendLine(SkillData.SkillName); //��ų�̸�
        NewToolTipText.Append("Type : ");
        NewToolTipText.AppendLine(SkillData.SkillType.ToString()); //��ų�� Ÿ��(��Ƽ������ �нú�����)
        NewToolTipText.Append("CurrentLevel/SkillMaxLevel : ");
        NewToolTipText.AppendLine(MakeSkillLevelAndMaxLevelText(PlayerSkillLevel, SkillData.SkillMaxLevel)); ; //��ų�� ���緹���� �ִ뷹��
        NewToolTipText.Append("MPCost : ");
        NewToolTipText.AppendLine(SkillData.MPCost.ToString()); //MPCost(���� �Ҹ�)

        switch (SkillData.SkillType) //Ÿ�� SKILLTYPE
        {
            case SKILLTYPE.SKILLVALUE_ACTIVE:
                {
                    NewToolTipText.Append("DammagePercent : ");
                }
                break;
            case SKILLTYPE.SKILLVALUE_PASSIVE:
                {
                    NewToolTipText.Append("Effect : ");
                }
                break;
            default:
                Debug.Log("��ųŸ������������(��ġ : ��ų����)");
                break;
        }
        NewToolTipText.AppendLine((SkillData.DammagePercent + (PlayerSkillLevel * SkillData.IncreasePerLevel)).ToString());

        NewToolTipText.Append("Level Per Increase : ");
        NewToolTipText.AppendLine(SkillData.IncreasePerLevel.ToString()); //������ ������/ȿ�� ��·�

        NewToolTipText.Append("Discription : ");
        NewToolTipText.AppendLine(SkillData.SkillDiscription); //��ų����

        return NewToolTipText.ToString();
    }
}
