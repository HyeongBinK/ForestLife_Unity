using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour
{
    [SerializeField] private List<int> m_SellItemList; //판매할 아이템리스트(아이템고유번호로 결정)
    private List<SellSlot> m_SellSlotList = new List<SellSlot>(); //아이템 판매슬롯들이 담길 리스트
    [SerializeField] private SellSlot m_SellSlotPrefab; //셀슬롯 프리팹
    [SerializeField] private Transform m_SellSlotsPosition; //슬롯들이 담길위치
    [SerializeField] private Button CloseButton; //닫기버튼


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
