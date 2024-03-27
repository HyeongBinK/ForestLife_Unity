using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIManager : MonoBehaviour
{
    private static UIManager m_instance; // �̱����� �Ҵ�� ����
    // �̱��� ���ٿ� ������Ƽ
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

    [SerializeField] private Transform m_MonsterHPBarsAndDamageTextsTransform; //����ü�¹�&���͵������ؽ�Ʈ�� Canvas�� ������ġ
    private Queue<M_HPBar> m_Monster_HPBars = new Queue<M_HPBar>();
    private Queue<FloatingDamageText> m_Monster_DamageTexts = new Queue<FloatingDamageText>();
    [SerializeField] private M_HPBar m_HPBar_Prefab;
    [SerializeField] private FloatingDamageText m_PlayerDamageText;
    [SerializeField] private FloatingDamageText m_MonsterDamageText;
    [SerializeField] private readonly int MAXMONSTER = 20; //���ʵ���Ʋ� �ִ�� ��ȯ�ɼ��ִ� �����Ǽ�(�̰ɹ������� ���� ü�¹ٶ� ��������Ʈ ��������)
    private int m_iCurEnabledHPBar = 0; // ���� Ȱ��ȭ�ִ� ü�¹��� ����
    private int m_iCurEnabledDamageText = 0; // ���� Ȱ��ȭ �Ǿ��ִ� �������ؽ�Ʈ ����

    [SerializeField] private SliderValueController m_PlayerHPBar;
    [SerializeField] private SliderValueController m_PlayerMPBar;
    [SerializeField] private SliderValueController m_PlayerEXPBar;
    [SerializeField] private Text m_LvAndNameText;
    [SerializeField] private Text m_SystemLog;
    [SerializeField] private ScrollRect m_ScrollRect;
    [SerializeField] private int m_FontSize = 27;
    [SerializeField] private int m_FontOffset = 25;

    [SerializeField] private InventoryUI m_InventoryUI; //�κ��丮UI
    [SerializeField] private ToolTip m_ToolTipBox; // �����������ڽ�
    public ToolTip GetToolTipBox { get { return m_ToolTipBox; } }
    public bool GetToolTipBoxActive { get { return m_ToolTipBox.gameObject.activeSelf; } }
    [SerializeField] private PlayerEquipment m_EquipmentUi; //���âUI
    [SerializeField] private StatusUI m_StatusUI; //����âUI
    [SerializeField] private SkillUI m_SkillUI; //��ųâUI
    [SerializeField] private DraggedObject m_draggedobject; //�巡�׵Ǵ� ������Ʈ(��ġ�� ������Ʈ�� ��ȣ�׸��� �̹���), ���Ƿ� �����Ǵ� ���Ե鿡 �����ϱ����ѿ뵵
    public DraggedObject GetDraggedObject { get { return m_draggedobject; } }
    [SerializeField] private GoPing m_goping; //�÷��̾��� ���콺�̵� ��ġ��ǥ ǥ��
    [SerializeField] private QuickSlotsManager m_QuickSlots; //�����Ե��� ������ �����ϱ����� ����
    public QuickSlotsManager GetQuickSlotsData { get { return m_QuickSlots; } }
    [SerializeField] private Button m_StatusUIButton; //�������ͽ�â�� �����ְ� ���ִ� ȭ����� ��ư
    [SerializeField] private Button m_InventoryUIButton; //�κ��丮â�� �����ְ� ���ִ� ȭ����� ��ư
    [SerializeField] private Button m_SkillUIButton; //��ųâ�� �����ְ� ���ִ� ȭ����� ��ư
    [SerializeField] private Button m_SaveButton; //������ �Ҽ��ְ� ���ִ� ȭ����� ��ư
    [SerializeField] private Button m_MiniMapButton; //�̴ϸ�â�� �����ְ� ���ִ� ȭ����� ��ư
    [SerializeField] private Button m_HowToPlayButton; //Ű��������Ű�� �˷��ִ� UI�� ȭ��� �����ִ� ��ư
    [SerializeField] private Button m_GameMenuButton; //���Ӹ޴���ư

    [SerializeField] private Store m_ConsumptionStoreUI; //�Ҹ�����ۻ���UI
    [SerializeField] private Store m_EquipmentStoreUI; //�������ۻ���UI
    [SerializeField] private IsSellButton m_IsSellButton; //�Ǹ�Ȯ�ι�ư UI
    public IsSellButton GetSellButton { get { return m_IsSellButton; } }
    [SerializeField] private InputSellQuantity m_InputSellQuantityUI; //�Ǹ��� ���� �Է� UI
    public InputSellQuantity GetSellQuantityUI { get { return m_InputSellQuantityUI; } }
    public bool GetIsSellQuantityUIActive { get { return m_InputSellQuantityUI.gameObject.activeSelf; } }
    [SerializeField] private GameObject MiniMap; //�̴ϸ�
    [SerializeField] private NpcChatCloud NpcCHat; //NPC��ȭ�ڽ�
    public NpcChatCloud GetNpcChatCloud { get { return NpcCHat; } }
    [SerializeField] private GameObject GameOverUI;
    public GameObject GetGameOverUI{ get{ return GameOverUI; }}
    [SerializeField] private NameInput m_NameInputUI; //�̸��Է�UI
    public bool GetIsNameInputUIActive { get { return m_NameInputUI.gameObject.activeSelf; } }
    public NameInput GetNameInputUI { get { return m_NameInputUI; }}
    [SerializeField] private ToolTip m_MiniToolTipBox; //ȭ�� ��ư���� ���� �������� �����ڽ�
    [SerializeField] private ShowMapName m_ShowMapName; //��(��)�̵����� ���̸�
    [SerializeField] private GameObject m_DammageEffect; //�÷��̾���������� ȭ���� ��¦ �������� �ٲ�� ȿ��
    [SerializeField] private FadeEffect m_FadeEffect; //���̵������̵�ƿ�ȿ��
    public FadeEffect GetFadeEffect { get { return m_FadeEffect; } }
    public bool IsMouseOnUI; //UI���� ���콺��ǲ�� �������� �÷��̾��� ���콺�̵�,������ �������� ���� �Ҹ�����
    [SerializeField] private MenuScript m_MenuUI; //�޴�UI
    [SerializeField] private GameObject m_HowToPlayUI; //���۹�����UI



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
    public void SetHPBarSlider(int CurValue, int MaxValue) //ü���ִ��ġ �����ÿ� ����
    {
        if (m_PlayerHPBar)
        {
            m_PlayerHPBar.Init(CurValue, MaxValue);
            m_PlayerHPBar.SetText(CurValue);
        }
    }
    public void SetMPBarSlider(int CurValue, int MaxValue) //�����ִ��ġ �����ÿ� ����
    {
        if (m_PlayerMPBar)
        {
            m_PlayerMPBar.Init(CurValue, MaxValue);
            m_PlayerMPBar.SetText(CurValue);
        }
    }
    public void SetExpBarSlider(int CurValue, int MaxValue) //����ġ �ִ��ġ �����ÿ� ����
    {
        if (m_PlayerEXPBar)
        {
            m_PlayerEXPBar.Init(CurValue, MaxValue);
            m_PlayerEXPBar.SetPerText(CurValue);
        }
    }
    public void ChangeHPBarCurValue(int CurValue) //����ü�¼�ġ �����ÿ� ����
    {
        if (m_PlayerHPBar)
            m_PlayerHPBar.SetText(CurValue);
    }
    public void ChangeMPBarCurValue(int CurValue) //���縶�¼�ġ �����ÿ� ����
    {
        if (m_PlayerMPBar)
            m_PlayerMPBar.SetText(CurValue);
    }
    public void ChangeEXPBarCurValue(int CurValue) //�������ġ��ġ �����ÿ� ����
    {
        if (m_PlayerEXPBar)
            m_PlayerEXPBar.SetPerText(CurValue);
    }

    public void SetLvAndNameText(int Level, string Name)
    {
        m_LvAndNameText.text = string.Format("LV : {0} / {1}", Level, Name);
    }

    public void SetSystemLog(List<string> Logs) //�ý��۷α��� �ؽ�Ʈ ��ġ �ڵ��̵�
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

    public void SetDamageTextToPlayerAndPlay(int Damage) //�÷��̾�Ӹ����� ����������ǥ��
    {
        Vector3 PlayerWorldLocation = GameManager.instance.playerobject.transform.position + new Vector3(0, 2.2f, 0);
        Vector2 PlayerScreenLocation = Camera.main.WorldToScreenPoint(PlayerWorldLocation);
        m_PlayerDamageText.SetPos(PlayerScreenLocation);
        m_PlayerDamageText.SetText(Damage.ToString());
    }

    public void InputInventoryKey() //�κ��丮UI Ȱ��ȭ/��Ȱ��ȭ
    {
        if (m_InventoryUI)
        {
            m_InventoryUI.gameObject.SetActive(!m_InventoryUI.gameObject.activeSelf);
            SetInventoryGoldText(Player.instance.GetCurInventoryGold());
            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
    }

    public void SetInventoryGoldText(int NewGold) //�κ��丮UI�� ���̴� ��差 ����
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

    public void InputEquipmentUiKey() //���âUI Ȱ��ȭ/��Ȱ��ȭ
    {
        if (m_EquipmentUi)
        {
            m_EquipmentUi.gameObject.SetActive(!m_EquipmentUi.gameObject.activeSelf);
            if (m_ToolTipBox.gameObject.activeSelf)
                m_ToolTipBox.gameObject.SetActive(false);
        }
     
    }

    public void InputStatusUiKey() //�������ͽ�âUI Ȱ��ȭ/��Ȱ��ȭ
    {
        if (m_StatusUI)
        {
            m_StatusUI.gameObject.SetActive(!m_StatusUI.gameObject.activeSelf);
            RefreshStatusUIInformation();
        }
    }

    public void OpenConsumptionStoreUI() //�Ҹ�����۽����UI ����
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

    public void OpenEquipmentStoreUI() //�������۽����UI ����
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

    public void RefreshStatusUIInformation() //�������ͽ�����â ������(����)����
    {
        if(m_StatusUI.gameObject.activeSelf == true)
        m_StatusUI.SetAllData();
    }

    public void InputSkillUIKey() //��ųâUI Ȱ��ȭ/��Ȱ��ȭ
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

    public void RefreshSkillUISkillPoint() //��ųâ UI �� ��ų����Ʈ�� ����
    {
        m_SkillUI.SetSkillPointText();
    }

    public void DrawGoPing(Vector3 ClickPoint) //ȭ�鿡 �̵���ġ(��ǥ) ǥ��
    {
        m_goping.DrawPing(ClickPoint);
    }

    public void OnOffMiniMap() //�̴ϸ�ī�޶� Ű�� ����
    {
        MiniMap.gameObject.SetActive(!MiniMap.gameObject.activeSelf);
    }

    public void OnOffGameMenuUI() //���Ӹ޴� Ȱ��ȭ ��Ȱ��ȭ
    {
        m_MenuUI.gameObject.SetActive(!m_MenuUI.gameObject.activeSelf);
    }

    public void OnOffHowToPlayUI() //���۹�����UI Ȱ��ȭ ��Ȱ��ȭ
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
