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
    public int ItemNum; //�����۰�����ȣ
    public string ItemName; //�������̸�
    public string ImageName; //�������̹����̸�(�̹����ؽ�Ʈ�ҷ��ÿ뵵)
    public ITEM_TYPE ItemType; //(�������� ����)
    public int Price; //���Ž��ǰ���(�Ǹ��ҋ��� �̰��� 1/10)
    public string Text; //���������� ���콺Ŀ�� �ø��� ǥ���� �����ؽ�Ʈ
}
