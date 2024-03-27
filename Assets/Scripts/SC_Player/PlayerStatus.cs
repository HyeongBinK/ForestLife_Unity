using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class PlayerCurState
{
    public string Name; //플레이어의 이름
    public int Level;//레벨
    public int MaxEXP; //레벨업에 필요한 요구경험치
    public int StatPoint; // 스탯포인트(레밸업시 3획득), 힘,덱스,건강,지능에 투자가능
    public int SkillPoint; //스킬포인트(레밸업시 1획득), 스킬투자가능
    public int CurrentEXP;  //현재 경험치
    public int CurrentHP; //현재체력
    public int CurrentMP;//현재마나
    public float PlayerSpeed; //플레이어이동속도
    public int PowerSwingSkillLevel; //파워스윙스킬레밸
    public int EarthquakeSkillLevel; //지진스킬레밸
    public int ThunderSkillLevel; //번개스킬레밸
    public int AttackIncreaseSkillLevel; //공격력증가스킬레밸
    public int ShieldIncreaseSkillLevel; //방어력증가스킬레밸
    public int CriticalIncreaseSkillLevel; //크리티컬확률스킬레밸
    public int SpeedIncreaseSkillLevel; //이동속도증가스킬레밸
    public Status PlayerStatus; //플레이어의 스탯능력치

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
    private Status State; //플레이어의 고유 능력치(스탯투자 및 레밸보너스)
    public Status GetStatus { get { return State; } }
    private Status TotalEquipmentData; //장비들과 패시브스킬의 추가능력치 총합
    private Status TotalState; //장비,패시브스킬에의한 추가능력치, 등이 합산된 능력치
    public Status GetTotalStatus { get { return TotalState; } }
    public string m_strName { get; private set; } //플레이어의 이름
    public int m_iLevel { get; private set; } //레벨
    public int m_iMaxEXP { get { return (10 * m_iLevel) + (int)Mathf.Pow(m_iLevel, 2); } } //레벨업에 필요한 요구경험치
    public int m_iStatPoint { get; private set; } // 스탯포인트(레밸업시 3획득), 힘,덱스,건강,지능에 투자가능
    public int m_iSkillPoint { get; private set; } //스킬포인트(레밸업시 1획득), 스킬투자가능
    public int m_iCurrentEXP { get; private set; }  //현재 경험치
    public int m_iCurrentHP { get; private set; } //현재체력
    public int m_iCurrentMP { get; private set; } //현재마나
    public float m_fPlayerSpeed { get; private set; } //플레이어이동속도
   // public bool IsPlayerDie { get; private set; } //체력이 0이하로감소하면 true로 바뀜

    public int PowerSwingSkillLevel { get; private set; } //파워스윙스킬레밸
    public int EarthquakeSkillLevel { get; private set; } //지진스킬레밸
    public int ThunderSkillLevel { get; private set; } //번개스킬레밸
    public int AttackIncreaseSkillLevel { get; private set; } //공격력증가스킬레밸
    public int ShieldIncreaseSkillLevel { get; private set; } //방어력증가스킬레밸
    public int CriticalIncreaseSkillLevel { get; private set; } //크리티컬확률스킬레밸
    public int SpeedIncreaseSkillLevel { get; private set; } //이동속도증가스킬레밸
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
    private void SetState() // 레밸업 or 스탯투자시에 불릴 예정
    {
        State.MaxHP = 100 + ((m_iLevel - 1) * 20) + (State.Health * 10);
        State.MaxMP = 50 + ((m_iLevel - 1) * 5) + (State.Int * 5);
        State.Atk = 10 + (State.Str * 5) + State.Dex + State.Int;
        State.Def = 0 + (State.Health * 0.1f);
        State.Critical = 1 + (State.Dex * 0.1f);
    }
    public void SetTotalState() //장비변경, 패시브스킬투자 와 위의 SetState 후에 불릴 예정
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
    public void SetName(string NewName) // 최초 이름입력 및 이름변경등에 사용
    {
        m_strName = NewName;
        UIManager.Instance.SetLvAndNameText(m_iLevel, m_strName);
    }

    public void LevelUpCheck() //경험치 습득시마다 불려짐
    {
        if (m_iCurrentEXP >= m_iMaxEXP)
        {
            LevelChange(); //레벨변경
            SetState(); //캐릭터고유 스탯정보 갱신
            SetTotalState(); //캐릭터토탈(패시브,장착아이템등포함) 스탯정보 갱신
            m_iCurrentHP = TotalState.MaxHP; //HP 회복
            m_iCurrentMP = TotalState.MaxMP; //MP 회복
            UpdateUI(); //UI 일괄갱신
        }
    }
    private void LevelChange() //레밸업
    {
        if (m_iCurrentEXP >= m_iMaxEXP)
        {
            m_iCurrentEXP -= m_iMaxEXP; 
            LevelUP(); // 레벨업
            LevelChange(); //추가 레벨업을 할수있는가?
        }
    }
    private void LevelUP() //레밸업시 레밸및 기초능력증가와 스탯,스킬포인트 지급 
    {
        m_iLevel++; //레벨 1증가
        m_iStatPoint += 3; //스탯포인트 3증가
        m_iSkillPoint++; //스킬포인트 1증가
        UIManager.Instance.RefreshStatusUIInformation(); //스탯창 갱신
        UIManager.Instance.RefreshSkillUISkillPoint(); //스킬창 갱신
        GameManager.instance.AddNewLog(m_strName + "님께서 레밸업을하여 " + m_iLevel.ToString() + "Level이 되셨습니다."); //시스템박스에 정보표시
        PlayerAct.instance.LevelUp(); //레벨업 이팩트 발생
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

        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP - MinusHP, 0, TotalState.MaxHP); //현재체력이 - 값이 되지않도록 조절
        UIManager.Instance.ChangeHPBarCurValue(m_iCurrentHP); //체력바 갱신
        UIManager.Instance.RefreshStatusUIInformation(); //스테이터스창의 체력정보 갱신
        if(MinusHP > 0) //입은 데미지가 0보다 크면 피격이펙트(화면 적색) 활성
        UIManager.Instance.ActiveDammageEffect();

        if (m_iCurrentHP <= 0) //체력이 0보다 적어져서 사망이벤트 발생
        {
            PlayerAct.instance.SetActState(PLAYERACTSTATE.DEAD); //플레이어를 죽음 상태로 변경하고 사망애니메이션 실행
            UIManager.Instance.GetGameOverUI.gameObject.SetActive(true); //화면UI에 "YOU DIED" Text 활성
            StartCoroutine(Resurrection()); //사망애니메이션 실행후 부활이벤트 실행
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
        m_iCurrentEXP += PlusExp; //현재 경험치 양에 새로 얻은 경험치 추가
        LevelUpCheck(); //레벨업 여부 확인하여 만족시 레벨업
        UIManager.Instance.SetExpBarSlider(m_iCurrentEXP, m_iMaxEXP); //경험치바 갱신
        UIManager.Instance.RefreshStatusUIInformation(); //스테이터스창 갱신
    }

    //장비변경후 플레이어 총스테이터스변경
    public void ChangeEquipmentData(Status NewTotalEquipmentData) 
    {
        TotalEquipmentData = NewTotalEquipmentData; //장비능력치합계의 스테이터스
        SetTotalState(); //캐릭터고유능력치+ 장비 + 패시브스킬 총합 스테이터스
    }
    public void DeadPenalty() // 죽으면 체력 마력 최대치의 절반 현재경험치량 10%감소하여 마을에서 부활
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

        if (SkillMaxLevel > CurLevel) //스킬최대레밸 보다 현재스킬레밸이 낮으면 
        {
            if (IsSkillPoint()) //남아있는 스킬포인트가 있으면 
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
                    Debug.LogError("위치 : 플레이어스테이터스클래스.SkillLevelUpByName 함수, Skill이름이 이상합니다");
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

    public void PlayCheatMode() //테스트용 치트모드(능력치 크게오름)
    {
        LoadStateData(PlayerCheatDataName);
        SetState();
        Player.instance.PickupGold(99999);
        SetTotalState();
        UpdateUI();
    }

    IEnumerator Resurrection() //부활
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

    public bool LoadStateData(string FileName) //로드파일
    {
        string FilePath = DataSaveAndLoad.Instance.MakeFilePath(FileName, "/SaveData/StateData/");
        if (!File.Exists(FilePath))
        {
            Debug.LogError("플레이어 상태 세이브파일을 찾지못했습니다.");
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

