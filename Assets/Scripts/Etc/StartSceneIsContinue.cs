using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartSceneIsContinue : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool m_bIsContinue = false; //ù ����ȭ�鿡�� ��Ƽ��(�̾��ϱ�)�� �������� Ʈ��
}
