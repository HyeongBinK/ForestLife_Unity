using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DexPlusButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        Player.instance.GetPlayerStatus.AddDex();
    }
}