using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SellSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image m_SellItemImage; //�Ǹž������� �̹���
    [SerializeField] private Text m_SellItemName; //�Ǹž������� �̸�
    [SerializeField] private Text m_SellItemPrice; //�Ǹž������� ����
    [SerializeField] private Button m_BuyButton; //���Ź�ư
    private ToolTip m_ToolTipBox; //���콺Ŀ���� �÷����� �ڼ��� �������� ȿ�������� ���� �����ڽ�
    private bool m_IsData; //�����Ͱ��ִ���������
    private string m_ToolTipText; //�ѹ��� �����ϸ��
    private int m_ItemUniqueNumber;
    private int m_Price; //����
  
    private void Awake()
    {
        ClickBuyButton();
    }

    public void SetData(int ItemUniqueNumber)
    {
        m_ItemUniqueNumber = ItemUniqueNumber;
        var ItemData = DataTableManager.instance.GetItemData(m_ItemUniqueNumber);
        m_SellItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
        m_SellItemName.text = ItemData.ItemName;
        m_Price = ItemData.Price;
        m_SellItemPrice.text = m_Price.ToString();
        m_ToolTipBox = UIManager.Instance.GetToolTipBox;
        m_ToolTipText = ItemManager.instance.MakeItemToolTipByUniqueNumber(m_ItemUniqueNumber);
        m_IsData = true;
    }

    public void ClickBuyButton()
    {
        m_BuyButton.onClick.AddListener(BuyButtonOnClickHandler);
    }

    public void BuyButtonOnClickHandler()
    {
        if (m_IsData)
        {
            Player.instance.BuyItem(m_ItemUniqueNumber, 1);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) //���콺�� ������(����)�� �������� ����ǥ��
    {
        if (m_IsData)
        {
            var Position = gameObject.transform.position;
            m_ToolTipBox.SetToolTipPosition(Position.x, Position.y);

            m_ToolTipBox.SetToolTipText(m_ToolTipText);
            m_ToolTipBox.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //���콺�� ������(����)���� �־������� ���� ��Ȱ��ȭ
    {
        if (m_IsData)
        {
            m_ToolTipBox.gameObject.SetActive(false);
           
        }
    }
}
