using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartSceneCode : MonoBehaviour
{
    [SerializeField] private Button NewGameButton;
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private StartSceneIsContinue IsContinue;

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

    public void NewGame() //���Ӹ���,ó������ 
    {
        SceneManager.LoadScene("Town");
    }

    public void ExitGame() //����(���ø����̼�) ����
    {
        Application.Quit();
    }

    public void LoadGame() //���Ӻҷ�����
    {
        IsContinue.m_bIsContinue = true;
        SceneManager.LoadScene("Town");
    }

}
