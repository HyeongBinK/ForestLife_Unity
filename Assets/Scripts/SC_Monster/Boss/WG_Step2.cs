using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WG_Step2 : FSM<Boss_WG>
{
    private Boss_WG m_Owner;
    private float time = 0;
    private int Pattern = 0;
    public WG_Step2(Boss_WG _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        time = 0;
        m_Owner.m_eCurState = BOSSMOB_ACT.STEP2;
        
        m_Owner.StopNav();
        Pattern = Random.Range(0, 2);
        switch (Pattern)
        {
            case 0:
                m_Owner.OnTrigger("PowerSwing");
                m_Owner.ActPowerSwingRange();
                break;
            case 1:
                m_Owner.OnTrigger("UpperPunch");
                m_Owner.ActUpperPunchRange();
                break;
        }
    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = BOSSMOB_ACT.STEP2;
    }

    public override void Run()
    {
        bool Finish = false;
        switch (Pattern)
        {
            case 0:
                if (m_Owner.PowerSwing_AnimTime <= (time += Time.deltaTime))
                    Finish = true;
                break;
            case 1:
                if (m_Owner.UpperPunch_AnimTime <= (time += Time.deltaTime))
                    Finish = true;
                break;
        }

        if (Finish)
            m_Owner.ChangeFSM(BOSSMOB_ACT.CHASE);
    }
}
