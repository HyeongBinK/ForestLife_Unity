using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private Button ContinueButton; //Ŭ���� �޴�â�� �ݰ� ���� �̾��ϱ�
    [SerializeField] private Button DataLoadButton; //�޴�â�� ������ �����ͷε�
    [SerializeField] private Button ExitButton; //Ŭ���� ������ ��������

    private void Awake()
    {
        ContinueButton.onClick.AddListener(DisActiveMenuUI);
        DataLoadButton.onClick.AddListener(ClickDataLoadButtonEvent);
        ExitButton.onClick.AddListener(Application.Quit);
    }

    public void DisActiveMenuUI()
    {
        gameObject.SetActive(false);
    }

    public void ClickDataLoadButtonEvent()
    {
        DataSaveAndLoad.Instance.LoadGame();
        gameObject.SetActive(false);
    }



}
