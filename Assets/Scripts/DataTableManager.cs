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

    private Dictionary<string, SkillData> SkillDataTable = new Dictionary<string, SkillData>(); //Key : 스킬명, Value : 스킬데이터
    public Dictionary<string, SkillData> GetSkillDataTable { get { return SkillDataTable; } } //통쨰로가져가서 사용할떄 쓸려고만든함수(안쓸수있음)
    public Dictionary<int, string> SkillUniqueNumberToSkillnName = new Dictionary<int, string>(); //스킬번호를 받아 스킬이름을 반환
    private Dictionary<int, ItemData> ItemDataTable = new Dictionary<int, ItemData>(); //Key : 아이템고유번호, Value : 아이템데이터
    private Dictionary<int, Item_ConsumptionData> ConsumptionDataTable = new Dictionary<int, Item_ConsumptionData>(); //Key : 아이템고유번호, Value : 아이템데이터
    private Dictionary<int, Item_EquipmentData> EquipmentDataTable = new Dictionary<int, Item_EquipmentData>(); ////Key : 아이템고유번호, Value : 장비아이템데이터
    
    private void Awake()
    {
        // 씬에 싱글톤 오브젝트가 된 다른 DataTableManager 오브젝트가 있다면
        if (instance != this)
        {
            // 자신을 파괴
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

        Debug.Log("에러위치 : 데이터테이블매니저-GetSkillMPCost, 스킬이름정보없음");
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
        NewToolTipText.AppendLine(SkillData.SkillName); //스킬이름
        NewToolTipText.Append("Type : ");
        NewToolTipText.AppendLine(SkillData.SkillType.ToString()); //스킬의 타입(액티브인지 패시브인지)
        NewToolTipText.Append("CurrentLevel/SkillMaxLevel : ");
        NewToolTipText.AppendLine(MakeSkillLevelAndMaxLevelText(PlayerSkillLevel, SkillData.SkillMaxLevel)); ; //스킬의 현재레벨과 최대레벨
        NewToolTipText.Append("MPCost : ");
        NewToolTipText.AppendLine(SkillData.MPCost.ToString()); //MPCost(마나 소모량)

        switch (SkillData.SkillType) //타입 SKILLTYPE
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
                Debug.Log("스킬타입정보가없음(위치 : 스킬슬롯)");
                break;
        }
        NewToolTipText.AppendLine((SkillData.DammagePercent + (PlayerSkillLevel * SkillData.IncreasePerLevel)).ToString());

        NewToolTipText.Append("Level Per Increase : ");
        NewToolTipText.AppendLine(SkillData.IncreasePerLevel.ToString()); //레벨당 데미지/효과 상승량

        NewToolTipText.Append("Discription : ");
        NewToolTipText.AppendLine(SkillData.SkillDiscription); //스킬설명

        return NewToolTipText.ToString();
    }
}
