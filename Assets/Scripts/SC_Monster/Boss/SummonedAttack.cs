using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SummonedAttack : MonoBehaviour
{
    private int m_iDammage; //루트에 플레이어 피격시 줄데미지(우드골렘으로 부터 받아옴)
    public event Action Disappear; //유지시간이 끝난후 발생하는 이벤트
    private bool m_bDamageOnce = false; //플레이어에게 데미지는 한번만 줄수있게끔 재어하기위한(다단히트억제) 변수
    private bool m_bDamageTimeoff = false; //소환후 일정시간지나면 플레이어가 부딛혀도데미지를 입지않음
    [SerializeField] private float m_iSummonedTime = 6.1f; //소환후 유지되는 시간

    public void Init(int NewDammage, Transform TargetTransform)
    {
        m_iDammage = NewDammage;
        gameObject.transform.position = TargetTransform.position;
    }

    public void ClearDisapper()
    {
        Disappear = null;
        m_bDamageOnce = false;
        m_bDamageTimeoff = false;
    }
  
    private void OnEnable()
    {
        StartCoroutine(Act());
        StartCoroutine(DamageDisable());
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player" && !m_bDamageOnce && !m_bDamageTimeoff)
        {
            Player.instance.GetDamage(m_iDammage);
            GameManager.instance.AddNewLog("바위소환공격적중!");
            m_bDamageOnce = true;
        }
    }

    private IEnumerator Act()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_iSummonedTime);
            if (Disappear != null)
            {
                Disappear();
            }
        }
    }

    IEnumerator DamageDisable()
    {
        yield return new WaitForSeconds(2.9f);
        if (m_bDamageTimeoff)
            m_bDamageTimeoff = true;
    }
}
