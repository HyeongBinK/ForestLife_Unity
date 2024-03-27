using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneIsContinue : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool m_bIsContinue = false; //첫 시작화면에서 컨티뉴(이어하기)를 눌럿을시 트루
}
