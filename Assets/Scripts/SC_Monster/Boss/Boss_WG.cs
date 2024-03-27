using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum BOSSMOB_ACT
{
    WAIT = 0,
    CHASE,
    STEP1, 
    STEP2, 
    STEP3, 
    DEAD,
    END
}

public class Boss_WG : MonoBehaviour
{
    public Head_Machine<Boss_WG> m_StateMachine; //유한상태기계머신
    private SetMonsterData AutoState; //레밸에따른자동능력치셋팅
    public SetMonsterData GetState {get{return AutoState;}}
    [SerializeField] private int m_iLevel; //보스몬스터의 레벨
    [SerializeField] private GameObject BerserkAura; //광폭화시 활성화시킬 오라
    private FSM<Boss_WG>[] m_arrState = new FSM<Boss_WG>[(int)BOSSMOB_ACT.END];

    [SerializeField] private int m_iCurrentHP; //현재체력
    public BOSSMOB_ACT m_ePrevState; //이전상태
    public BOSSMOB_ACT m_eCurState; //현재상태
    private BOSSMOB_ACT Phase; //보스의 단계
    [SerializeField] private LayerMask whatIsTarget; // 추적 대상 레이어
    private Transform TargetObject; //추적할 타겟
    private Collider TargetCollider; 
    [SerializeField] private Collider PowerSwingRange; //파워스윙범위
    [SerializeField] private Collider UpperPunchRange; //어퍼펀치범위 
    [SerializeField] private ParticleSystem hitEffect; // 피격시 재생할 파티클 효과
    [SerializeField] private AudioClip deathSound; // 사망시 재생할 소리
    [SerializeField] private AudioClip hitSound; // 피격시 재생할 소리
    [SerializeField] private AudioClip PowerSwingSound; // 패턴1 재생할 소리
    [SerializeField] private AudioClip UpperPunchSound; // 패턴2 재생할 소리
    [SerializeField] private AudioClip SummonRockSound; // 패턴3 재생할 소리

    [SerializeField] private Animator m_Animator; // 애니메이터 컴포넌트
    [SerializeField] private NavMeshAgent pathFinder; // 경로계산 AI 에이전트
    [SerializeField] private SliderValueController HPBar; //보스몬스터 체력바
    [SerializeField] private FloatingDamageText DamageText; //머리위에 데미지폰트
    private float m_flastAttackTime; // 마지막 공격 시점(공격애니메이션이 끝난시점부터 카운팅)
    public float m_fDistance { get; private set; } //플레이어와의 거리
    [SerializeField] private float range = 10; //플레이어 서치범위
    public bool m_bIsChase { get { return (range >= m_fDistance); } } //일정거리안에 플레이어가 다가오면 배틀시작
    
    //몬스터의 애니메이션 재생시간
    public readonly float PowerSwing_AnimTime = 5.0f;
    public readonly float UpperPunch_AnimTime = 5.0f;
    public readonly float SummonRoot_AnimTime = 7.0f;
    public readonly float Wakeup_AnimTime = 6.0f;
    public readonly float DieAnimTime = 6.0f;
    // public readonly float WG_Pattern1DamagerTerm = 3.0f; //패턴1 공격모션 시전후 공격데미지 들어갈떄 까지의 텀
    // public readonly float WG_Pattern2DamagerTerm = 3.0f; //패턴2 공격모션 시전후 공격데미지 들어갈떄 까지의 텀
    private const float PaternDamageTerm = 3.0f; //패턴1,2 공격모션후 데미지 들어갈떄까지의 텀

    [SerializeField] private int m_iSummonServantNumber = 3; // 한번에 소환할 부하몬스터의 숫자
    [SerializeField] private SummonedAttack SA; //바위소환공격 오브젝트
    public bool m_bSummonAttackFlag; //3페이지에서 플레이어가 도망만 다닐경우 일정시간마다 강제로 공격페이지로 전환후 소환공격하기 위한 불변수
    private float m_fSummonAttackClock; //강제 3페이지 공격모션으로 전환하기 위한 타이머
    private readonly float ChangeSummonAttackTime = 8f; //강제 바위소환공격 쿨타임
    private bool m_bInvincibilityFlag; // true 이면 무적
    public bool m_bPowerSwingActiveFlag; //파워스윙공격범위 활성화
    public bool m_bUpperPunchActiveFlag; //어퍼펀치공격범위 활성화


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }

    public void Init()
    {
        m_StateMachine = new Head_Machine<Boss_WG>();
        m_arrState[(int)BOSSMOB_ACT.WAIT] = new WG_Wait(this);
        m_arrState[(int)BOSSMOB_ACT.CHASE] = new WG_Chase(this);
        m_arrState[(int)BOSSMOB_ACT.STEP1] = new WG_Step1(this);
        m_arrState[(int)BOSSMOB_ACT.STEP2] = new WG_Step2(this);
        m_arrState[(int)BOSSMOB_ACT.STEP3] = new WG_Step3(this);
        m_arrState[(int)BOSSMOB_ACT.DEAD] = new WG_Dead(this);

        m_StateMachine.SetState(m_arrState[(int)BOSSMOB_ACT.WAIT], this);
    }

    public void ChangeFSM(BOSSMOB_ACT ps)
    {
        for(int i = (int)BOSSMOB_ACT.WAIT; i < (int)BOSSMOB_ACT.END; i++)
        {
            if (i == (int)ps)
                m_StateMachine.Change(m_arrState[(int)ps]);
        }
    }

    public void Begin()
    {
        m_StateMachine.Begin();
    }

    public void Run()
    {
        m_StateMachine.Run();
    }

    public void Exit()
    {
        m_StateMachine.Exit();
    }
 
    private void Awake()
    {
        Init();
        //pathFinder = GetComponent<NavMeshAgent>();
        //m_Animator = GetComponent<Animator>();
       //  m_Renderer = GetComponentInChildren<Renderer>();

        AutoState = new SetMonsterData(m_iLevel, MONSTER_TYPE.WOODGOLLEM);
        TargetObject = GameManager.instance.playerobject.transform;
        TargetCollider = GameManager.instance.playercollider;
        m_iCurrentHP = AutoState.State.MaxHP;
        m_eCurState = BOSSMOB_ACT.WAIT;
        pathFinder.isStopped = true;
        pathFinder.speed = AutoState.State.Speed;
        Phase = BOSSMOB_ACT.STEP1;
        m_bSummonAttackFlag = false;
        m_fSummonAttackClock = 0;
        m_bInvincibilityFlag = false;
        m_bUpperPunchActiveFlag = false;
        m_bPowerSwingActiveFlag = false;
       
        if (HPBar)
        {
            HPBar.Init(m_iCurrentHP, AutoState.State.MaxHP);
            HPBar.SetText(m_iCurrentHP);
            HPBar.ActiveSlider();
        }

        StartCoroutine(SetDamageTextPosition());
        DamageText.gameObject.SetActive(false);
    }
    private void Start()
    {
        Begin();
    }
    private void Update()
    {
        m_fDistance = Vector3.Distance(TargetObject.position, transform.position);
        Run();
    }

    public void OnDamage(int Damage)
    {
        if (m_eCurState == BOSSMOB_ACT.WAIT || m_eCurState == BOSSMOB_ACT.DEAD) //대기상태일떈 무적
            return;

        int TrueDamage = TrueDamage = Mathf.Clamp(Damage - AutoState.State.Def, 0, GameManager.instance.MaxDammage);

        if (TrueDamage > 0)
            m_iCurrentHP -= TrueDamage;

        DamageText.SetText(TrueDamage.ToString());
        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP, 0, AutoState.State.MaxHP);

        if (HPBar) HPBar.SetText(m_iCurrentHP);
        ChangePhase();
        
        if (m_iCurrentHP <= 0)
            ChangeFSM(BOSSMOB_ACT.DEAD);
    }
    public void Chase()
    {
        m_Animator.SetFloat("Velocity", pathFinder.velocity.magnitude);
        pathFinder.SetDestination(TargetObject.position);

        if (Phase == BOSSMOB_ACT.STEP3 && m_bSummonAttackFlag == false)
        {
            if (ChangeSummonAttackTime <= (m_fSummonAttackClock += Time.deltaTime))
            {
                m_fSummonAttackClock = 0;
                m_bSummonAttackFlag = true;
                ChangeFSM(BOSSMOB_ACT.STEP3);
            }
        }
    }
    public void OnTrigger(string TriggerName)
    {
        m_Animator.SetTrigger(TriggerName);
    }
    public void ChangePhase()
    {
        if (Phase == BOSSMOB_ACT.STEP1 && m_iCurrentHP <= (AutoState.State.MaxHP * 0.66f))
        {
            Phase = BOSSMOB_ACT.STEP2;
            EnableAura();
            AutoState.Berserk();
        }
        else if (Phase == BOSSMOB_ACT.STEP2 && m_iCurrentHP <= (AutoState.State.MaxHP * 0.33f))
        {
            BGMChange(3);
            Phase = BOSSMOB_ACT.STEP3;
        }
    }
    public void DoDisable()
    {
        gameObject.SetActive(false);
    }
    public void StopNav() //공격모션 중 플레이어 추격기능멈춤
    {
        pathFinder.isStopped = true;
    }
    public void StartNav() // 공격모션 종료후 다시 플레이어 추격
    {
        pathFinder.isStopped = false;
    }

    private void OnTriggerStay(Collider other) 
    {
        //파워스윙공격데미지판정
        if (m_bPowerSwingActiveFlag == true && other.tag == "Player")
        {
            GiveDamageToPlayer(1.0f);
        }

        //어퍼펀치공격데미지판정
        if (m_bUpperPunchActiveFlag == true && other.tag == "Player")
        {
            GiveDamageToPlayer(1.5f);
        }

        //플레이어에게 접근시 공격
        if (m_eCurState == BOSSMOB_ACT.CHASE && other == TargetCollider)
        {

            // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
            if (m_eCurState != BOSSMOB_ACT.DEAD && Time.time >= m_flastAttackTime + AutoState.State.AttackTerm)
            {
                m_flastAttackTime = Time.time;
                switch (Phase)
                {
                    case BOSSMOB_ACT.STEP1:
                        ChangeFSM(BOSSMOB_ACT.STEP1);
                        break;
                    case BOSSMOB_ACT.STEP2:
                        ChangeFSM(BOSSMOB_ACT.STEP2);
                        break;
                    case BOSSMOB_ACT.STEP3:
                        ChangeFSM(BOSSMOB_ACT.STEP3);
                        break;
                    default:
                        break;
                }

            }
        }
        //피격판정
        if (m_bInvincibilityFlag == false && m_eCurState != BOSSMOB_ACT.WAIT)
        {
            if (other.tag == "Zone" && m_iCurrentHP > 0)
            {
                int Damage = GameManager.instance.DamageCalculate(other.name);
                Damage += (int)(UnityEngine.Random.Range(-Damage * 0.1f, Damage * 0.1f));
                GameManager.instance.AddNewLog(Damage.ToString());
                OnDamage(Damage);
                SoundManager.Instance.PlayEffectSoundForOnce(hitSound);
                StartCoroutine(ActiveInvincibility());
            }
        }
    }

    public void SummonServants()
    {
        MonsterSpawner.instance.SummonMonster(m_iSummonServantNumber);
    }
    public void SummonAttack()
    {
        SA.Init(AutoState.State.Atk * 2, TargetObject.transform);
        SA.gameObject.SetActive(true);

        SA.Disappear += () => SA.ClearDisapper();
        SA.Disappear += () => SA.gameObject.SetActive(false);

    }
    public void EnableAura() //오라 활성화 
    {
        BerserkAura.SetActive(true);
    }
    public void SetHPBarDisable() //체력바 비활성화
    {
        if (HPBar) HPBar.gameObject.SetActive(false);
    }
    IEnumerator ActiveInvincibility() //무적시간 활성화
    {
        m_bInvincibilityFlag = true;
        yield return new WaitForSeconds(0.2f);
        m_bInvincibilityFlag = false;
    }

    IEnumerator ActiveAttackRange(Collider collider) //공격범위활성화
    {
        string PatternName = collider.name;

        if (PatternName == "PowerSwingRange")
            SoundManager.Instance.PlayEffectSoundForOnce(PowerSwingSound);
        else
            SoundManager.Instance.PlayEffectSoundForOnce(UpperPunchSound);

        yield return new WaitForSeconds(PaternDamageTerm);
        collider.gameObject.SetActive(true);
        
        if (PatternName == "PowerSwingRange")
            m_bPowerSwingActiveFlag = true;
        else
            m_bUpperPunchActiveFlag = true;

        yield return new WaitForSeconds(0.1f);
        collider.gameObject.SetActive(false);
        
        if (PatternName == "PowerSwingRange")
            m_bPowerSwingActiveFlag = false;
        else
            m_bUpperPunchActiveFlag = false;
    }

    public void ActPowerSwingRange()
    {
        StartCoroutine(ActiveAttackRange(PowerSwingRange));
    }

    public void ActUpperPunchRange()
    {
        StartCoroutine(ActiveAttackRange(UpperPunchRange));
    }

    public void GiveDamageToPlayer(float Num) //Num에는 데미지배율
    {
        int FinalDamage = (int)(AutoState.State.Atk * Num);
        Player.instance.GetDamage(FinalDamage);
    }
    public void PlaySummonAttackSound() //바위소환공격 사운드재생
    {
        SoundManager.Instance.PlayEffectSoundForOnce(SummonRockSound);
    }

    public void PlayAwakeSound() //기상시 사운드발생
    {
        SoundManager.Instance.PlayEffectSoundForOnce(PowerSwingSound);
    }

    public void OnDeath()
    {
        m_eCurState = BOSSMOB_ACT.DEAD;

        pathFinder.ResetPath();
        m_bInvincibilityFlag = true;
        BerserkAura.SetActive(false);

        OnTrigger("Dead");
        SoundManager.Instance.PlayEffectSoundForOnce(deathSound);
        if(MonsterSpawner.instance)
        MonsterSpawner.instance.ClearField();
        DropItem();
        var Exp = AutoState.State.Exp;
        Player.instance.GetPlayerStatus.GetExp(Exp);
        GameManager.instance.AddNewLog("경험치 " + Exp.ToString() + " 획득");
    }

    public void DropItem()
    {
        var ResultState = AutoState.State;
        var Postion = gameObject.transform.position;
        ItemManager.instance.CreateDropItemWithDropRate(ResultState.DropItemName, 1, ResultState.NormalDropRate, Postion);
        ItemManager.instance.CreateDropItemWithDropRate(ResultState.DropRareItemName, 1, ResultState.RareDropRate, Postion);
        ItemManager.instance.CreateRandomGoldItem(ResultState.Gold, Postion);
    }

    IEnumerator SetDamageTextPosition()
    {
        while (m_eCurState != BOSSMOB_ACT.DEAD)
        {
            DamageText.SetPos(Camera.main.WorldToScreenPoint(gameObject.transform.position + new Vector3(0, 6, 0)));
            yield return null;
        }
    }

    public void BGMChange(int Phase)
    {
        SoundManager.Instance.BossBGMChange(Phase);
    }
}
