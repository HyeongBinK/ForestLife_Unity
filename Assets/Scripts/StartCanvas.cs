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

    public void NewGame() //���Ӹ���,ó������ 
    {
        gameObject.SetActive(false);
    }

    public void ExitGame() //����(���ø����̼�) ����
    {
        Application.Quit();
    }

    public void LoadGame() //���Ӻҷ�����
    {
        if (DataSaveAndLoad.Instance.LoadGame())
        {
            gameObject.SetActive(false);
        }
    }

}
