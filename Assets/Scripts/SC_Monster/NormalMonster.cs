using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public enum MONSTERACTIONSTATE
{
    PEACE = 0,
    CHASE,
    ATTACK,
    DEAD
}
public class NormalMonster : MonoBehaviour
{
    private SetMonsterData AutoState; //레밸에 따라 자동으로 바뀌는 스테이터스
    public int MaxHP { get { return (null != AutoState) ? AutoState.State.MaxHP : 0; } }
    [SerializeField] private int m_iLevel;
    [SerializeField] private MONSTER_TYPE MobType;
    public MONSTER_TYPE GetMonsterType { get { return MobType; } }
    public int m_iCurrentHP;
    private MONSTERACTIONSTATE ActState;
    [SerializeField] private LayerMask whatIsTarget; // 추적 대상 레이어
    private Transform TargetObject; //추적할 타겟
    private Collider TargetCollider;
    [SerializeField] private float m_fChaseDistance; //추적시작 거리
    [SerializeField] private Collider AttackRange; //공격범위
    [SerializeField] private AudioClip deathSound; // 사망시 재생할 소리
    [SerializeField] private AudioClip hitSound; // 피격시 재생할 소리
    private Animator MobAnimator; // 애니메이터 컴포넌트
    private NavMeshAgent pathFinder; // 경로계산 AI 에이전트
    private bool m_bInvincibilityFlag; // true 이면 무적
    private float m_flastAttackTime; // 마지막 공격 시점(공격애니메이션이 끝난시점부터 카운팅)
    private float m_fDistance; //플레이어와의 거리
    private bool m_bIsMove; // 죽으면 false 살아있을땐 true 로하여 사망애니메이션 재생중 update에서 죽음상태 반복호출막는용도
    [SerializeField] private float PatrolTime = 0; //정찰주기
    private float m_fPatrolClock = 0; //정찰주기 메모타이머
    private float m_fAttackClock = 0; //공격주기 메모타이머
    public event Action OnDeath;
    public event Action<int> SetHPBar;
    public event Action<int> SetDamageText;
    private const float MonsterAttackDamageTerm = 1.1f; //몬스터들의 공격모션 시전후 공격데미지 들어갈떄 까지의 텀

    public float HpBarYPlusValue { get; private set; } //체력바 위치 조절치
    public float FloatingDamageText_YPlusValue { get; private set; } //데미지텍스트 위치 조절치
    private bool m_bAttackRangeActiveFlag; //공격범위 활성화


    public void Init(int Level, Transform tr)
    {
        this.m_iLevel = Level;
        transform.position = tr.position;
        transform.rotation = tr.rotation;
        reset();

    }

    private void Awake()
    { 
        // 초기화
        pathFinder = GetComponent<NavMeshAgent>(); 
        MobAnimator = GetComponent<Animator>();
        //MobRenderer = GetComponentInChildren<Renderer>();
        TargetObject = GameManager.instance.playerobject.transform;
        TargetCollider = GameManager.instance.playercollider;
        m_bInvincibilityFlag = false;
        m_bAttackRangeActiveFlag = false;
        SetHPBarYPlusValue();
    }
    private void SetHPBarYPlusValue()
    {
        switch (MobType)
        {
            case MONSTER_TYPE.BABYWOOD:
                HpBarYPlusValue = 1.4f;
                FloatingDamageText_YPlusValue = 2.4f;
                break;
            case MONSTER_TYPE.ROCKSNAIL:
                HpBarYPlusValue = 1.7f;
                FloatingDamageText_YPlusValue = 2.7f;
                break;
            case MONSTER_TYPE.PUNCHTREE:
                HpBarYPlusValue = 2.2f;
                FloatingDamageText_YPlusValue = 3.2f;
                break;
            case MONSTER_TYPE.MINIGOLLEM:
                HpBarYPlusValue = 2.0f;
                FloatingDamageText_YPlusValue = 3.0f;
                break;
        }
    }
    public void reset()
    {
        AutoState = new SetMonsterData(m_iLevel, MobType);
        m_iCurrentHP = AutoState.State.MaxHP;
        ActState = MONSTERACTIONSTATE.PEACE;
        m_bIsMove = true;
        PatrolTime = UnityEngine.Random.Range(2.5f, 3.5f);

        pathFinder.speed = AutoState.State.Speed;
    }
    public void ClearDeadEvent()
    {
        OnDeath = null;
        SetDamageText = null;
        SetHPBar = null;
    }
    private void OnEnable()
    {
        StartCoroutine(Act());
    }
 
    public void OnDamage(int Damage)
    {
        if (ActState == MONSTERACTIONSTATE.DEAD)
            return;

        if (ActState != MONSTERACTIONSTATE.ATTACK)
        ActState = MONSTERACTIONSTATE.CHASE;

        int TrueDamage = TrueDamage = Mathf.Clamp(Damage - AutoState.State.Def, 0, GameManager.instance.MaxDammage);

        if (TrueDamage > 0)
            m_iCurrentHP -= TrueDamage;

        m_iCurrentHP = Mathf.Clamp(m_iCurrentHP, 0, AutoState.State.MaxHP);
       

        if (m_iCurrentHP <= 0)
        {
            ActState = MONSTERACTIONSTATE.DEAD;
            Player.instance.GetPlayerStatus.GetExp(AutoState.State.Exp);
        }
        if(SetHPBar != null)
        SetHPBar(m_iCurrentHP);
        //if (SetDamageText != null)
        //SetDamageText(TrueDamage);
      
        SetDamageText?.Invoke(TrueDamage);
    }
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle 상태일시 주변배회를 위해 자신주변 원형으로 특정좌표를 return 해주는 함수
    {
        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * distance + center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);
        return hit.position;
    }
    private void OnTriggerStay(Collider other) 
    {
        //플레이어에게 접근시 공격
        if (ActState == MONSTERACTIONSTATE.CHASE && other == TargetCollider && ActState != MONSTERACTIONSTATE.DEAD)
        {

            // 트리거 충돌한 상대방 게임 오브젝트가 추적 대상이라면 공격 실행
            if (ActState != MONSTERACTIONSTATE.DEAD && Time.time >= m_flastAttackTime + AutoState.State.AttackTerm)
            {
                m_flastAttackTime = Time.time;
                pathFinder.isStopped = true;
                MobAnimator.SetTrigger("Attack"); 
                ActState = MONSTERACTIONSTATE.ATTACK;
                StartCoroutine(ActiveAttackRange(MonsterAttackDamageTerm));
            }
        }
        //피격이벤트
        if (m_bInvincibilityFlag == false && ActState != MONSTERACTIONSTATE.DEAD)
        {
            if (other.tag == "Zone")
            {
                int Damage = GameManager.instance.DamageCalculate(other.name);
                Damage += (int)(UnityEngine.Random.Range(-Damage * 0.1f, Damage * 0.1f));
                OnDamage(Damage);
                SoundManager.Instance.PlayEffectSoundForOnce(hitSound);
                StartCoroutine(ActiveInvincibility());
            }
        }

        //공격데미지이벤트
        if(m_bAttackRangeActiveFlag == true && other.tag == "Player")
        {
            Player.instance.GetDamage(AutoState.State.Atk);
        }
    }

    private IEnumerator Act()
    {
        if (!TargetObject) yield break; //목표대상이 없으면 break
        while (m_bIsMove) //움직일수 있는 상태면
        {
            m_fDistance = Vector3.Distance(TargetObject.position, transform.position); //플레이어와의 거리
            switch (ActState) //상태에따라
            {
                case MONSTERACTIONSTATE.PEACE: //평화로운 상태
                    {
                        MobAnimator.SetFloat("Velocity", pathFinder.velocity.magnitude); 
                        if (PatrolTime <= (m_fPatrolClock += Time.deltaTime)) //정찰주기
                        {
                            m_fPatrolClock = 0; 
                            pathFinder.SetDestination(GetRandomPointOnNavMesh(gameObject.transform.position, 2)); //일정거리 정찰
                        }
                        
                        if (m_fChaseDistance >= m_fDistance) //플레이어가 다가오면 상태변경
                            ActState = MONSTERACTIONSTATE.CHASE;
                    }
                    break;
                case MONSTERACTIONSTATE.CHASE: //추격상태
                    {
                        MobAnimator.SetFloat("Velocity", pathFinder.velocity.magnitude);
                        pathFinder.SetDestination(TargetObject.position);

                        if (m_fChaseDistance < m_fDistance) //플레이어가 멀어지면 다시 평화로운 상태로 변경
                            ActState = MONSTERACTIONSTATE.PEACE;
                    }
                    break;
                case MONSTERACTIONSTATE.ATTACK: //공격상태
                    {
                        if (AutoState.State.AttackAnimTime <= (m_fAttackClock += Time.deltaTime))
                        {
                            m_fAttackClock = 0;
                            pathFinder.isStopped = false;
                            ActState = MONSTERACTIONSTATE.CHASE; //다시 추격
                        }
                    }
                    break;
                case MONSTERACTIONSTATE.DEAD:
                    {
                        pathFinder.isStopped = true;
                        // pathFinder.enabled = false;
                        MobAnimator.SetTrigger("Dead");
                        MobAnimator.SetBool("Die", true); 
                        var Exp = AutoState.State.Exp;
                        Player.instance.GetPlayerStatus.GetExp(Exp); //플레이어에게 경험치
                        GameManager.instance.AddNewLog("경험치 " + Exp.ToString() + " 획득");
                        SoundManager.Instance.PlayEffectSoundForOnce(deathSound); //사망효과음
                        m_bIsMove = false;
                        m_bAttackRangeActiveFlag = false;
                        yield return new WaitForSeconds(AutoState.State.DieAnimTime);
                        DropItem(); //아이템 드롭

                        if (OnDeath != null)
                        {
                            OnDeath();
                            gameObject.SetActive(false);
                        }
                        yield break;
                    }
            }
            yield return null;
        }
    }

    IEnumerator ActiveInvincibility()
    {
        m_bInvincibilityFlag = true;
        yield return new WaitForSeconds(0.1f);
        m_bInvincibilityFlag = false;
    }

    IEnumerator ActiveAttackRange(float DelayTime)
    {
        yield return new WaitForSeconds(DelayTime);

        AttackRange.gameObject.SetActive(true);
        m_bAttackRangeActiveFlag = true;

        yield return new WaitForSeconds(0.2f);

        AttackRange.gameObject.SetActive(false);
        m_bAttackRangeActiveFlag = false;
    }

    public void DropItem()
    {
        var ResultState = AutoState.State;
        var Postion = gameObject.transform.position;
        ItemManager.instance.CreateDropItemWithDropRate(ResultState.DropItemName, 1, ResultState.NormalDropRate, Postion);
        ItemManager.instance.CreateDropItemWithDropRate(ResultState.DropRareItemName, 1, ResultState.RareDropRate, Postion);
        ItemManager.instance.CreateRandomGoldItem(ResultState.Gold, Postion);

    }
}
