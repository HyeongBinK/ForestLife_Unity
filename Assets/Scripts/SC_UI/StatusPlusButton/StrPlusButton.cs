using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StrPlusButton : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData) //마우스가 포인터(슬롯)을 눌럿을떄
    {
        Player.instance.GetPlayerStatus.AddStr();
    }
}
