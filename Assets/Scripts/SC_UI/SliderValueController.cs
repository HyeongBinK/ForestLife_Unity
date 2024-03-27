using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueController : MonoBehaviour
{
    [SerializeField] private Slider Bar;
    [SerializeField] private Text text;

    public void Init(int CurValue, int MaxValue)
    {
        Bar.maxValue = MaxValue;
        Bar.value = CurValue;
    }
    public void HideSlider() //��ũ������带 ����ÿ� �����̴��� ����� ���� ������
    {
        gameObject.SetActive(false); 
    }
    public void ActiveSlider() 
    {
        gameObject.SetActive(true);
    }
    public void SetText(int value) // �����ġ/�ִ��ġ
    {
        Bar.value = value;
        text.text = string.Format("{0} / {1}", value, Bar.maxValue);
    }
  
    public void SetPerText(int value) //�����ġ % -> ����ġ �����̴����� ���
    {
        Bar.value = value;
        text.text = string.Format("{0:P}", (float)(value / Bar.maxValue));
    }


}
