using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ITEM_TYPE
{
    NONE = 0,
    EQUIPMENT,
    CONSUMPTION,
    ETC,
    SCROLL
}
public class ItemData
{
    public int ItemNum; //아이템고유번호
    public string ItemName; //아이템이름
    public string ImageName; //아이템이미지이름(이미지텍스트불러올용도)
    public ITEM_TYPE ItemType; //(아이템의 종류)
    public int Price; //구매시의가격(판매할떄는 이값의 1/10)
    public string Text; //아이템위에 마우스커서 올릴시 표시할 설명텍스트
}
