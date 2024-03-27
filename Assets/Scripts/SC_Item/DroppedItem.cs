using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_ItemImage;
    [SerializeField] private int m_ItemNumber;
    [SerializeField] private int m_ItemQuantity;
    public event Action DropItemDisalbe; // 아이템을 먹거나 습득가능시간이 지나서 사라질시 호출
    public void ItemDataInit(int newitemnumber, int Quantity, Vector3 tr) //아이템 드롭시 아이템데이터 입력받음
    {
        if (DataTableManager.instance.GetItemData(newitemnumber) != null)
        {
            m_ItemNumber = newitemnumber;
            m_ItemQuantity = Quantity;
            var ItemData = DataTableManager.instance.GetItemData(m_ItemNumber);
            m_ItemImage.sprite = Resources.Load<Sprite>("UI/Item/" + ItemData.ImageName);
            gameObject.transform.position = tr;

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                StartCoroutine(DisappeardByTime());
            }
        }
    }
    public void OnDisableEvent()
    {
        if (DropItemDisalbe != null)
        {
            DropItemDisalbe();
            DropItemDisalbe = null;
        }
        gameObject.SetActive(false);
    }

    /*private void OnCollisionEnter(Collision collision) //플레이어에 부딛히면 아이템습득
    {
        Player.instance.PickupItem(m_ItemNumber, m_ItemQuantity);
        GameManager.instance.AddNewLog(DataTableManager.instance.GetItemData(m_ItemNumber).ItemName + " " + m_ItemQuantity.ToString() + "개 획득!");
        OnDisableEvent();
    }*/

    private void OnTriggerEnter(Collider other) //플레이어에 부딛히면 아이템습득
    {
        if (other.tag == "Player")
        {
            Player.instance.PickupItem(m_ItemNumber, m_ItemQuantity);
            SoundManager.Instance.PlayPickUpSound();
            GameManager.instance.AddNewLog(DataTableManager.instance.GetItemData(m_ItemNumber).ItemName + " " + m_ItemQuantity.ToString() + "개 획득!");
            OnDisableEvent();
        }
    }

    private void Awake()
    {
        DropItemDisalbe = null;
    }

    IEnumerator DisappeardByTime() //일정시간이 지나면 자동으로 아이템이 사라짐
    {
        yield return new WaitForSeconds(GameManager.instance.ItemMaintainTime);
        OnDisableEvent();
    }
}
