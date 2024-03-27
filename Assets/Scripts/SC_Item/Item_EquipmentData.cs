using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EQUIPMENT_TYPE
{
    NONE = 0,
    WEAPON,
    HEAD,
    ARMOR,
    SHOES,
    GLOVES,
    RING,
    END
}

[CreateAssetMenu(fileName = "EquipmentData", menuName = "ScriptableObjects/Equipment/Data", order = 1)]
public class Item_EquipmentData : ScriptableObject
{
    public int ItemNum; //아이템고유번호
    public EQUIPMENT_TYPE Type;
    public Status Min_Status = new Status();
    public Status Max_Status = new Status();
    public int Max_ReinforceNum; //최대강화가능횟수
}

