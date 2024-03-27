using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NpcChatCloud : MonoBehaviour
{
    [SerializeField] private Text m_TextBox;

    public void SetChatCloud(float ScreenX, float ScreenY) //(string NewChat, float ScreenX, float ScreenY)
    {
        //m_TextBox.text = NewChat;
        gameObject.transform.position = new Vector2(ScreenX, ScreenY);
        if(!gameObject.activeSelf)
        gameObject.SetActive(true);
        // StartCoroutine(After3SecDisappear());
    }

    /*IEnumerator After3SecDisappear()
    {
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }*/
}
    
