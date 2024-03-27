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
    public void HideSlider() //스크린샷모드를 만들시에 슬라이더를 숨기기 위해 만들어둠
    {
        gameObject.SetActive(false); 
    }
    public void ActiveSlider() 
    {
        gameObject.SetActive(true);
    }
    public void SetText(int value) // 현재수치/최대수치
    {
        Bar.value = value;
        text.text = string.Format("{0} / {1}", value, Bar.maxValue);
    }
  
    public void SetPerText(int value) //현재수치 % -> 경험치 슬라이더에서 사용
    {
        Bar.value = value;
        text.text = string.Format("{0:P}", (float)(value / Bar.maxValue));
    }


}
