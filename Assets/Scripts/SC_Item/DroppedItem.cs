using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer m_ItemImage;
    [SerializeField] private int m_ItemNumber;
    [SerializeField] private int m_ItemQuantity;
    public event Action DropItemDisalbe; // �������� �԰ų� ���氡�ɽð��� ������ ������� ȣ��
    public void ItemDataInit(int newitemnumber, int Quantity, Vector3 tr) //������ ��ӽ� �����۵����� �Է¹���
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

    /*private void OnCollisionEnter(Collision collision) //�÷��̾ �ε����� �����۽���
    {
        Player.instance.PickupItem(m_ItemNumber, m_ItemQuantity);
        GameManager.instance.AddNewLog(DataTableManager.instance.GetItemData(m_ItemNumber).ItemName + " " + m_ItemQuantity.ToString() + "�� ȹ��!");
        OnDisableEvent();
    }*/

    private void OnTriggerEnter(Collider other) //�÷��̾ �ε����� �����۽���
    {
        if (other.tag == "Player")
        {
            Player.instance.PickupItem(m_ItemNumber, m_ItemQuantity);
            SoundManager.Instance.PlayPickUpSound();
            GameManager.instance.AddNewLog(DataTableManager.instance.GetItemData(m_ItemNumber).ItemName + " " + m_ItemQuantity.ToString() + "�� ȹ��!");
            OnDisableEvent();
        }
    }

    private void Awake()
    {
        DropItemDisalbe = null;
    }

    IEnumerator DisappeardByTime() //�����ð��� ������ �ڵ����� �������� �����
    {
        yield return new WaitForSeconds(GameManager.instance.ItemMaintainTime);
        OnDisableEvent();
    }
}
