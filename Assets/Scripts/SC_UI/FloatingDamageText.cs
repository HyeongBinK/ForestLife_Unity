using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private Text m_Text;
    public event Action TextDisable;
    private void Awake()
    {
        TextDisable = null;
    }
    public void OnDisableEvent()
    {
        if (TextDisable != null)
        {
            TextDisable();
            TextDisable = null;
        }

        gameObject.SetActive(false);
    }
    public void SetText(string NewText)
    {
        m_Text.text = NewText;
        if(!gameObject.activeSelf)
        gameObject.SetActive(true);
        StartCoroutine(FloatDamage());
    }
    /*public void HideText() 
    {
        gameObject.SetActive(false);
    }*/
  /*  public void ActiveText()
    {
        gameObject.SetActive(true);
    }*/

    public void SetPos(Vector2 pos)
    {
        transform.position = pos;
    }

    IEnumerator FloatDamage()
    {
        yield return new WaitForSeconds(1.0f);
        gameObject.SetActive(false);
    }


}
