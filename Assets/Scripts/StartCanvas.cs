using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartCanvas : MonoBehaviour
{
    [SerializeField] private Button NewGameButton;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button ExitButton;

    private void Awake()
    {
        ButtonConnecter();
    }

    public void ButtonConnecter()
    {
        NewGameButton.onClick.AddListener(NewGame);
        ContinueButton.onClick.AddListener(LoadGame);
        ExitButton.onClick.AddListener(ExitGame);
    }

    public void NewGame() //게임리셋,처음시작 
    {
        gameObject.SetActive(false);
    }

    public void ExitGame() //게임(어플리케이션) 종료
    {
        Application.Quit();
    }

    public void LoadGame() //게임불러오기
    {
        if (DataSaveAndLoad.Instance.LoadGame())
        {
            gameObject.SetActive(false);
        }
    }

}
