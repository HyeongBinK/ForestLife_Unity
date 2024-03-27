using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;

public class SellSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image m_SellItemImage; //판매아이템의 이미지
    [SerializeField] private Text m_SellItemName; //판매아이템의 이름
    [SerializeField] private Text m_SellItemPrice; //판매아이템의 가격
    [SerializeField] private Button m_BuyButton; //구매버튼
    private ToolTip m_ToolTipBox; //마우스커서를 올렸을때 자세한 아이템의 효과설명을 위한 툴팁박스
    private bool m_IsData; //데이터가있는지없는지
    private string m_ToolTipText; //한번만 설정하면됨
    private int m_ItemUniqueNumber;
    private int m_Price; //가격
  
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

    public void OnPointerEnter(PointerEventData eventData) //마우스가 포인터(슬롯)에 들어왔을때 툴팁표시
    {
        if (m_IsData)
        {
            var Position = gameObject.transform.position;
            m_ToolTipBox.SetToolTipPosition(Position.x, Position.y);

            m_ToolTipBox.SetToolTipText(m_ToolTipText);
            m_ToolTipBox.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) //마우스가 포인터(슬롯)에서 멀어졌을떄 툴팁 비활성화
    {
        if (m_IsData)
        {
            m_ToolTipBox.gameObject.SetActive(false);
           
        }
    }
}
