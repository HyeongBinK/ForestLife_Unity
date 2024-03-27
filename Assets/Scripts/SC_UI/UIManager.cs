using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIManager : MonoBehaviour
{
    private static UIManager m_instance; // 싱글톤이 할당될 변수
    // 싱글톤 접근용 프로퍼티
    public static UIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }

    [SerializeField] private Transform m_MonsterHPBarsAndDamageTextsTransform; //몬스터체력바&몬스터데미지텍스트의 Canvas내 생성위치
    private Queue<M_HPBar> m_Monster_HPBars = new Queue<M_HPBar>();
    private Queue<FloatingDamageText> m_Monster_DamageTexts = new Queue<FloatingDamageText>();
    [SerializeField] private M_HPBar m_HPBar_Prefab;
    [SerializeField] private FloatingDamageText m_PlayerDamageText;
    [SerializeField] private FloatingDamageText m_MonsterDamageText;
    [SerializeField] private readonly int MAXMONSTER = 20; //전필드통틀어서 최대로 소환될수있는 몬스터의수(이걸바탕으로 몬스터 체력바랑 데미지폰트 갯수셋팅)
    private int m_iCurEnabledHPBar = 0; // 현재 활성화있는 체력바의 갯수
    private int m_iCurEnabledDamageText = 0; // 현재 활성화 되어있는 데미지텍스트 갯수

    [SerializeField] private SliderValueController m_PlayerHPBar;
    [SerializeField] private SliderValueController m_PlayerMPBar;
    [SerializeField] private SliderValueController m_PlayerEXPBar;
    [SerializeField] private Text m_LvAndNameText;
    [SerializeField] private Text m_SystemLog;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private int m_FontSize = 27;
    [SerializeField] private int m_FontOffset = 25;

    [SerializeField] private InventoryUI m_InventoryUI; //인벤토리UI
    [SerializeField] private ToolTip m_ToolTipBox; // 아이템툴팁박스
    public ToolTip GetToolTipBox { get { return m_ToolTipBox; } }
    public bool GetToolTipBoxActive { get { return m_ToolTipBox.gameObject.activeSelf; } }
    [SerializeField] private PlayerEquipment m_EquipmentUi; //장비창UI
    [SerializeField] private StatusUI m_StatusUI; //스탯창UI
    [SerializeField] private SkillUI m_SkillUI; //스킬창UI
    [SerializeField] private DraggedObject m_draggedobject; //드래그되는 오브젝트(위치와 오브젝트의 번호그리고 이미지), 임의로 생성되는 슬롯들에 연결하기위한용도
    public DraggedObject GetDraggedObject { get { return m_draggedobject; } }
    [SerializeField] private GoPing m_goping; //플레이어의 마우스이동 위치좌표 표시
    [SerializeField] private QuickSlotsManager m_QuickSlots; //퀵슬롯들의 정보에 접근하기위해 연결
    public QuickSlotsManager GetQuickSlotsData { get { return m_QuickSlots; } }
    [SerializeField] private Button m_StatusUIButton; //스테이터스창을 열수있게 해주는 화면상의 버튼
    [SerializeField] private Button m_InventoryUIButton; //인벤토리창을 열수있게 해주는 화면상의 버튼
    [SerializeField] private Button m_SkillUIButton; //스킬창을 열수있게 해주는 화면상의 버튼
    [SerializeField] private Button m_SaveButton; //저장을 할수있게 해주는 화면상의 버튼
    [SerializeField] private Button m_MiniMapButton; //미니맵창을 열수있게 해주는 화면상의 버튼
    [SerializeField] private Button m_HowToPlayButton; //키보드조작키를 알려주는 UI를 화면상에 보여주는 버튼
    [SerializeField] private Button m_GameMenuButton; //게임메뉴버튼

    [SerializeField] private Store m_ConsumptionStoreUI; //소모아이템상점UI
    [SerializeField] private Store m_EquipmentStoreUI; //장비아이템상점UI
    [SerializeField] private IsSellButton m_IsSellButton; //판매확인버튼 UI
    public IsSellButton GetSellButton { get { return m_IsSellButton; } }
    [SerializeField] private InputSellQuantity m_InputSellQuantityUI; //판매할 갯수 입력 UI
    public InputSellQuantity GetSellQuantityUI { get { return m_InputSellQuantityUI; } }
    public bool GetIsSellQuantityUIActive { get { return m_InputSellQuantityUI.gameObject.activeSelf; } }
    [SerializeField] private GameObject MiniMap; //미니맵
    [SerializeField] private NpcChatCloud NpcCHat; //NPC대화박스
    public NpcChatCloud GetNpcChatCloud { get { return NpcCHat; } }
    [SerializeField] private GameObject GameOverUI;
    public GameObject GetGameOverUI{ get{ return GameOverUI; }}
    [SerializeField] private NameInput m_NameInputUI; //이름입력UI
    public bool GetIsNameInputUIActive { get { return m_NameInputUI.gameObject.activeSelf; } }
    public NameInput GetNameInputUI { get { return m_NameInputUI; }}
    [SerializeField] private ToolTip m_MiniToolTipBox; //화면 버튼등의 작은 사이즈의 툴팁박스
    [SerializeField] private ShowMapName m_ShowMapName; //씬(맵)이동시의 맵이름
    [SerializeField] private GameObject m_DammageEffect; //플레이어데미지입을시 화면이 살짝 적색으로 바뀌는 효과
    [SerializeField] private FadeEffect m_FadeEffect; //페이드인페이드아웃효과
    public FadeEffect GetFadeEffect { get { return m_FadeEffect; } }
    public bool IsMouseOnUI; //UI위에 마우스인풋이 들어왔을떄 플레이어의 마우스이동,공격을 막기위해 만든 불린변수
    [SerializeField] private MenuScript m_MenuUI; //메뉴UI
    [SerializeField] private GameObject m_HowToPlayUI; //조작법설명UI



    public void ActiveMainUIButton()
    {
        m_StatusUIButton.onClick.AddListener(InputStatusUiKey);
        m_InventoryUIButton.onClick.AddListener(InputInventoryKey);
        m_SkillUIButton.onClick.AddListener(InputSkillUIKey);
        m_MiniMapButton.onClick.AddListener(OnOffMiniMap);
        m_SaveButton.onClick.AddListener(DataSaveAndLoad.Instance.SaveGame);
        m_GameMenuButton.onClick.AddListener(OnOffGameMenuUI);
        m_HowToPlayButton.onClick.AddListener(OnOffHowToPlayUI);
    }

    private void MakeHPBar()
    {
        M_HPBar NewHPBar = Instantiate(m_HPBar_Prefab, m_MonsterHPBarsAndDamageTextsTransform);
        m_Monster_HPBars.Enqueue(NewHPBar);
        NewHPBar.gameObject.SetActive(false);
    }

    private void MakeMonsterDamageText()
    {
        FloatingDamageText NewDamageText = Instantiate(m_MonsterDamageText, m_MonsterHPBarsAndDamageTextsTransform);
        m_Monster_DamageTexts.Enqueue(NewDamageText);
        NewDamageText.gameObject.SetActive(false);
    }
    private void MakeHPBarsAndDamageTexts()
    {
        /*GameObject Parent = new GameObject();
        Parent.transform.SetParent(MonsterHPBarsAndDamageTextsTransform);*/

        for (int i = 0; i < MAXMONSTER; i++)
        {
            MakeHPBar();
            MakeMonsterDamageText();
        }
    }
  
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }

        ActiveMainUIButton();
    }

    public void WithDrawHPBar(M_HPBar hpbar)
    {
        m_Monster_HPBars.Enqueue(hpbar);
    }
    public void WithDrawDamageText(FloatingDamageText damageText)
    {
        m_Monster_DamageTexts.Enqueue(damageText);
    }

    public void ResetMonsetHpbarAndDamageText()
    {
        int LoopTime = m_Monster_HPBars.Count;
        for (int i = 0; i < LoopTime; i++)
        {
            Destroy(m_Monster_HPBars.Dequeue().gameObject);
        }
        LoopTime = m_Monster_DamageTexts.Count;
        for (int i = 0; i < LoopTime; i++)
        {
            Destroy(m_Monster_DamageTexts.Dequeue().gameObject);
        }

        if (m_Monster_HPBars.Count != 0)
            m_Monster_HPBars.Clear();
        if (m_Monster_DamageTexts.Count != 0)
            m_Monster_DamageTexts.Clear();
    }
    public void MakeMonsetHpbarAndDamageText()
    {
        if (m_Monster_HPBars.Count != 0)
            m_Monster_HPBars.Clear();
        if (m_Monster_DamageTexts.Count != 0)
            m_Monster_DamageTexts.Clear();
        m_iCurEnabledHPBar = 0;
        m_iCurEnabledDamageText = 0;
        MakeHPBarsAndDamageTexts();
    }

    public M_HPBar GetHPBar()
    {
        if (m_iCurEnabledHPBar < MAXMONSTER)
        {
            if (m_Monster_HPBars.Count <= 0) MakeHPBar();

            M_HPBar hpbar = m_Monster_HPBars.Dequeue();
            if (hpbar)
            {
                hpbar.gameObject.SetActive(true);
                m_iCurEnabledHPBar++;

                hpbar.HPBarDisable += () => m_Monster_HPBars.Enqueue(hpbar);
                hpbar.HPBarDisable += () => m_iCurEnabledHPBar--;

                return hpbar;
            }
        }
        return null;
    }

    public FloatingDamageText GetDamageText()
    {
        if (m_iCurEnabledDamageText < MAXMONSTER)
        {
            if (m_Monster_DamageTexts.Count <= 0) MakeMonsterDamageText();

            FloatingDamageText damageText = m_Monster_DamageTexts.Dequeue();
            if (damageText)
            {
                //damageText.gameObject.SetActive(false);
                m_iCurEnabledDamageText++;

                damageText.TextDisable += () => m_Monster_DamageTexts.Enqueue(damageText);
                damageText.TextDisable += () => m_iCurEnabledDamageText--;

                return damageText;
            }
        }
        return null;
    }
    public void SetHPBarSlider(int CurValue, int MaxValue) //체력최대수치 변동시에 쓰임
    {
        if (m_PlayerHPBar)
        {
            m_PlayerHPBar.Init(CurValue, MaxValue);
            m_PlayerHPBar.SetText(CurValue);
        }
    }
    public void SetMPBarSlider(int CurValue, int MaxValue) //마력최대수치 변동시에 쓰임
    {
        if (m_PlayerMPBar)
        {
            m_PlayerMPBar.Init(CurValue, MaxValue);
            m_PlayerMPBar.SetText(CurValue);
        }
    }
    public void SetExpBarSlider(int CurValue, int MaxValue) //경험치 최대수치 변동시에 쓰임
    {
        if (m_PlayerEXPBar)
        {
            m_PlayerEXPBar.Init(CurValue, MaxValue);
            m_PlayerEXPBar.SetPerText(CurValue);
        }
    }
    public void ChangeHPBarCurValue(int CurValue) //현재체력수치 변동시에 쓰임
    {
        if (m_PlayerHPBar)
            m_PlayerHPBar.SetText(CurValue);
    }
    public void ChangeMPBarCurValue(int CurValue) //현재마력수치 변동시에 쓰임
    {
        if (m_PlayerMPBar)
            m_PlayerMPBar.SetText(CurValue);
    }
    public void ChangeEXPBarCurValue(int CurValue) //현재경험치수치 변동시에 쓰임
    {
        if (m_PlayerEXPBar)
            m_PlayerEXPBar.SetPerText(CurValue);
    }

    public void SetLvAndNameText(int Level, string Name)
    {
        m_LvAndNameText.text = string.Format("LV : {0} / {1}", Level, Name);
    }

    public void SetSystemLog(List<string> Logs) //시스템로그의 텍스트 위치 자동이동
    {
        var text = new System.Text.StringBuilder();
        m_SystemLog.rectTransform.sizeDelta = Vector2.up * (Logs.Count * m_FontSize + m_FontOffset);
        for (int i = 0; Logs.Count > i; i++)
        {
            text.AppendLine(Logs[i]);
        }
        m_SystemLog.text = text.ToString();

        m_ScrollRect.verticalNormalizedPosition = 0;
    }

    public void SetDamageTextToPlayerAndPlay(int Damage) //플레이어머리위에 맞은데미지표시
    {
        Vector3 PlayerWorldLocation = GameManager.instance.playerobject.transform.position + new Vector3(0, 2.2f, 0);
        Vector2 PlayerScreenLocation = Camera.main.WorldToScreenPoint(PlayerWorldLocation);
        m_PlayerDamageText.SetPos(PlayerScreenLocation);
        m_PlayerDamageText.SetText(Damage.ToString());
    }

    public void InputInventoryKey() //인벤토리UI 활성화/비활성화
    {
        if (m_InventoryUI)
        {
            m_InventoryUI.gameObject.SetActive(!m_InventoryUI.gameObject.activeSelf);
            SetInventoryGoldText(Player.instance.GetCurInventoryGold());
            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void SetInventoryGoldText(int NewGold) //인벤토리UI에 보이는 골드량 수정
    {
        m_InventoryUI.SetGoldText(NewGold);
    }

    public void SetInventorySlotNumber(int ActivatedSlotNumber)
    {
        m_InventoryUI.SetActivatedSlotNumber(ActivatedSlotNumber);
    }

    public void SetSlotData(int SlotNumber)
    {
        m_InventoryUI.SetSlotData(SlotNumber);
    }

    public void InputEquipmentUiKey() //장비창UI 활성화/비활성화
    {
        if (m_EquipmentUi)
        {
            m_EquipmentUi.gameObject.SetActive(!m_EquipmentUi.gameObject.activeSelf);
            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
     
    }

    public void InputStatusUiKey() //스테이터스창UI 활성화/비활성화
    {
        if (m_StatusUI)
        {
            m_StatusUI.gameObject.SetActive(!m_StatusUI.gameObject.activeSelf);
            RefreshStatusUIInformation();
        }
    }

    public void OpenConsumptionStoreUI() //소모아이템스토어UI 열기
    {
        if (m_ConsumptionStoreUI)
        {
            m_ConsumptionStoreUI.gameObject.SetActive(true);
            Player.instance.SetIsStoreUi(true);
            m_InventoryUI.gameObject.SetActive(true);
        

            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void OpenEquipmentStoreUI() //장비아이템스토어UI 열기
    {
        if (m_EquipmentStoreUI)
        {
            m_EquipmentStoreUI.gameObject.SetActive(true);
            Player.instance.SetIsStoreUi(true);
            m_InventoryUI.gameObject.SetActive(true);

            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void RefreshStatusUIInformation() //스테이터스정보창 데이터(정보)갱신
    {
        if(m_StatusUI.gameObject.activeSelf == true)
        m_StatusUI.SetAllData();
    }

    public void InputSkillUIKey() //스킬창UI 활성화/비활성화
    {
        
        if (m_SkillUI)
        { 
            if (!m_SkillUI.gameObject.activeSelf)
                RefreshSkillUISkillPoint();
            m_SkillUI.gameObject.SetActive(!m_SkillUI.gameObject.activeSelf);

            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
      
    } 

    public void RefreshSkillUISkillPoint() //스킬창 UI 의 스킬포인트를 갱신
    {
        m_SkillUI.SetSkillPointText();
    }

    public void DrawGoPing(Vector3 ClickPoint) //화면에 이동위치(좌표) 표시
    {
        m_goping.DrawPing(ClickPoint);
    }

    public void OnOffMiniMap() //미니맵카메라 키고 끄기
    {
        MiniMap.gameObject.SetActive(!MiniMap.gameObject.activeSelf);
    }

    public void OnOffGameMenuUI() //게임메뉴 활성화 비활성화
    {
        m_MenuUI.gameObject.SetActive(!m_MenuUI.gameObject.activeSelf);
    }

    public void OnOffHowToPlayUI() //조작법설명UI 활성화 비활성화
    {
        m_HowToPlayUI.gameObject.SetActive(!m_HowToPlayUI.gameObject.activeSelf);
    }

    public void SetAndActiveMiniToolTipBox(string Text, Vector2 ScreenPosition)
    {
        m_MiniToolTipBox.gameObject.SetActive(true);
        m_MiniToolTipBox.SetToolTipPosition(ScreenPosition.x, ScreenPosition.y);
        m_MiniToolTipBox.SetToolTipText(Text);
    }

    public void DisActiveMiniToolTip()
    {
        m_MiniToolTipBox.gameObject.SetActive(false);
        m_MiniToolTipBox.SetToolTipText("");
    }

    public void ShowMapName()
    {
        m_ShowMapName.SetMapNameAndShow();
    }

    public void ActiveDammageEffect()
    {
        m_DammageEffect.gameObject.SetActive(true);
        StartCoroutine(DamageEffectDisappear());
    }

    IEnumerator DamageEffectDisappear()
    {
        yield return new WaitForSeconds(0.3f);
        if(m_DammageEffect.gameObject.activeSelf)
            m_DammageEffect.gameObject.SetActive(false);
    }

    public void WhenSceneMoveOffActivatedUI()
    {
        m_InventoryUI.gameObject.SetActive(false);
        m_ToolTipBox.gameObject.SetActive(false);
        m_EquipmentUi.gameObject.SetActive(false);
        m_StatusUI.gameObject.SetActive(false);
        m_SkillUI.gameObject.SetActive(false);
        m_draggedobject.gameObject.SetActive(false);
        m_ConsumptionStoreUI.gameObject.SetActive(false);
        m_EquipmentStoreUI.gameObject.SetActive(false);
        m_IsSellButton.gameObject.SetActive(false);
        m_InputSellQuantityUI.gameObject.SetActive(false);
        NpcCHat.gameObject.SetActive(false);
        m_NameInputUI.gameObject.SetActive(false);
        m_MiniToolTipBox.gameObject.SetActive(false);
        m_ShowMapName.gameObject.SetActive(false);
        m_DammageEffect.gameObject.SetActive(false);
    }
}
