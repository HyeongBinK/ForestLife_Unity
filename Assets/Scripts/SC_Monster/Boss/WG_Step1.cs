using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WG_Step1 : FSM<Boss_WG>
{
    private Boss_WG m_Owner;
    private float time = 0;

    public WG_Step1(Boss_WG _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        time = 0;
        m_Owner.m_eCurState = BOSSMOB_ACT.STEP1;
        m_Owner.OnTrigger("PowerSwing");
        m_Owner.ActPowerSwingRange();
        m_Owner.StopNav();
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = BOSSMOB_ACT.STEP1;
    }

    public override void Run()
    {
        if (m_Owner.PowerSwing_AnimTime <= (time += Time.deltaTime))
        {
            m_Owner.ChangeFSM(BOSSMOB_ACT.CHASE);
        }
    }
}
