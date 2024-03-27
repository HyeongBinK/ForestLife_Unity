using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HEALTYPE
{ 
    NONE =0,
    HP,
    MP,
    EXP
}

public class Item_ConsumptionData
{
    public int ItemNum;
    public HEALTYPE Type;
    public int Value;
}
