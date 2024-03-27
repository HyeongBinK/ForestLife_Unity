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
    private SetMonsterData AutoState; //���뿡 ���� �ڵ����� �ٲ�� �������ͽ�
    public int MaxHP { get { return (null != AutoState) ? AutoState.State.MaxHP : 0; } }
    [SerializeField] private int m_iLevel;
    [SerializeField] private MONSTER_TYPE MobType;
    public MONSTER_TYPE GetMonsterType { get { return MobType; } }
    public int m_iCurrentHP;
    private MONSTERACTIONSTATE ActState;
    [SerializeField] private LayerMask whatIsTarget; // ���� ��� ���̾�
    private Transform TargetObject; //������ Ÿ��
    private Collider TargetCollider;
    [SerializeField] private float m_fChaseDistance; //�������� �Ÿ�
    [SerializeField] private Collider AttackRange; //���ݹ���
    [SerializeField] private AudioClip deathSound; // ����� ����� �Ҹ�
    [SerializeField] private AudioClip hitSound; // �ǰݽ� ����� �Ҹ�
    private Animator MobAnimator; // �ִϸ����� ������Ʈ
    private NavMeshAgent pathFinder; // ��ΰ�� AI ������Ʈ
    private bool m_bInvincibilityFlag; // true �̸� ����
    private float m_flastAttackTime; // ������ ���� ����(���ݾִϸ��̼��� ������������ ī����)
    private float m_fDistance; //�÷��̾���� �Ÿ�
    private bool m_bIsMove; // ������ false ��������� true ���Ͽ� ����ִϸ��̼� ����� update���� �������� �ݺ�ȣ�⸷�¿뵵
    [SerializeField] private float PatrolTime = 0; //�����ֱ�
    private float m_fPatrolClock = 0; //�����ֱ� �޸�Ÿ�̸�
    private float m_fAttackClock = 0; //�����ֱ� �޸�Ÿ�̸�
    public event Action OnDeath;
    public event Action<int> SetHPBar;
    public event Action<int> SetDamageText;
    private const float MonsterAttackDamageTerm = 1.1f; //���͵��� ���ݸ�� ������ ���ݵ����� ���� ������ ��

    public float HpBarYPlusValue { get; private set; } //ü�¹� ��ġ ����ġ
    public float FloatingDamageText_YPlusValue { get; private set; } //�������ؽ�Ʈ ��ġ ����ġ
    private bool m_bAttackRangeActiveFlag; //���ݹ��� Ȱ��ȭ


    public void Init(int Level, Transform tr)
    {
        this.m_iLevel = Level;
        transform.position = tr.position;
        transform.rotation = tr.rotation;
        reset();

    }

    private void Awake()
    { 
        // �ʱ�ȭ
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
    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance) //Idle �����Ͻ� �ֺ���ȸ�� ���� �ڽ��ֺ� �������� Ư����ǥ�� return ���ִ� �Լ�
    {
        Vector3 randomPos = UnityEngine.Random.insideUnitSphere * distance + center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);
        return hit.position;
    }
    private void OnTriggerStay(Collider other) 
    {
        //�÷��̾�� ���ٽ� ����
        if (ActState == MONSTERACTIONSTATE.CHASE && other == TargetCollider && ActState != MONSTERACTIONSTATE.DEAD)
        {

            // Ʈ���� �浹�� ���� ���� ������Ʈ�� ���� ����̶�� ���� ����
            if (ActState != MONSTERACTIONSTATE.DEAD && Time.time >= m_flastAttackTime + AutoState.State.AttackTerm)
            {
                m_flastAttackTime = Time.time;
                pathFinder.isStopped = true;
                MobAnimator.SetTrigger("Attack"); 
                ActState = MONSTERACTIONSTATE.ATTACK;
                StartCoroutine(ActiveAttackRange(MonsterAttackDamageTerm));
            }
        }
        //�ǰ��̺�Ʈ
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

        //���ݵ������̺�Ʈ
        if(m_bAttackRangeActiveFlag == true && other.tag == "Player")
        {
            Player.instance.GetDamage(AutoState.State.Atk);
        }
    }

    private IEnumerator Act()
    {
        if (!TargetObject) yield break; //��ǥ����� ������ break
        while (m_bIsMove) //�����ϼ� �ִ� ���¸�
        {
            m_fDistance = Vector3.Distance(TargetObject.position, transform.position); //�÷��̾���� �Ÿ�
            switch (ActState) //���¿�����
            {
                case MONSTERACTIONSTATE.PEACE: //��ȭ�ο� ����
                    {
                        MobAnimator.SetFloat("Velocity", pathFinder.velocity.magnitude); 
                        if (PatrolTime <= (m_fPatrolClock += Time.deltaTime)) //�����ֱ�
                        {
                            m_fPatrolClock = 0; 
                            pathFinder.SetDestination(GetRandomPointOnNavMesh(gameObject.transform.position, 2)); //�����Ÿ� ����
                        }
                        
                        if (m_fChaseDistance >= m_fDistance) //�÷��̾ �ٰ����� ���º���
                            ActState = MONSTERACTIONSTATE.CHASE;
                    }
                    break;
                case MONSTERACTIONSTATE.CHASE: //�߰ݻ���
                    {
                        MobAnimator.SetFloat("Velocity", pathFinder.velocity.magnitude);
                        pathFinder.SetDestination(TargetObject.position);

                        if (m_fChaseDistance < m_fDistance) //�÷��̾ �־����� �ٽ� ��ȭ�ο� ���·� ����
                            ActState = MONSTERACTIONSTATE.PEACE;
                    }
                    break;
                case MONSTERACTIONSTATE.ATTACK: //���ݻ���
                    {
                        if (AutoState.State.AttackAnimTime <= (m_fAttackClock += Time.deltaTime))
                        {
                            m_fAttackClock = 0;
                            pathFinder.isStopped = false;
                            ActState = MONSTERACTIONSTATE.CHASE; //�ٽ� �߰�
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
                        Player.instance.GetPlayerStatus.GetExp(Exp); //�÷��̾�� ����ġ
                        GameManager.instance.AddNewLog("����ġ " + Exp.ToString() + " ȹ��");
                        SoundManager.Instance.PlayEffectSoundForOnce(deathSound); //���ȿ����
                        m_bIsMove = false;
                        m_bAttackRangeActiveFlag = false;
                        yield return new WaitForSeconds(AutoState.State.DieAnimTime);
                        DropItem(); //������ ���

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
