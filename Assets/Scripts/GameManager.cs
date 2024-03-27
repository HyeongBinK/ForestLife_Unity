using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance; // �̱����� �Ҵ�� ����
    private Vector3 StartPosition; //�ʿ� �÷��̾ ����(����,��ȯ) �������� ��ġ��ǥ
    private bool PlayerDeadFlag; //�÷��̾ ������ true �׿� false(���ӿ���üũ��//���ӿ����� �г�Ƽ�� �������� ��Ȱ)
    public GameObject playerobject;
    public Collider playercollider;
    public readonly int MaxDammage = 9999;
    public readonly int ErrorNumber = -1;
    static int CriticalDamagePercent = 200;
    public readonly float ItemMaintainTime = 30f; //�����۵���� �����Ǵ½ð�
    private const int MaxSystemMessageLine = 20; //�ý��۷α� �ִ�ǥ���, �Ѿ�� �Ǿ��� �α׺��� ���������� ������� ���α� �߰�
    private string m_prevSceneName;
    private string m_CurSceneName;
    private StartSceneIsContinue IsContinueObject;
    private List<string> SystemLogs = new List<string>();//UIManager�� �ý��۷α���¿� ���� �α׵�
    private bool IsKeyInput = true; //Ű�����Է��� �Ҽ��ִ°� �� ���� �Ҹ��� ����
    // �̱��� ���ٿ� ������Ƽ
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
                DontDestroyOnLoad(m_instance.gameObject);
                SceneManager.sceneLoaded += m_instance.LoadedScene;
            }
            return m_instance;
        }
    }
    public void SetPlayerNewPosition(Vector3 NewPosition)
    {
        StartPosition = NewPosition;
    }

    private void LoadedScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.isLoaded)
        {
            UIManager.Instance.GetFadeEffect.Fadein();
            StartCoroutine(DelayShowMapName());
            playerobject.SetActive(false);
            playerobject.transform.position = StartPosition; 
            playerobject.SetActive(true);
            Player.instance.GetPlayerStatus.UpdateUI();
            switch (scene.name)
            {
                case "BossField":
                case "Field1":
                case "Field2":
                case "Field3":
                    UIManager.Instance.MakeMonsetHpbarAndDamageText();
                    break;
                default :
                    break;
            }

            ItemManager.instance.ReadyToMakeItem();
            SoundManager.Instance.PlayBGM();
            ClearSystemLogs();
            AddNewLog(scene.name + "���� �̵��Ϸ�!");
            IsKeyInput = true;
            Player.instance.PlayerEndMoveScene();
            Player.instance.GetPlayerStatus.SetPlayerSpeed();
        }
    }

    public void MoveScene(string TargetScene)
    {
        UIManager.Instance.GetFadeEffect.FadeOut();
        if (MonsterSpawner.instance) //���ͽ����ʰ������� �ʵ峻 ��������
        {
            MonsterSpawner.instance.ClearField();
        }
        if(ItemManager.instance) //�����۸Ŵ����������� �ʵ峻 ����������
        {
            ItemManager.instance.ClearField();
        }

        IsKeyInput = false;
        Player.instance.PlayerStartMoveScene();
        UIManager.Instance.ResetMonsetHpbarAndDamageText(); //UiManager�� ���� ü�¹ٶ� �������ؽ��Ͱ������� ����
        UIManager.Instance.WhenSceneMoveOffActivatedUI();

        if (IsContinueObject)
        {
            if (IsContinueObject.m_bIsContinue)
            {
                SceneManager.LoadScene(TargetScene);
                return;
            }
        }
        StartCoroutine(DelayLoadScene(TargetScene));
        //SceneManager.LoadScene(TargetScene);
    }

    IEnumerator DelayLoadScene(string TargetScene)
    {
        yield return new WaitForSeconds(2.0f); ;
        SceneManager.LoadScene(TargetScene);
    }

    IEnumerator DelayShowMapName()
    {
        yield return new WaitForSeconds(1.5f);
        UIManager.Instance.ShowMapName();
    }

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }

        StartPosition = new Vector3(0, 0, -10);
    }

    private void Start()
    {
        IsContinueObject = FindObjectOfType<StartSceneIsContinue>();
        if (IsContinueObject)
        {
            if (IsContinueObject.m_bIsContinue)
            {
                DataSaveAndLoad.Instance.LoadGame();
                IsContinueObject.m_bIsContinue = false;
                return;
            }
        }
        else
            AddNewLog("����ȵ����͸� ã�� ���Ͽ� ó�����ͽ����մϴ�.");

        StartCoroutine(DelayActiveNameInputUI());
    }

    IEnumerator DelayActiveNameInputUI()
    {
        yield return new WaitForSeconds(3.0f);
        UIManager.Instance.GetNameInputUI.gameObject.SetActive(true);
    }

    public void AddNewLog(string NewText)
    {
        if(SystemLogs.Count > MaxSystemMessageLine)
        {
            SystemLogs.RemoveAt(0);
        }
        SystemLogs.Add(NewText);
        UIManager.Instance.SetSystemLog(SystemLogs);
    }

    public void ClearSystemLogs()
    {
        SystemLogs.Clear();
    }

    private void Update()
    {
        PlayerInputEvent();
    }

    public int DamageCalculate(string SkillName)
    {
        if (Player.instance)
        {
            int PlayerAtk = Player.instance.GetPlayerStatus.GetTotalStatus.Atk;
            int Damage = PlayerAtk;

            if (SkillName != "NormalAttack")
            {
                if (DataTableManager.instance)
                {
                    int SkillValue = DataTableManager.instance.GetActiveSkillDamagePercent(SkillName);
                    Damage = (int)(PlayerAtk * SkillValue * 0.01f);
                }
                else
                    Debug.LogError("���������̺���");
            }
            //AddNewLog(Damage.ToString());
            float CriticalRate = Player.instance.GetPlayerStatus.ExportTotalCiriticalRate();
            if (UnityEngine.Random.Range(0, 100) < CriticalRate)
                Damage = Mathf.Clamp(Damage * (CriticalDamagePercent / 100), 0, MaxDammage);
            return Damage;
        }
        else
            Debug.LogError("���Ӹ޴��� ����");

        return 0;
    }

    public void ReturnToHome() //��ȯ�ֹ������� �Ǵ� ����� ���
    {
        SetPlayerNewPosition(new Vector3(0,1,-10));
       /* MoveScene("Town");

        ItemManager.instance.ReadyToMakeItem();
        SoundManager.Instance.PlayBGM();*/
        UIManager.Instance.GetGameOverUI.gameObject.SetActive(false);
        MoveScene("Town");
        ClearSystemLogs();
        //AddNewLog("Ȩ���� ��ȯ �Ϸ�!");
    }

    public void PlayerInputEvent()
    {
        if (!IsKeyInput)
            return;
        if (UIManager.Instance.GetIsNameInputUIActive || UIManager.Instance.GetIsSellQuantityUIActive)
            return;
        //��ŸŰ�Է��̺�Ʈ
        if (Input.GetKeyDown("c"))
        {
            if (PlayerAct.instance.SetActState(PLAYERACTSTATE.ATTACK))
            {
                AddNewLog("TestKey C(�⺻�����׽�Ʈ)");
            }
        }

        //�������̺�Ʈ
        var QuickSlotData = UIManager.Instance.GetQuickSlotsData;

        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(0)))
        {
            QuickSlotData.GetQuickSlotData(0).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(1)))
        {
            QuickSlotData.GetQuickSlotData(1).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(2)))
        {
            QuickSlotData.GetQuickSlotData(2).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(3)))
        {
            QuickSlotData.GetQuickSlotData(3).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(4)))
        {
            QuickSlotData.GetQuickSlotData(4).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(5)))
        {
            QuickSlotData.GetQuickSlotData(5).UseQuickSlot();
        }
        if (Input.GetKeyDown(QuickSlotData.GetQuickSlotKey(6)))
        {
            QuickSlotData.GetQuickSlotData(6).UseQuickSlot();
        }

        //SpaceŰ(��ŻŰ) �Է��̺�Ʈ
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Player.instance.PushPortalKey();
        }

        //f(NPC��ȭŰ) �Է��̺�Ʈ
        if (Input.GetKeyDown("f"))
        {
            Player.instance.PushNpcKey();
        }

        //"["(�κ��丮Ȯ��Ű) �Է��̺�Ʈ
        if (Input.GetKeyDown("["))
        {
            AddNewLog("������ ��ĭ �þ���ϴ�.");
            Player.instance.UpgradeInventorySlot();
        }
        //�׽�Ʈ�� ġƮ�������ͽ����� �̺�Ʈ
        if (Input.GetKeyDown("]"))
        {
            AddNewLog("ġƮ���");
            Player.instance.GetPlayerStatus.PlayCheatMode();
        }
        //�κ��丮Ű(i)�Է��̺�Ʈ
        if (Input.GetKeyDown("i"))
        {
            UIManager.Instance.InputInventoryKey();
        }
        //���â(u)�Է��̺�Ʈ
        if (Input.GetKeyDown("u"))
        {
            UIManager.Instance.InputEquipmentUiKey();
        }
        //�������ͽ�â(s)�Է��̺�Ʈ
        if (Input.GetKeyDown("s"))
        {
            UIManager.Instance.InputStatusUiKey();
        }
        //��ųâ(k)�Է��̺�Ʈ
        if (Input.GetKeyDown("k"))
        {
            UIManager.Instance.InputSkillUIKey();
        }
        //�̴ϸ�â(m)�Է��̺�Ʈ
        if (Input.GetKeyDown("m"))
        {
            UIManager.Instance.OnOffMiniMap();
        }
        //��������(/)�Է��̺�Ʈ
        if (Input.GetKeyDown("/"))
        {
            DataSaveAndLoad.Instance.SaveGame();
        }
        //���ӷε�(.)�Է��̺�Ʈ
        if (Input.GetKeyDown("."))
        {
            DataSaveAndLoad.Instance.LoadGame();
        }
        //���θ޴�(esc)�Է��̺�Ʈ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.OnOffGameMenuUI();
        }
    }
}
