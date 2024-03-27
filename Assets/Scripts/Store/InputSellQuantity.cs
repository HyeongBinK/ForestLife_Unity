using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputSellQuantity : MonoBehaviour
{
    [SerializeField] private Text m_CurrentQuantityText;
    [SerializeField] private InputField m_InputField;
    private int m_OriginSlotNumber = -1;
    private int m_OriginSlotQuantity = 0;
 
    public void SetScreenPosition(float ScreenX, float ScreenY)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = new Vector2(ScreenX, ScreenY);
    }
    public void SetData(int NewNumber, int NewQuantity)
    {
        m_InputField.text = "";
        m_OriginSlotNumber = NewNumber;
        m_OriginSlotQuantity = NewQuantity;
        m_CurrentQuantityText.text = NewQuantity.ToString();
    }

    public void InputApply(InputField inputField)
    {
        //inputField.text
        if(int.TryParse(inputField.text, out int value))
        {
            if(m_OriginSlotQuantity >= value)
            {
                Player.instance.SellItem(m_OriginSlotNumber, value);
                gameObject.SetActive(false);
            }
            else 
            GameManager.instance.AddNewLog("보유갯수보다 많이 팔수없습니다");
        }
        else 
        GameManager.instance.AddNewLog("숫자만입력하세요!");
    }

    public void InputChangeValue(InputField inputField)
    {
        if (0 >= inputField.text.Length) return;

        var a = inputField.text[inputField.text.Length - 1];
        if(!int.TryParse(a.ToString(), out int value))
        {
            inputField.text = "";// = inputField.text.Substring(0, inputField.text.Length - 1);
        }
    }
}
