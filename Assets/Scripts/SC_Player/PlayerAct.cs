
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum PLAYERACTSTATE
{
    FREE = 0, // �⺻����(�̻��°� �ƴϸ� ��ų���Ұ�)
    ATTACK, //�⺻����
    ROLLING, //������(ȸ�ǻ��·�) �����������Ÿ��̵��� ��������
    POWERSWING, //�Ŀ�������ų
    EARTHQUAKE, //������ų
    THUNDER, //������ų
    STRUN, //���������̻�
    DEAD //���
}

[System.Serializable]
public struct ActionMatch 
{
    public PLAYERACTSTATE PlayerActState;
    public ParticleSystem Particle;
    public AudioClip Audio;
    public float AnimTime;
    public float DelayEffectTime;
}


public class PlayerAct : MonoBehaviour
{
    private static PlayerAct m_instance;
    public static PlayerAct instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<PlayerAct>();
            }

            return m_instance;
        }
    }

    [SerializeField] ActionMatch[] Actions;
    private Dictionary<PLAYERACTSTATE, ActionMatch> ActionDictionary;

    [SerializeField] private NavMeshAgent m_PlayerAgent;
    [SerializeField] private Animator m_PlayerAnimator;
    [SerializeField] private Rigidbody m_PlayerRigid;
    [SerializeField] private GameObject m_PlayerCharacter;
    [SerializeField] private PlayerAttackColliderController m_PC;
    Vector3 dir = Vector3.zero;
    //private bool m_bIsDead; //������ true ��������� false
    private bool m_bIsMove; //�����ϼ��ִ��������� bool����, ��ų�̳� ���ݵ� �ൿ�ÿ� �̵��� �������� 
    private bool m_bDoOnce; //Trigger �� �ݺ�ȣ����ϰԲ� Ʈ���Ÿ� �ѹ� ȣ���ϸ� false ȣ�������� true

    [SerializeField] private ParticleSystem ThunderAwakeEffect;
    [SerializeField] private ParticleSystem LevelUpEffect;
    [SerializeField] private AudioClip ThunderAwakeSound; //������ų �����Ҹ�
    [SerializeField] private AudioClip LevelUpSound; // ������Ҹ�
    [SerializeField] private PLAYERACTSTATE P_ActState; //�÷��̾��� �ൿ���� 
    private float m_flastAttackTime; // ������ ���� ����(���ݾִϸ��̼��� ������������ ī����)
    private float m_fAnimClock; //�ִϸ��̼� Ÿ�̸�

    public Vector3 movePoint; // �̵� ��ġ ����


    private void Awake()
    {
        if (instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }

       // m_PlayerAgent.speed
       //
       // = 5.0f;
        m_bIsMove = true;
      //  m_bIsDead = false;
        m_fAnimClock = 0;
        m_bDoOnce = true;
        P_ActState = PLAYERACTSTATE.FREE;
      

    }
    public void SetNewSpeed(float NewSpeed)
    {
        m_PlayerAgent.speed = NewSpeed;
    }
    private void Update()
    {
        PlayAction();
    }

    private void AnimTImer(float AnimTIme)
    {
        m_bIsMove = false;
        m_bDoOnce = false;
        m_PlayerAgent.isStopped = true;

        if ((m_fAnimClock += Time.deltaTime) >= AnimTIme)
        {
            m_fAnimClock = 0;
            P_ActState = PLAYERACTSTATE.FREE;
            m_bIsMove = true;
            m_bDoOnce = true;
            m_PlayerAgent.isStopped = false;
            FixTransform();
        }
    }

    private void FixTransform() //�ִϸ������� ApplyRootMotion �� ����ϸ� ĳ������ Transform Y��� Rot �� �̻������� ������ �����ϱ����� �ڵ�
    {
        m_PlayerCharacter.transform.localPosition = Vector3.zero;
    }
    
    public bool IsPlayerActByMouse()
    {
        if (UIManager.Instance.IsMouseOnUI)
            return true;

        if (UIManager.Instance.GetDraggedObject.IsDrag)
            return true;

        if (EventSystem.current.currentSelectedGameObject)
        {
            if (EventSystem.current.currentSelectedGameObject.tag == "UI")
                return true;
        }
        return false;
    }

    private void Move()
    {
        if (Input.GetMouseButton(1))
        {
            //���콺�� �ٸ� ����� �������϶� true �� �����Ͽ� �ش����� ������ �����ϴ�.
            if (IsPlayerActByMouse()) return;  

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                var MovePoint = raycastHit.point;
                m_PlayerAgent.SetDestination(MovePoint);
                UIManager.Instance.DrawGoPing(MovePoint);
                dir = (MovePoint - transform.position).normalized;
                dir.y = 0;
            }
        }

        //�÷��̾��� �̵��ִϸ��̼��� �����մϴ�. 0 = Idle, 0�̾ƴϸ� Walk
        m_PlayerAnimator.SetFloat("Velocity", m_PlayerAgent.velocity.magnitude); 
        //�÷��̾�ĳ���Ͱ� �ٶ󺸴� ������ �����մϴ�.
        if (Vector3.zero != dir)
            gameObject.transform.rotation = Quaternion.LookRotation(dir);
    }

    private void NormalAttackByMouse()
    {
        if (Input.GetMouseButton(0))
        {
            if (IsPlayerActByMouse()) return;
            if (SetActState(PLAYERACTSTATE.ATTACK))
                m_bIsMove = false;
        }
    }

    public bool IsPlayerCanAct() //�÷��̾ �����ο�������� �׸��� �����ϼ� �ִ��� ���θ������� �ൿ�� �Ҽ��ִ����� ���� �Ҹ������� ����
    {
        if (P_ActState == PLAYERACTSTATE.FREE && m_bIsMove == true)
            return true;
        else
            return false;
    }
    public bool SetActState(PLAYERACTSTATE NewAct)
    {
        if (P_ActState == PLAYERACTSTATE.FREE && m_bIsMove == true)
        {
            P_ActState = NewAct;
            return true;
        }
        return false;
    }

    IEnumerator DelayParticlePlay(float DelayTime, ParticleSystem Particle)
    {
        yield return new WaitForSeconds(DelayTime);

        Particle.Play();
        Particle.transform.parent = null;

        while (Particle.isPlaying) yield return null;

        Particle.transform.SetParent(transform);
        if(Particle.name == "ThunderHitEffect")
            Particle.transform.localPosition = new Vector3(0,0,2.5f);
        else
        Particle.transform.localPosition = Vector3.forward;
    }
    IEnumerator DelaySoundPlay(float DelayTime, AudioClip Audio)
    {
        yield return new WaitForSeconds(DelayTime);
        SoundManager.Instance.PlayEffectSoundForOnce(Audio);
    }

    public void LevelUp()
    {
        SoundManager.Instance.PlayEffectSoundForOnce(LevelUpSound);
        LevelUpEffect.Play();
    }

    public void Rebirth()
    {
        P_ActState = PLAYERACTSTATE.FREE;
        m_bIsMove = true;
    }

    public void SetPlayerIsAct(bool Is)
    {
        m_bIsMove = Is;
    }

    private void PlayAction()
    {
        for (int i = 0; i < Actions.Length; i++)
        {
            if (P_ActState == Actions[i].PlayerActState)
            {
                if (Actions[i].PlayerActState == PLAYERACTSTATE.FREE)
                {
                    Move();
                    NormalAttackByMouse();
                    break;
                }

                if (Actions[i].PlayerActState == PLAYERACTSTATE.DEAD)
                {
                    if (m_bDoOnce)
                    {
                        int DeadMotion = UnityEngine.Random.Range(0, 1);
                        m_PlayerAnimator.SetFloat("Dead", DeadMotion);
                        m_PlayerAnimator.SetTrigger("DeadTrigger");
                        SoundManager.Instance.PlayEffectSoundForOnce(Actions[i].Audio);
                        AnimTImer(3.0f);
                    }
                    break;
                }
                else
                {
                    if (m_bDoOnce)
                    {
                        string ActName = Actions[i].PlayerActState.ToString();
                        m_PlayerAnimator.SetTrigger(ActName);

                        if (Actions[i].PlayerActState == PLAYERACTSTATE.ATTACK)
                            m_PC.NormalAttack();
                        else
                            m_PC.UseSkill(ActName);

                        if (Actions[i].PlayerActState == PLAYERACTSTATE.THUNDER)
                        {
                            ThunderAwakeEffect.Play();
                            SoundManager.Instance.PlayEffectSoundForOnce(ThunderAwakeSound);
                        }

                        if (Actions[i].Particle)
                            StartCoroutine(DelayParticlePlay(Actions[i].DelayEffectTime, Actions[i].Particle));
                        if (Actions[i].Audio)
                            StartCoroutine(DelaySoundPlay(Actions[i].DelayEffectTime, Actions[i].Audio));
                    }
                    AnimTImer(Actions[i].AnimTime);
                }
            }
        }
    }
}