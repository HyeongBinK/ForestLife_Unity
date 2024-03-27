using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerCurState
{
    public string Name; //�÷��̾��� �̸�
    public int Level;//����
    public int MaxEXP; //�������� �ʿ��� �䱸����ġ
    public int StatPoint; // ��������Ʈ(������� 3ȹ��), ��,����,�ǰ�,���ɿ� ���ڰ���
    public int SkillPoint; //��ų����Ʈ(������� 1ȹ��), ��ų���ڰ���
    public int CurrentEXP;  //���� ����ġ
    public int CurrentHP; //����ü��
    public int CurrentMP;//���縶��
    public float PlayerSpeed; //�÷��̾��̵��ӵ�
    public int PowerSwingSkillLevel; //�Ŀ�������ų����
    public int EarthquakeSkillLevel; //������ų����
    public int ThunderSkillLevel; //������ų����
    public int AttackIncreaseSkillLevel; //���ݷ�������ų����
    public int ShieldIncreaseSkillLevel; //����������ų����
    public int CriticalIncreaseSkillLevel; //ũ��Ƽ��Ȯ����ų����
    public int SpeedIncreaseSkillLevel; //�̵��ӵ�������ų����
    public Status PlayerStatus; //�÷��̾��� ���ȴɷ�ġ

    public void SetData(string name, int level, int statpoint,int skillpoint, int curExp, int CurHP, int CurMP, float Speed,
        int PowerSwingLevel, int EarthQuakeLevel, int ThunderLevel, int AttackLevel, int ShieldLevel, int CritLevel, int SpeedLevel, Status StatusData)
    {
        Name = name;
        Level = level;
        StatPoint = statpoint;
        SkillPoint = skillpoint;
        CurrentEXP = curExp;
        CurrentHP = CurHP;
        CurrentMP = CurMP;
        PlayerSpeed = Speed;
        PowerSwingSkillLevel = PowerSwingLevel;
        EarthquakeSkillLevel = EarthQuakeLevel;
        ThunderSkillLevel = ThunderLevel;
        AttackIncreaseSkillLevel = AttackLevel;
        ShieldIncreaseSkillLevel = ShieldLevel;
        CriticalIncreaseSkillLevel = CritLevel;
        SpeedIncreaseSkillLevel = SpeedLevel;
        PlayerStatus = StatusData;
    }
}

public class PlayerStatus : MonoBehaviour
{
    private Status State; //�÷��̾��� ���� �ɷ�ġ(�������� �� ���뺸�ʽ�)
    public Status GetStatus { get { return State; } }
    private Status TotalEquipmentData; //����� �нú꽺ų�� �߰��ɷ�ġ ����
    private Status TotalState; //���,�нú꽺ų������ �߰��ɷ�ġ, ���� �ջ�� �ɷ�ġ
    public Status GetTotalStatus { get { return TotalState; } }
    public string m_strName { get; private set; } //�÷��̾��� �̸�
    public int m_iLevel { get; private set; } //����
    public int m_iMaxEXP { get { return (10 * m_iLevel) + (int)Mathf.Pow(m_iLevel, 2); } } //�������� �ʿ��� �䱸����ġ
    public int m_iStatPoint { get; private set; } // ��������Ʈ(������� 3ȹ��), ��,����,�ǰ�,���ɿ� ���ڰ���
    public int m_iSkillPoint { get; private set; } //��ų����Ʈ(������� 1ȹ��), ��ų���ڰ���
    public int m_iCurrentEXP { get; private set; }  //���� ����ġ
    public int m_iCurrentHP { get; private set; } //����ü��
    public int m_iCurrentMP { get; private set; } //���縶��
    public float m_fPlayerSpeed { get; private set; } //�÷��̾��̵��ӵ�
   // public bool IsPlayerDie { get; private set; } //ü���� 0���Ϸΰ����ϸ� true�� �ٲ�

    public int PowerSwingSkillLevel { get; private set; } //�Ŀ�������ų����
    public int EarthquakeSkillLevel { get; private set; } //������ų����
    public int ThunderSkillLevel { get; private set; } //������ų����
    public int AttackIncreaseSkillLevel { get; private set; } //���ݷ�������ų����
    public int ShieldIncreaseSkillLevel { get; private set; } //����������ų����
    public int CriticalIncreaseSkillLevel { get; private set; } //ũ��Ƽ��Ȯ����ų����
    public int SpeedIncreaseSkillLevel { get; private set; } //�̵��ӵ�������ų����
    public readonly string PlayerSaveDataName = "PlayerStateSaveData";
    public readonly string PlayerDefaultDataName = "PlayerDefaultData";
    public readonly string PlayerCheatDataName = "PlayerCheatStateSaveData";

    public void UpdateUI()
    {
        UIManager.Instance.SetExpBarSlider(m_iCurrentEXP, m_iMaxEXP);
        UIManager.Instance.SetLvAndNameText(m_iLevel, m_strName);
        UIManager.Instance.SetHPBarSlider(m_iCurrentHP, TotalState.MaxHP);
        UIManager.Instance.SetMPBarSlider(m_iCurrentMP, TotalState.MaxMP);
        UIManager.Instance.RefreshSkillUISkillPoint();
    }
    private void SetState() // ����� or �������ڽÿ� �Ҹ� ����
    {
        State.MaxHP = 100 + ((m_iLevel - 1) * 20) + (State.Health * 10);
        State.MaxMP = 50 + ((m_iLevel - 1) * 5) + (State.Int * 5);
        State.Atk = 10 + (State.Str * 5) + State.Dex + State.Int;
        State.Def = 0 + (State.Health * 0.1f);
        State.Critical = 1 + (State.Dex * 0.1f);
    }
    public void SetTotalState() //��񺯰�, �нú꽺ų���� �� ���� SetState �Ŀ� �Ҹ� ����
    {
        TotalState.Str = State.Str + TotalEquipmentData.Str;
        TotalState.Dex = State.Dex + TotalEquipmentData.Dex;
        TotalState.Health = State.Health + TotalEquipmentData.Health;
        TotalState.Int = State.Int + TotalEquipmentData.Int;

        TotalState.MaxHP = 100 + ((m_iLevel - 1) * 20) + (TotalState.Health * 10) + TotalEquipmentData.MaxHP;
        TotalState.MaxMP = 50 + ((m_iLevel - 1) * 5) + (TotalState.Int * 5) + TotalEquipmentData.MaxMP;
        if (DataTableManager.instance)
        {
            var SkillDatas = DataTableManager.instance.GetSkillDataTable;
            float PassiveSkillATKIncrease, PassiveSkillDEFIncrease, PassiveSkillCRITIncrease, PassiveSkillSpeedIncrease;

            PassiveSkillATKIncrease = SkillDatas["AttackIncrease"].GetValue(AttackIncreaseSkillLevel);
            PassiveSkillDEFIncrease = SkillDatas["ShieldIncrease"].GetValue(ShieldIncreaseSkillLevel);
            PassiveSkillCRITIncrease = SkillDatas["CriticalIncrease"].GetValue(CriticalIncreaseSkillLevel);
            PassiveSkillSpeedIncrease = SkillDatas["SpeedIncrease"].GetValue(SpeedIncreaseSkillLevel);

            TotalState.Atk = (int)((10 + (TotalState.Str * 5) + (TotalState.Dex + TotalState.Int + TotalEquipmentData.Atk)) * (1 + (PassiveSkillATKIncrease * 0.01f)));
            TotalState.Def = 0 + (TotalState.Health * 0.1f) + TotalEquipmentData.Def + PassiveSkillDEFIncrease;
            TotalState.Critical = 1 + (TotalState.Dex * 0.1f) + TotalEquipmentData.Critical + PassiveSkillCRITIncrease;
            m_fPlayerSpeed = 5 + (PassiveSkillSpeedIncrease * 4);
        }

        RegulateOverMaxHPMP();
        TotalState.Critical = Mathf.Clamp(TotalState.Critical, 0, 100);
        PlayerAct.instance.SetNewSpeed(m_fPlayerSpeed);
        UIManager.Instance.RefreshStatusUIInformation();
        UpdateUI();
    }

    private void Awake()
    {
        SetDefaultState();
    }
    public void SetName(string NewName) // ���� �̸��Է� �� �̸����� ���
    {
        m_strName = NewName;
        UIManager.Instance.SetLvAndNameText(m_iLevel, m_strName);
    }

    public void LevelUpCheck() //����ġ ����ø��� �ҷ���
    {
        if (m_iCurrentEXP >= m_iMaxEXP)
        {
            LevelChange(); //��������
            SetState(); //ĳ���Ͱ��� �������� ����
            SetTotalState(); //ĳ������Ż(�нú�,���������۵�����) �������� ����
            m_iCurrentHP = TotalState.MaxHP; //HP ȸ��
            m_iCurrentMP = TotalState.MaxMP; //MP ȸ��
            UpdateUI(); //UI �ϰ�����
        }
    }
    private void LevelChange() //�����
    {
        if (m_iCurrentEXP >= m_iMaxEXP)
        {
            m_iCurrentEXP -= m_iMaxEXP; 
            LevelUP(); // ������
            LevelChange(); //�߰� �������� �Ҽ��ִ°�?
        }
    }
    private void LevelUP() //������� ����� ���ʴɷ������� ����,��ų����Ʈ ���� 
    {
        m_iLevel++; //���� 1����
        m_iStatPoint += 3; //��������Ʈ 3����
        m_iSkillPoint++; //��ų����Ʈ 1����
        UIManager.Instance.RefreshStatusUIInformation(); //����â ����
        UIManager.Instance.RefreshSkillUISkillPoint(); //��ųâ ����
        GameManager.instance.AddNewLog(m_strName + "�Բ��� ��������Ͽ� " + m_iLevel.ToString() + "Level�� �Ǽ̽��ϴ�."); //�ý��۹ڽ��� ����ǥ��
        PlayerAct.instance.LevelUp(); //������ ����Ʈ �߻�
    }

    private bool IsStatPoint()
    {
        if (m_iStatPoint > 0)
        {
            m_iStatPoint--;
            return true;
        }
        return false;
    }

    public bool IsSkillPoint()
    {
        if (m_iSkillPoint > 0)
        {
            m_iSkillPoint--;
            UIManager.Instance.RefreshSkillUISkillPoint();
            return true;
        }
        return false;
    }

    public void AddStr()
    {
        if (IsStatPoint())
        {
            State.Str++;
            SetTotalState();
        }
    }

    public void AddDex()
    {
        if (IsStatPoint())
        {
            State.Dex++;
            SetTotalState();
        }
    }
    public void AddHealth()
    {
        if (IsStatPoint())
        {
            State.Health++;
            SetTotalState();
        }
    }

    public void AddInt()
    {
        if (IsStatPoint())
        {
            State.Int++;
            SetTotalState();
        }
    }
    public void MPDecrease(int MPCost)
    {
        m_iCurrentMP = Mathf.Clamp(m_iCurrentMP - MPCost, 0, TotalState.MaxMP);
        UIManager.Instance.ChangeMPBarCurValue(m_iCurrentMP);
        UIManager.Instance.RefreshStatusUIInformation();
    }

    public void HPDecrease(int MinusHP)
    {
        if (m_iCurrentHP <= 0) return;

        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP - MinusHP, 0, TotalState.MaxHP); //����ü���� - ���� �����ʵ��� ����
        UIManager.Instance.ChangeHPBarCurValue(m_iCurrentHP); //ü�¹� ����
        UIManager.Instance.RefreshStatusUIInformation(); //�������ͽ�â�� ü������ ����
        if(MinusHP > 0) //���� �������� 0���� ũ�� �ǰ�����Ʈ(ȭ�� ����) Ȱ��
        UIManager.Instance.ActiveDammageEffect();

        if (m_iCurrentHP <= 0) //ü���� 0���� �������� ����̺�Ʈ �߻�
        {
            PlayerAct.instance.SetActState(PLAYERACTSTATE.DEAD); //�÷��̾ ���� ���·� �����ϰ� ����ִϸ��̼� ����
            UIManager.Instance.GetGameOverUI.gameObject.SetActive(true); //ȭ��UI�� "YOU DIED" Text Ȱ��
            StartCoroutine(Resurrection()); //����ִϸ��̼� ������ ��Ȱ�̺�Ʈ ����
        }
    }
    public void UsePortion(float HPValue, float MPValue, int ExpValue)
    {
        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP + (int)(TotalState.MaxHP * HPValue), m_iCurrentHP, TotalState.MaxHP);
        m_iCurrentMP = Mathf.Clamp(m_iCurrentMP + (int)(TotalState.MaxMP * MPValue), m_iCurrentMP, TotalState.MaxMP);
        UIManager.Instance.ChangeMPBarCurValue(m_iCurrentMP);
        UIManager.Instance.ChangeHPBarCurValue(m_iCurrentHP);
        GetExp(ExpValue);
    }

    public void GetExp(int PlusExp)
    {
        m_iCurrentEXP += PlusExp; //���� ����ġ �翡 ���� ���� ����ġ �߰�
        LevelUpCheck(); //������ ���� Ȯ���Ͽ� ������ ������
        UIManager.Instance.SetExpBarSlider(m_iCurrentEXP, m_iMaxEXP); //����ġ�� ����
        UIManager.Instance.RefreshStatusUIInformation(); //�������ͽ�â ����
    }

    //��񺯰��� �÷��̾� �ѽ������ͽ�����
    public void ChangeEquipmentData(Status NewTotalEquipmentData) 
    {
        TotalEquipmentData = NewTotalEquipmentData; //���ɷ�ġ�հ��� �������ͽ�
        SetTotalState(); //ĳ���Ͱ����ɷ�ġ+ ��� + �нú꽺ų ���� �������ͽ�
    }
    public void DeadPenalty() // ������ ü�� ���� �ִ�ġ�� ���� �������ġ�� 10%�����Ͽ� �������� ��Ȱ
    {
        GameManager.instance.ReturnToHome();
        m_iCurrentEXP = Mathf.RoundToInt(m_iCurrentEXP * 0.9f);
        m_iCurrentHP = (int)(TotalState.MaxHP * 0.5f);
        m_iCurrentMP = (int)(TotalState.MaxMP * 0.5f);
        PlayerAct.instance.Rebirth();
        PlayerAct.instance.SetNewSpeed(m_fPlayerSpeed);
     
    }

    public int GetSkillLevelByName(string SkillName)
    {
        switch (SkillName)
        {
            case "PowerSwing":
                return PowerSwingSkillLevel;

            case "Earthquake":
                return EarthquakeSkillLevel;

            case "Thunder":
                return ThunderSkillLevel;

            case "AttackIncrease":
                return AttackIncreaseSkillLevel;

            case "ShieldIncrease":
                return ShieldIncreaseSkillLevel;

            case "CriticalIncrease":
                return CriticalIncreaseSkillLevel;

            case "SpeedIncrease":
                return SpeedIncreaseSkillLevel;

            default:
                return 0;
        }
    }

    public bool CanSkillLevelUP(int SkillMaxLevel, int CurLevel)
    {
        bool IsCan = false;

        if (SkillMaxLevel > CurLevel) //��ų�ִ뷹�� ���� ���罺ų������ ������ 
        {
            if (IsSkillPoint()) //�����ִ� ��ų����Ʈ�� ������ 
                IsCan = true;
        }

        return IsCan;
    }

    public bool SkillLevelUpByName(string SkillName, int CurSkillLevel)
    {
        bool IsLevelUpSucces = CanSkillLevelUP(DataTableManager.instance.GetSkillData(SkillName).SkillMaxLevel, CurSkillLevel);

        if (IsLevelUpSucces)
        {
            switch (SkillName)
            {
                case "PowerSwing":
                    PowerSwingSkillLevel++;
                    break;
                case "Earthquake":
                    EarthquakeSkillLevel++;
                    break;
                case "Thunder":
                    ThunderSkillLevel++;
                    break;
                case "AttackIncrease":
                    AttackIncreaseSkillLevel++;
                    SetTotalState();
                    break;
                case "ShieldIncrease":
                    ShieldIncreaseSkillLevel++;
                    SetTotalState();
                    break;
                case "CriticalIncrease":
                    CriticalIncreaseSkillLevel++;
                    SetTotalState();
                    break;
                case "SpeedIncrease":
                    SpeedIncreaseSkillLevel++;
                    SetTotalState();
                    PlayerAct.instance.SetNewSpeed(m_fPlayerSpeed);
                    break;
                default:
                    Debug.LogError("��ġ : �÷��̾�����ͽ�Ŭ����.SkillLevelUpByName �Լ�, Skill�̸��� �̻��մϴ�");
                    break;
            }
        }

        return IsLevelUpSucces;
    }

    public void SetDefaultState()
    {
        LoadStateData(PlayerDefaultDataName);
        SetState();
        TotalEquipmentData = new Status();
        SetTotalState();
        UpdateUI();
    }

    public void PlayCheatMode() //�׽�Ʈ�� ġƮ���(�ɷ�ġ ũ�Կ���)
    {
        LoadStateData(PlayerCheatDataName);
        SetState();
        Player.instance.PickupGold(99999);
        SetTotalState();
        UpdateUI();
    }

    IEnumerator Resurrection() //��Ȱ
    {
        yield return new WaitForSeconds(3.0f);
        DeadPenalty();
    }
    public float ExportTotalCiriticalRate()
    {
        return TotalState.Critical;
    }
   
    public void SetData(string name, int level, int statpoint, int skillpoint, int curExp, int CurHP, int CurMP, float Speed,
       int PowerSwingLevel, int EarthQuakeLevel, int ThunderLevel, int AttackLevel, int ShieldLevel, int CritLevel, int SpeedLevel, Status state)
    {
        m_strName = name;
        m_iLevel = level;
        m_iStatPoint = statpoint;
        m_iSkillPoint = skillpoint;
        m_iCurrentEXP = curExp;
        m_iCurrentHP = CurHP;
        m_iCurrentMP = CurMP;
        m_fPlayerSpeed = Speed;
        PowerSwingSkillLevel = PowerSwingLevel;
        EarthquakeSkillLevel = EarthQuakeLevel;
        ThunderSkillLevel = ThunderLevel;
        AttackIncreaseSkillLevel = AttackLevel;
        ShieldIncreaseSkillLevel = ShieldLevel;
        CriticalIncreaseSkillLevel = CritLevel;
        SpeedIncreaseSkillLevel = SpeedLevel;
        State = state;
        SetState();
        SetTotalState();
        UpdateUI();
    }

    public void SaveStateData()
    {
        PlayerCurState PlayerStateData = new PlayerCurState();
        
        PlayerStateData.SetData(m_strName, m_iLevel, m_iStatPoint, m_iSkillPoint, m_iCurrentEXP, m_iCurrentHP, m_iCurrentMP, m_fPlayerSpeed,
             PowerSwingSkillLevel, EarthquakeSkillLevel, ThunderSkillLevel, AttackIncreaseSkillLevel, ShieldIncreaseSkillLevel, CriticalIncreaseSkillLevel, SpeedIncreaseSkillLevel, State);

        string json = JsonUtility.ToJson(PlayerStateData);
        File.WriteAllText(DataSaveAndLoad.Instance.MakeFilePath(PlayerSaveDataName, "/SaveData/StateData/"), json);
    }

    public bool LoadStateData(string FileName) //�ε�����
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath(FileName, "/SaveData/StateData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("�÷��̾� ���� ���̺������� ã�����߽��ϴ�.");
            return false;
        }
        string SaveFile = File.ReadAllText(FilePath);
        PlayerCurState PlayerStateData = JsonUtility.FromJson<PlayerCurState>(SaveFile);
        SetData(PlayerStateData.Name, PlayerStateData.Level, PlayerStateData.StatPoint, PlayerStateData.SkillPoint, PlayerStateData.CurrentEXP, PlayerStateData.CurrentHP, PlayerStateData.CurrentMP, 
            PlayerStateData.PlayerSpeed, PlayerStateData.PowerSwingSkillLevel, PlayerStateData.EarthquakeSkillLevel, PlayerStateData.ThunderSkillLevel, PlayerStateData.AttackIncreaseSkillLevel, 
            PlayerStateData.ShieldIncreaseSkillLevel,PlayerStateData.CriticalIncreaseSkillLevel, PlayerStateData.SpeedIncreaseSkillLevel, PlayerStateData.PlayerStatus);
        
        return true;
    }

    public void SetPlayerSpeed()
    {
        PlayerAct.instance.SetNewSpeed(m_fPlayerSpeed);
    }

    public void RegulateOverMaxHPMP()
    {
        if (m_iCurrentHP > TotalState.MaxHP)
            m_iCurrentHP = TotalState.MaxHP;
        if (m_iCurrentMP > TotalState.MaxMP)
            m_iCurrentMP = TotalState.MaxMP;
    }
}

