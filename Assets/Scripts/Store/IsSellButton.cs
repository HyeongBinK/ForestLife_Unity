using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsSellButton : MonoBehaviour
{
    [SerializeField] private InputSellQuantity HowMuchToSell; //�󸶳� ���� �Է��ϴ� ��ǲ������Ʈ
    [SerializeField] private Button SellButton; //������ �󸶳��Ǹ����� �Է��ϴ� ������Ʈ
    [SerializeField] private Button StopButton; //������ â������
    private int m_OriginSlotNumber; //�Ǹ��Ǵ���̵� ���Թ�ȣ 
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

    public void DisActiveUI() //�ش� UI ��Ȱ��ȭ
    {
        gameObject.SetActive(false);
    }

}
