using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WG_Dead : FSM<Boss_WG>
{
    private Boss_WG m_Owner;
    private float time;

    public WG_Dead(Boss_WG _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        time = 0;
        m_Owner.OnDeath();
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = BOSSMOB_ACT.DEAD;
        m_Owner.BGMChange(4);
        m_Owner.SetHPBarDisable();
        m_Owner.DoDisable();
    }

    public override void Run()
    {
        if (m_Owner.DieAnimTime <= (time += Time.deltaTime))
        {
            Exit();
        }
    }
}
