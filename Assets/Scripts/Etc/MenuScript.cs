using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    [SerializeField] private Button ContinueButton; //클릭시 메뉴창을 닫고 게임 이어하기
    [SerializeField] private Button DataLoadButton; //메뉴창을 닫으며 데이터로드
    [SerializeField] private Button ExitButton; //클릭시 게임을 완전종료

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
