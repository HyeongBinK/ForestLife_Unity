using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GoldItem : MonoBehaviour
{
    [SerializeField]private int m_Gold;
    public event Action GoldItemDisable;
    public void SetData(int Gold, Vector3 tr) //골드드랍시 골드량과 위치 셋팅
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
        private void OnCollisionEnter(Collision collision) //골드오브젝트에 캐릭터가 닿으면 자동습득
        {
            Player.instance.PickupGold(m_Gold);
            OnDisableEvent();
        }*/

    private void OnTriggerEnter(Collider other) //골드오브젝트에 캐릭터가 닿으면 자동습득
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

    IEnumerator DisappeardByTime() //일정시간이 지나면 자동으로 아이템이 사라짐
    {
        yield return new WaitForSeconds(GameManager.instance.ItemMaintainTime);
        OnDisableEvent();
    }
}
