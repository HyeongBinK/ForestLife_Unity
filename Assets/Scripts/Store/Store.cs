using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
    [SerializeField] private List<int> m_SellItemList; //�Ǹ��� �����۸���Ʈ(�����۰�����ȣ�� ����)
    private List<SellSlot> m_SellSlotList = new List<SellSlot>(); //������ �ǸŽ��Ե��� ��� ����Ʈ
    [SerializeField] private SellSlot m_SellSlotPrefab; //������ ������
    [SerializeField] private Transform m_SellSlotsPosition; //���Ե��� �����ġ
    [SerializeField] private Button CloseButton; //�ݱ��ư


    private void Awake()
    {
        SetSellList();
        CloseButton.onClick.AddListener(CloseStoreUI);
    }

    public void SetSellList()
    {
        if(m_SellItemList.Count > 0)
        {
            for (int i = 0; i < m_SellItemList.Count; i++)
            {
                SellSlot NewSellSlot = Instantiate(m_SellSlotPrefab, m_SellSlotsPosition);
                NewSellSlot.name = i.ToString();
                NewSellSlot.SetData(m_SellItemList[i]);
                m_SellSlotList.Add(NewSellSlot);
            }
        }
    }

    public void CloseStoreUI()
    {
        gameObject.SetActive(false);
        Player.instance.SetIsStoreUi(false);
        UIManager.Instance.InputInventoryKey();
    }
}
