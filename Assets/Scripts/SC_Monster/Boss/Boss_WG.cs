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
    public Head_Machine<Boss_WG> m_StateMachine; //���ѻ��±��ӽ�
    private SetMonsterData AutoState; //���뿡�����ڵ��ɷ�ġ����
    public SetMonsterData GetState {get{return AutoState;}}
    [SerializeField] private int m_iLevel; //���������� ����
    [SerializeField] private GameObject BerserkAura; //����ȭ�� Ȱ��ȭ��ų ����
    private FSM<Boss_WG>[] m_arrState = new FSM<Boss_WG>[(int)BOSSMOB_ACT.END];

    [SerializeField] private int m_iCurrentHP; //����ü��
    public BOSSMOB_ACT m_ePrevState; //��������
    public BOSSMOB_ACT m_eCurState; //�������
    private BOSSMOB_ACT Phase; //������ �ܰ�
    [SerializeField] private LayerMask whatIsTarget; // ���� ��� ���̾�
    private Transform TargetObject; //������ Ÿ��
    private Collider TargetCollider; 
    [SerializeField] private Collider PowerSwingRange; //�Ŀ���������
    [SerializeField] private Collider UpperPunchRange; //������ġ���� 
    [SerializeField] private ParticleSystem hitEffect; // �ǰݽ� ����� ��ƼŬ ȿ��
    [SerializeField] private AudioClip deathSound; // ����� ����� �Ҹ�
    [SerializeField] private AudioClip hitSound; // �ǰݽ� ����� �Ҹ�
    [SerializeField] private AudioClip PowerSwingSound; // ����1 ����� �Ҹ�
    [SerializeField] private AudioClip UpperPunchSound; // ����2 ����� �Ҹ�
    [SerializeField] private AudioClip SummonRockSound; // ����3 ����� �Ҹ�

    [SerializeField] private Animator m_Animator; // �ִϸ����� ������Ʈ
    [SerializeField] private NavMeshAgent pathFinder; // ��ΰ�� AI ������Ʈ
    [SerializeField] private SliderValueController HPBar; //�������� ü�¹�
    [SerializeField] private FloatingDamageText DamageText; //�Ӹ����� ��������Ʈ
    private float m_flastAttackTime; // ������ ���� ����(���ݾִϸ��̼��� ������������ ī����)
    public float m_fDistance { get; private set; } //�÷��̾���� �Ÿ�
    [SerializeField] private float range = 10; //�÷��̾� ��ġ����
    public bool m_bIsChase { get { return (range >= m_fDistance); } } //�����Ÿ��ȿ� �÷��̾ �ٰ����� ��Ʋ����
    
    //������ �ִϸ��̼� ����ð�
    public readonly float PowerSwing_AnimTime = 5.0f;
    public readonly float UpperPunch_AnimTime = 5.0f;
    public readonly float SummonRoot_AnimTime = 7.0f;
    public readonly float Wakeup_AnimTime = 6.0f;
    public readonly float DieAnimTime = 6.0f;
    // public readonly float WG_Pattern1DamagerTerm = 3.0f; //����1 ���ݸ�� ������ ���ݵ����� ���� ������ ��
    // public readonly float WG_Pattern2DamagerTerm = 3.0f; //����2 ���ݸ�� ������ ���ݵ����� ���� ������ ��
    private const float PaternDamageTerm = 3.0f; //����1,2 ���ݸ���� ������ ���������� ��

    [SerializeField] private int m_iSummonServantNumber = 3; // �ѹ��� ��ȯ�� ���ϸ����� ����
    [SerializeField] private SummonedAttack SA; //������ȯ���� ������Ʈ
    public bool m_bSummonAttackFlag; //3���������� �÷��̾ ������ �ٴҰ�� �����ð����� ������ ������������ ��ȯ�� ��ȯ�����ϱ� ���� �Һ���
    private float m_fSummonAttackClock; //���� 3������ ���ݸ������ ��ȯ�ϱ� ���� Ÿ�̸�
    private readonly float ChangeSummonAttackTime = 8f; //���� ������ȯ���� ��Ÿ��
    private bool m_bInvincibilityFlag; // true �̸� ����
    public bool m_bPowerSwingActiveFlag; //�Ŀ��������ݹ��� Ȱ��ȭ
    public bool m_bUpperPunchActiveFlag; //������ġ���ݹ��� Ȱ��ȭ


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
        if (m_eCurState == BOSSMOB_ACT.WAIT || m_eCurState == BOSSMOB_ACT.DEAD) //�������ϋ� ����
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
    public void StopNav() //���ݸ�� �� �÷��̾� �߰ݱ�ɸ���
    {
        pathFinder.isStopped = true;
    }
    public void StartNav() // ���ݸ�� ������ �ٽ� �÷��̾� �߰�
    {
        pathFinder.isStopped = false;
    }

    private void OnTriggerStay(Collider other) 
    {
        //�Ŀ��������ݵ���������
        if (m_bPowerSwingActiveFlag == true && other.tag == "Player")
        {
            GiveDamageToPlayer(1.0f);
        }

        //������ġ���ݵ���������
        if (m_bUpperPunchActiveFlag == true && other.tag == "Player")
        {
            GiveDamageToPlayer(1.5f);
        }

        //�÷��̾�� ���ٽ� ����
        if (m_eCurState == BOSSMOB_ACT.CHASE && other == TargetCollider)
        {

            // Ʈ���� �浹�� ���� ���� ������Ʈ�� ���� ����̶�� ���� ����
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
        //�ǰ�����
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
    public void EnableAura() //���� Ȱ��ȭ 
    {
        BerserkAura.SetActive(true);
    }
    public void SetHPBarDisable() //ü�¹� ��Ȱ��ȭ
    {
        if (HPBar) HPBar.gameObject.SetActive(false);
    }
    IEnumerator ActiveInvincibility() //�����ð� Ȱ��ȭ
    {
        m_bInvincibilityFlag = true;
        yield return new WaitForSeconds(0.2f);
        m_bInvincibilityFlag = false;
    }

    IEnumerator ActiveAttackRange(Collider collider) //���ݹ���Ȱ��ȭ
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

    public void GiveDamageToPlayer(float Num) //Num���� ����������
    {
        int FinalDamage = (int)(AutoState.State.Atk * Num);
        Player.instance.GetDamage(FinalDamage);
    }
    public void PlaySummonAttackSound() //������ȯ���� �������
    {
        SoundManager.Instance.PlayEffectSoundForOnce(SummonRockSound);
    }

    public void PlayAwakeSound() //���� ����߻�
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
        GameManager.instance.AddNewLog("����ġ " + Exp.ToString() + " ȹ��");
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
