using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCTYPE
{
    NONE = -1,
    CONSUMPTIONSTORENPC,
    WEAPONSTORENPC,
    QUESTNPC,
    REINFORCENPC,
    END
}


public class StoreNPC : MonoBehaviour
{
    [SerializeField] private NPCTYPE m_NpcType;
    private readonly float range = 7; //�÷��̾� ��ġ����
    public float m_fDistance { get; private set; } //�÷��̾���� �Ÿ�
    private Transform TargetObject; //������ Ÿ��
    public bool m_bIsPlayerClose { get { return (range >= m_fDistance); } } //�����Ÿ��ȿ� �÷��̾ �ٰ��ý� ���º���
    private bool m_bIsStoreOpen = false;
    [SerializeField] private float ChatCloud_X_Offset = -2.2f;
    [SerializeField] private float ChatCloud_Y_Offset = 2.4f;

    // [SerializeField] private string HelloChat; //�÷��̾ �ٰ��ý� ���� ����

    private void Awake()
    {
        TargetObject = GameManager.instance.playerobject.transform;
        StartCoroutine(SetDistance());
    }

    IEnumerator SetDistance()
    {
        while (true)
        {
            m_fDistance = Vector3.Distance(TargetObject.position, transform.position);
            var ChatCloud = UIManager.Instance.GetNpcChatCloud;
            if (ChatCloud)
            {
                if (m_bIsPlayerClose && !m_bIsStoreOpen)
                    ChatCloud.SetChatCloud(NpcScreenPosition().x, NpcScreenPosition().y);
                else
                    ChatCloud.gameObject.SetActive(false);
                yield return null;
                //yield return new WaitForSeconds(0.1f);
            }
        } 
    }

    private Vector2 NpcScreenPosition()
    {
        Vector3 NPCWorldLocation = gameObject.transform.position + new Vector3(ChatCloud_X_Offset, ChatCloud_Y_Offset, 0);
        Vector2 NPCScreenLocation = Camera.main.WorldToScreenPoint(NPCWorldLocation);

        return NPCScreenLocation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Player.instance.OpenStore(m_NpcType))
                m_bIsStoreOpen = true;
        }
    }
}
