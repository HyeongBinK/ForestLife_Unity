using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance; // 싱글톤이 할당될 변수
    private Vector3 StartPosition; //맵에 플레이어가 스폰(도착,소환) 됬을떄의 위치좌표
    private bool PlayerDeadFlag; //플레이어가 죽으면 true 그외 false(게임오버체크용//게임오버시 패널티후 거점에서 부활)
    public GameObject playerobject;
    public Collider playercollider;
    public readonly int MaxDammage = 9999;
    public readonly int ErrorNumber = -1;
    static int CriticalDamagePercent = 200;
    public readonly float ItemMaintainTime = 30f; //아이템드롭후 유지되는시간
    private const int MaxSystemMessageLine = 20; //시스템로그 최대표기수, 넘어가면 맨앞의 로그부터 순차적으로 사라지고 새로그 추가
    private string m_prevSceneName;
    private string m_CurSceneName;
    private StartSceneIsContinue IsContinueObject;
    private List<string> SystemLogs = new List<string>();//UIManager의 시스템로그출력에 쓰일 로그들
    private bool IsKeyInput = true; //키보드입력을 할수있는가 에 대한 불린형 변수
    // 싱글톤 접근용 프로퍼티
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
            AddNewLog(scene.name + "으로 이동완료!");
            IsKeyInput = true;
            Player.instance.PlayerEndMoveScene();
            Player.instance.GetPlayerStatus.SetPlayerSpeed();
        }
    }

    public void MoveScene(string TargetScene)
    {
        UIManager.Instance.GetFadeEffect.FadeOut();
        if (MonsterSpawner.instance) //몬스터스포너가있으면 필드내 몬스터정리
        {
            MonsterSpawner.instance.ClearField();
        }
        if(ItemManager.instance) //아이템매니저가있으면 필드내 아이템정리
        {
            ItemManager.instance.ClearField();
        }

        IsKeyInput = false;
        Player.instance.PlayerStartMoveScene();
        UIManager.Instance.ResetMonsetHpbarAndDamageText(); //UiManager에 몬스터 체력바랑 데미지텍스터가있으면 정리
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
            AddNewLog("저장된데이터를 찾지 못하여 처음부터시작합니다.");

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
                    Debug.LogError("데이터테이블에러");
            }
            //AddNewLog(Damage.ToString());
            float CriticalRate = Player.instance.GetPlayerStatus.ExportTotalCiriticalRate();
            if (UnityEngine.Random.Range(0, 100) < CriticalRate)
                Damage = Mathf.Clamp(Damage * (CriticalDamagePercent / 100), 0, MaxDammage);
            return Damage;
        }
        else
            Debug.LogError("게임메니저 에러");

        return 0;
    }

    public void ReturnToHome() //귀환주문서사용시 또는 사망시 사용
    {
        SetPlayerNewPosition(new Vector3(0,1,-10));
       /* MoveScene("Town");

        ItemManager.instance.ReadyToMakeItem();
        SoundManager.Instance.PlayBGM();*/
        UIManager.Instance.GetGameOverUI.gameObject.SetActive(false);
        MoveScene("Town");
        ClearSystemLogs();
        //AddNewLog("홈으로 귀환 완료!");
    }

    public void PlayerInputEvent()
    {
        if (!IsKeyInput)
            return;
        if (UIManager.Instance.GetIsNameInputUIActive || UIManager.Instance.GetIsSellQuantityUIActive)
            return;
        //평타키입력이벤트
        if (Input.GetKeyDown("c"))
        {
            if (PlayerAct.instance.SetActState(PLAYERACTSTATE.ATTACK))
            {
                AddNewLog("TestKey C(기본공격테스트)");
            }
        }

        //퀵슬롯이벤트
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

        //Space키(포탈키) 입력이벤트
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Player.instance.PushPortalKey();
        }

        //f(NPC대화키) 입력이벤트
        if (Input.GetKeyDown("f"))
        {
            Player.instance.PushNpcKey();
        }

        //"["(인벤토리확장키) 입력이벤트
        if (Input.GetKeyDown("["))
        {
            AddNewLog("슬롯이 한칸 늘어났습니다.");
            Player.instance.UpgradeInventorySlot();
        }
        //테스트용 치트스테이터스셋팅 이벤트
        if (Input.GetKeyDown("]"))
        {
            AddNewLog("치트모드");
            Player.instance.GetPlayerStatus.PlayCheatMode();
        }
        //인벤토리키(i)입력이벤트
        if (Input.GetKeyDown("i"))
        {
            UIManager.Instance.InputInventoryKey();
        }
        //장비창(u)입력이벤트
        if (Input.GetKeyDown("u"))
        {
            UIManager.Instance.InputEquipmentUiKey();
        }
        //스테이터스창(s)입력이벤트
        if (Input.GetKeyDown("s"))
        {
            UIManager.Instance.InputStatusUiKey();
        }
        //스킬창(k)입력이벤트
        if (Input.GetKeyDown("k"))
        {
            UIManager.Instance.InputSkillUIKey();
        }
        //미니맵창(m)입력이벤트
        if (Input.GetKeyDown("m"))
        {
            UIManager.Instance.OnOffMiniMap();
        }
        //게임저장(/)입력이벤트
        if (Input.GetKeyDown("/"))
        {
            DataSaveAndLoad.Instance.SaveGame();
        }
        //게임로드(.)입력이벤트
        if (Input.GetKeyDown("."))
        {
            DataSaveAndLoad.Instance.LoadGame();
        }
        //메인메뉴(esc)입력이벤트
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.OnOffGameMenuUI();
        }
    }
}
