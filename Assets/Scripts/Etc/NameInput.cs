using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
    [SerializeField] private InputField m_InputField;

    public void SetScreenPosition(float ScreenX, float ScreenY)
    {
        gameObject.SetActive(true);
        gameObject.transform.position = new Vector2(ScreenX, ScreenY);
    }
    public void SetData()
    {
        m_InputField.text = "";
    }

    public void InputApply(InputField inputField)
    {
        Player.instance.GetPlayerStatus.SetName(inputField.text);
        gameObject.SetActive(false);
    }
    public void InputChangeValue(InputField inputField)
    {
        if (0 >= inputField.text.Length) return;
    }
}
