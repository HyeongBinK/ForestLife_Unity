using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SummonedAttack : MonoBehaviour
{
    private int m_iDammage; //��Ʈ�� �÷��̾� �ǰݽ� �ٵ�����(�������� ���� �޾ƿ�)
    public event Action Disappear; //�����ð��� ������ �߻��ϴ� �̺�Ʈ
    private bool m_bDamageOnce = false; //�÷��̾�� �������� �ѹ��� �ټ��ְԲ� ����ϱ�����(�ٴ���Ʈ����) ����
    private bool m_bDamageTimeoff = false; //��ȯ�� �����ð������� �÷��̾ �ε������������� ��������
    [SerializeField] private float m_iSummonedTime = 6.1f; //��ȯ�� �����Ǵ� �ð�

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
            GameManager.instance.AddNewLog("������ȯ��������!");
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
