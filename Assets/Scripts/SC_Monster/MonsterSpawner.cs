using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    private static MonsterSpawner m_instance; // 싱글톤이 할당될 변수
    // 싱글톤 접근용 프로퍼티
    public static MonsterSpawner instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<MonsterSpawner>();
            }
            return m_instance;
        }
    }

    [SerializeField] private NormalMonster MonsterPrefab1; // 생성할 적 AI1
    [SerializeField] private NormalMonster MonsterPrefab2; // 생성할 적 AI2
    [SerializeField] private Transform[] spawnPoints; // 적 AI를 소환할 위치들

    [SerializeField] private int MaxLevel; //랜덤스폰된 몬스터의 최대레밸
    [SerializeField] private int MinLevel; //랜덤스폰된 몬스터의 최소레밸

    [SerializeField] [Range(0, 100)] private int AnotherTypeSpawnRate; //타입이 다른 몬스터 스폰확률(범위 0 ~ 100)

    [SerializeField] private int MaxFieldMonster; //최대 몬스터 젠숫자
    [SerializeField] private int CurField; //현재 필드몬스터 숫자

    private List<NormalMonster> Monsters = new List<NormalMonster>(); // 생성된 적들을 담는 리스트
    private List<M_HPBar> OnEnabledHPBars = new List<M_HPBar>(); //생성된 체력바들을 담는 리스트
    private List<FloatingDamageText> OnEnabledDamageText = new List<FloatingDamageText>(); //생성된 데미지텍스트들을 담는 리스트

    private Dictionary<MONSTER_TYPE, Queue<NormalMonster>> monsterPooling = new Dictionary<MONSTER_TYPE, Queue<NormalMonster>>();
    [SerializeField] private float MonsterReGenTime = 5.0f;
    [SerializeField] private bool IsBossRoom; //보스룸이 true이면 보스가 잡몹소환스킬시전시에만 몬스터소환하고 입장시의 몬스터스폰과 정해진시간마다 리젠하는기능 off
    private GameObject TempTransformGameObject; //몬스터 스폰지역 랜덤을 위한 임시 위치좌표
   
    private void Init()
    {
        TempTransformGameObject = new GameObject();
        TempTransformGameObject.name = "랜덤위치좌표";

        for (int i = 0; i < MaxFieldMonster; i++)
        {
            MakeMonster(MonsterPrefab1); //몬스터 타입1
            MakeMonster(MonsterPrefab2); //몬스터 타입2
        }
    }

    private void Clear()
    {
        monsterPooling.Clear();
        Monsters.Clear();
    }
    private Transform RandomPoint()
    {
        if (0 >= spawnPoints.Length) return null;

        var index = Random.Range(0, spawnPoints.Length);
        int RandomNumber1 = Random.Range(-5, 5);
        int RandomNumber2 = Random.Range(-5, 5);

        var OriginSpawnZone = spawnPoints[index];
        Vector3 NewSpawnZone = new Vector3(OriginSpawnZone.transform.position.x + RandomNumber1, OriginSpawnZone.transform.position.y, OriginSpawnZone.transform.position.z + RandomNumber2);
        Transform temp = TempTransformGameObject.transform;

        temp.position = NewSpawnZone;
        temp.rotation = OriginSpawnZone.rotation;
        return temp;
    }
    private int RandomLevel()
    {
        return Random.Range(MinLevel, MaxLevel) + 1;
    }

    private void MakeMonster(NormalMonster prefab)
    {
        if (prefab && MONSTER_TYPE.NONE != prefab.GetMonsterType)
        {
            NormalMonster NewMonster = Instantiate(prefab);
            if (!monsterPooling.ContainsKey(prefab.GetMonsterType))
            {
                monsterPooling.Add(prefab.GetMonsterType, new Queue<NormalMonster>());
            }
            monsterPooling[prefab.GetMonsterType].Enqueue(NewMonster);
            NewMonster.gameObject.SetActive(false);
        }
    }
    
    private NormalMonster GetMonster(MONSTER_TYPE type)
    {
        if (monsterPooling.ContainsKey(type))
        {
            return monsterPooling[type].Dequeue();
        }
        return null;
    }

    private bool CreateMonster()
    {
        if (CurField < MaxFieldMonster)
        {
            NormalMonster monster = null;
          
            int RandomRate = Random.Range(0, 100); //랜덤 숫자 생성
            if (RandomRate >= AnotherTypeSpawnRate) //스폰확률 비교
            {
                monster = GetMonster(MonsterPrefab1.GetMonsterType); //풀링된거에서 Dequeue 해서 가져옴
            }
            else
            {
                monster = GetMonster(MonsterPrefab2.GetMonsterType);
            }

            if (monster)
            {
                var tr = RandomPoint();
                if (tr)
                {
                    monster.Init(RandomLevel(), tr); //몬스터의 LV(LV 설정시 능력치 자동설정, 소환위치 설정)
                    monster.gameObject.SetActive(true); //몬스터 활성화
                    M_HPBar hpbar = UIManager.Instance.GetHPBar(); 
                    hpbar.SetSlider(monster.MaxHP);
                    FloatingDamageText damageText = UIManager.Instance.GetDamageText();
                    CurField++;
                    monster.SetHPBar += (curhp) => hpbar.SetValue(curhp);
                    monster.SetDamageText += (damage) => damageText.SetText(damage.ToString());
                    
                    monster.OnDeath += () => monster.ClearDeadEvent();
                    monster.OnDeath += () => Monsters.Remove(monster);
                    monster.OnDeath += () => monsterPooling[monster.GetMonsterType].Enqueue(monster);
                    monster.OnDeath += () => CurField--;

                    //monster.OnDeath += () => OnEnabledHPBars.Remove(hpbar);
                    monster.OnDeath += () => hpbar.OnDisableEvent();
                    monster.OnDeath += () => OnEnabledHPBars.Remove(hpbar);

                    monster.OnDeath += () => OnEnabledDamageText.Remove(damageText);
                    monster.OnDeath += () => damageText.OnDisableEvent();
                    monster.OnDeath += () => damageText.gameObject.SetActive(false);
                    
                    Monsters.Add(monster);
                    OnEnabledHPBars.Add(hpbar);
                    OnEnabledDamageText.Add(damageText);
                }
            }

            return true;
        }

        return false;
    }
    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }

        Init();
        if (!IsBossRoom)
        {
            for (int i = 0; i < MaxFieldMonster; i++)
            {
                if (!CreateMonster()) break;
            }
        }
    }

    private void Start()
    {
        if(!IsBossRoom)
        StartCoroutine(MonsterReGen());
        StartCoroutine(MatchHPBarAndDamageTextToMonster());
    }

    private IEnumerator MonsterReGen()
    {
        while (true)
        {
            for (int i = 0; i < MaxFieldMonster; i++)
            {
                if (!CreateMonster()) break;
            }

            yield return new WaitForSeconds(MonsterReGenTime);
        }
    }

    private IEnumerator MatchHPBarAndDamageTextToMonster()
    {
        while (true)
        {
            for (int i = 0; i < Monsters.Count; i++)
            {
                var RegionMonster = Monsters[i];
                OnEnabledHPBars[i].SetPos(Camera.main.WorldToScreenPoint(RegionMonster.transform.position + new Vector3(0, RegionMonster.HpBarYPlusValue, 0)));
                OnEnabledDamageText[i].SetPos(Camera.main.WorldToScreenPoint(RegionMonster.transform.position + new Vector3(0, RegionMonster.FloatingDamageText_YPlusValue, 0)));
            }

            yield return null;
        }
    }
    public void SummonMonster(int Number)
    {
        for (int i = 0; i < Number; i++)
        {
            if (!CreateMonster()) break;
        }
    }

    public void ClearField()
    {
        int LoopTime = Monsters.Count;
        for (int i =0; i < Monsters.Count; i++)
        {
            Monsters[i].gameObject.SetActive(false);
        }

        LoopTime = OnEnabledHPBars.Count;
        for (int i = 0; i < LoopTime; i ++)
        {
            OnEnabledHPBars[i].gameObject.SetActive(false);
            UIManager.Instance.WithDrawHPBar(OnEnabledHPBars[i]);
        }

        LoopTime = OnEnabledDamageText.Count;
        for (int i = 0; i < LoopTime; i++)
        {
            OnEnabledDamageText[i].gameObject.SetActive(false);
            UIManager.Instance.WithDrawDamageText(OnEnabledDamageText[i]);
        }

        Monsters.Clear();
        OnEnabledHPBars.Clear();
        OnEnabledDamageText.Clear();
        TempTransformGameObject = null;
    }
}
