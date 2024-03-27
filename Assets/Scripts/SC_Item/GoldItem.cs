using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GoldItem : MonoBehaviour
{
    [SerializeField]private int m_Gold;
    public event Action GoldItemDisable;
    public void SetData(int Gold, Vector3 tr) //������� ��差�� ��ġ ����
    {
        m_Gold = Gold;
        gameObject.transform.position = tr;
        
        if(!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            StartCoroutine(DisappeardByTime());
        }
    }
    public void OnDisableEvent()
    {
        if (GoldItemDisable != null)
        {
            GoldItemDisable();
            GoldItemDisable = null;
        }
        gameObject.SetActive(false);
    }
    /*
        private void OnCollisionEnter(Collision collision) //��������Ʈ�� ĳ���Ͱ� ������ �ڵ�����
        {
            Player.instance.PickupGold(m_Gold);
            OnDisableEvent();
        }*/

    private void OnTriggerEnter(Collider other) //��������Ʈ�� ĳ���Ͱ� ������ �ڵ�����
    {
        if (other.tag == "Player")
        {
            Player.instance.PickupGold(m_Gold);
            SoundManager.Instance.PlayPickUpSound();
            OnDisableEvent();
        }
    }

    private void Awake()
    {
        GoldItemDisable = null;
    }

    IEnumerator DisappeardByTime() //�����ð��� ������ �ڵ����� �������� �����
    {
        yield return new WaitForSeconds(GameManager.instance.ItemMaintainTime);
        OnDisableEvent();
    }
}
