using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StrPlusButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData) //���콺�� ������(����)�� ��������
    {
        Player.instance.GetPlayerStatus.AddStr();
    }
}
