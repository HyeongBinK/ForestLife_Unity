using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsSellButton : MonoBehaviour
{
    [SerializeField] private InputSellQuantity HowMuchToSell; //얼마나 팔지 입력하는 인풋오브젝트
    [SerializeField] private Button SellButton; //누르면 얼마나판매할지 입력하는 오브젝트
    [SerializeField] private Button StopButton; //누르면 창을닫음
    private int m_OriginSlotNumber; //판매의대상이된 슬롯번호 
    private ITEM_TYPE m_type;
    private int m_Quantity;

    private void Awake()
    {
        ButtonConnect();
        m_OriginSlotNumber = -1;
    }

    public void ButtonConnect()
    {
        SellButton.onClick.AddListener(SellButtonOnClickHandler);
        StopButton.onClick.AddListener(DisActiveUI);
    }

    public void SetScreenPosition(float ScreenX, float ScreenY)
    {
        gameObject.transform.position = new Vector2(ScreenX, ScreenY);
    }

    public void SetOriginSlotNumber(int NewOriginSlotNumber, ITEM_TYPE Type, int Quantity)
    {
        m_OriginSlotNumber = NewOriginSlotNumber;
        m_type = Type;
        m_Quantity = Quantity;
    }

    public void SellButtonOnClickHandler()
    {
        if (m_OriginSlotNumber != -1)
        {
            if(m_type == ITEM_TYPE.EQUIPMENT || m_Quantity == 1)
            {
                Player.instance.SellItem(m_OriginSlotNumber, 1);
                DisActiveUI();
                return;
            }

            HowMuchToSell.SetData(m_OriginSlotNumber, m_Quantity);
            HowMuchToSell.SetScreenPosition(gameObject.transform.position.x, gameObject.transform.position.y);
        }
        DisActiveUI();
    }

    public void DisActiveUI() //해당 UI 비활성화
    {
        gameObject.SetActive(false);
    }

}
