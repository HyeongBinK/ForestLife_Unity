using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WG_Wait : FSM<Boss_WG>
{
    private Boss_WG m_Owner;
    private float time = 0;
    private bool WakeUp;
    public WG_Wait(Boss_WG _owner)
    {
        m_Owner = _owner;
    }
    public override void Begin()
    {
        WakeUp = false;
        time = 0;

        m_Owner.StopNav();
        m_Owner.m_eCurState = BOSSMOB_ACT.WAIT;

    }

    public override void Exit()
    {
        m_Owner.m_ePrevState = BOSSMOB_ACT.WAIT;
    }

    public override void Run()
    {
        if (m_Owner.m_bIsChase && !WakeUp)
        {
            WakeUp = true;
            m_Owner.PlayAwakeSound();
        }
        if (WakeUp)
        {
            m_Owner.OnTrigger("Battle");
            m_Owner.BGMChange(1);
            if (m_Owner.Wakeup_AnimTime <= (time += Time.deltaTime))
            {
                m_Owner.ChangeFSM(BOSSMOB_ACT.CHASE);
            }
        }  
        
    }
}
