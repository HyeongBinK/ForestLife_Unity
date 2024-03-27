using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WG_Chase : FSM<Boss_WG>
{
    private Boss_WG m_Owner;
   

    public WG_Chase(Boss_WG _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        m_Owner.StartNav();
        m_Owner.m_eCurState = BOSSMOB_ACT.CHASE;
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = BOSSMOB_ACT.CHASE;
    }

    public override void Run()
    {
        m_Owner.Chase();
    }

   
}
   
