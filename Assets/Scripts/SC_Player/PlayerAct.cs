
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum PLAYERACTSTATE
{
    FREE = 0, // 기본상태(이상태가 아니면 스킬사용불가)
    ATTACK, //기본공격
    ROLLING, //구르기(회피상태로) 앞으로일정거리이동및 무적상태
    POWERSWING, //파워스윙스킬
    EARTHQUAKE, //지진스킬
    THUNDER, //번개스킬
    STRUN, //기절상태이상
    DEAD //사망
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
    //private bool m_bIsDead; //죽으면 true 살아있으면 false
    private bool m_bIsMove; //움직일수있는지에대한 bool변수, 스킬이나 공격등 행동시엔 이동이 막힐예정 
    private bool m_bDoOnce; //Trigger 를 반복호출안하게끔 트리거를 한번 호출하면 false 호출이전엔 true

    [SerializeField] private ParticleSystem ThunderAwakeEffect;
    [SerializeField] private ParticleSystem LevelUpEffect;
    [SerializeField] private AudioClip ThunderAwakeSound; //번개스킬 시전소리
    [SerializeField] private AudioClip LevelUpSound; // 레밸업소리
    [SerializeField] private PLAYERACTSTATE P_ActState; //플레이어의 행동상태 
    private float m_flastAttackTime; // 마지막 공격 시점(공격애니메이션이 끝난시점부터 카운팅)
    private float m_fAnimClock; //애니메이션 타이머

    public Vector3 movePoint; // 이동 위치 저장


    private void Awake()
    {
        if (instance != this)
        {
            // 자신을 파괴
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

    private void FixTransform() //애니메이터의 ApplyRootMotion 을 사용하면 캐릭터의 Transform Y축과 Rot 가 이상해지는 현상을 수정하기위한 코드
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
            //마우스로 다른 기능이 실행중일때 true 를 리턴하여 해당기능의 실행을 막습니다.
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

        //플레이어의 이동애니메이션을 조절합니다. 0 = Idle, 0이아니면 Walk
        m_PlayerAnimator.SetFloat("Velocity", m_PlayerAgent.velocity.magnitude); 
        //플레이어캐릭터가 바라보는 방향을 조절합니다.
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

    public bool IsPlayerCanAct() //플레이어가 자유로운상태인지 그리고 움직일수 있는지 여부를가지고 행동을 할수있는지에 대한 불린변수를 리턴
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