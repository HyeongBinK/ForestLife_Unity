using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class M_HPBar : MonoBehaviour //몬스터의체력바 M(Monster)
{
   [SerializeField] private Slider Bar;
    public event Action HPBarDisable;
    public void SetSlider(int MaxHP)
    {
        if (!Bar) return; 
        Bar.maxValue = MaxHP;
        Bar.value = MaxHP;
    }
    public void SetValue(int CurHP)
    {
        if (!Bar) return;
        Bar.value = CurHP;
    }

    private void Awake()
    {
        HPBarDisable = null;
    }
    public void OnDisableEvent()
    {
        if (HPBarDisable != null)
        {
            HPBarDisable();
            HPBarDisable = null;
        }
        gameObject.SetActive(false);
    }

    public void SetPos(Vector2 pos)
    {
        transform.position = pos;
    }
}
